using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using SharpWebServer.Attributes;
using SharpWebServer.Attributes.FromAttributes;
using SharpWebServer.Helpers;
using SharpWebServer.Interfaces;
using SharpWebServer.Models;

namespace SharpWebServer;

public class SimpleWebServer : IWebServer
{
    private readonly ConcurrentDictionary<IController, string> _controllers = new();
    private readonly ConcurrentBag<IHandler> _handlers = new();
    private readonly HttpListener _listener;

    public SimpleWebServer(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            throw new ArgumentNullException(nameof(prefix));

        Prefix = prefix;

        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
    }

    public string Prefix { get; }

    public void RegisterHandler<T>() where T : IHandler
    {
        _handlers.Add(Activator.CreateInstance<T>());
    }

    public void RegisterController<T>() where T : IController
    {
        var apiControllerAttribute =
            typeof(T).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();

        if (apiControllerAttribute is not ApiControllerAttribute attribute)
            throw new InvalidDataException(nameof(T));

        var apiRoot = attribute.ApiRoot;

        _controllers.AddOrUpdate(Activator.CreateInstance<T>(), apiRoot, (_, s) => s);
    }

    public void UnRegisterController<T>() where T : IController
    {
        var controller = _controllers.Keys.FirstOrDefault(c => c.GetType() == typeof(T));

        if (controller == default) return;

        _controllers.TryRemove(controller, out _);
    }

    public bool GetController<T>(out T? controller) where T : IController
    {
        var result = _controllers.Keys.FirstOrDefault(c => c.GetType() == typeof(T));

        if (result != default)
            controller = (T)result;
        else
            controller = default;

        return result != default;
    }

    public void Start()
    {
        _listener.Start();
        Task.Run(StartInternal);
    }

    public void Stop()
    {
        _listener.Stop();
        _listener.Close();
    }

    public void Dispose()
    {
        Stop();
    }

    private void HandleErrorInternal(HttpListenerContext context, HttpListenerResponse res)
    {
        var errorHandler = _handlers.FirstOrDefault(h => h is IErrorHandler);
        if (errorHandler != null)
        {
            errorHandler.Handle(context);
            res.Close();
            return;
        }

        res.StatusCode = 404;
        res.ContentLength64 = 0;

        res.Close();
    }

    private void StartInternal()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext? context = null;
            HttpListenerResponse? res = null;

            try
            {
                context = _listener.GetContext();
                var req = context.Request;
                res = context.Response;
                var reqPath = req.Url?.AbsolutePath ?? "/";
                reqPath = reqPath.Length != 1 ? reqPath.TrimEnd('/') : reqPath;

#if DEBUG
                Console.WriteLine(reqPath);
#endif

                var availableControllers = _controllers
                    .Where(p => reqPath.StartsWith(p.Value, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Key)
                    .ToList();

                foreach (var handler in _handlers.Where(h => h is not IErrorHandler)) handler.Handle(context);

                if (!availableControllers.Any())
                {
                    HandleErrorInternal(context, res);
                    continue;
                }

                var handled = false;

                foreach (var controller in availableControllers)
                {
                    if (handled) break;

                    var controllerMethods = controller.GetType().GetMethods()
                        .Where(m => m.GetCustomAttributes(typeof(HttpAttribute)).FirstOrDefault() != default)
                        .ToList();

                    foreach (var method in controllerMethods)
                    {
                        var attribute = (HttpAttribute)method.GetCustomAttributes(typeof(HttpAttribute)).First();
                        var attributePath = attribute.Path ?? "/";

                        if (!attribute.Method.Equals(req.HttpMethod, StringComparison.OrdinalIgnoreCase)) continue;

                        if (!string.IsNullOrEmpty(attribute.RequestContentType) &&
                            !(req.ContentType?.Equals(attribute.RequestContentType,
                                StringComparison.OrdinalIgnoreCase) ?? false))
                            continue;


                        var attributePathTrimmed =
                            attributePath.Length != 1 ? attributePath.TrimStart('/') : attributePath;
                        var hasReplaceablePattern = Regex.IsMatch(attributePath, "\\{(\\w+)\\}");

                        var pathMatchPattern = hasReplaceablePattern
                            ? $"/{Regex.Replace(attributePathTrimmed, "\\{(\\w+)\\}", "(\\w+)")}"
                            : attributePath;
                        pathMatchPattern = pathMatchPattern.Length != 1
                            ? pathMatchPattern.TrimEnd('/')
                            : pathMatchPattern;

                        var reqPathTrimmed = $"/{reqPath[_controllers[controller].Length..].TrimStart('/')}";

                        if (!hasReplaceablePattern)
                        {
                            if (!reqPathTrimmed.Equals(pathMatchPattern, StringComparison.OrdinalIgnoreCase)) continue;
                        }
                        else
                        {
                            var match = Regex.Match(reqPathTrimmed, pathMatchPattern);
                            if (!match.Success || match.Index != 0) continue;
                        }

                        using var reqStream = req.InputStream;
                        using var reqStreamReader = new StreamReader(reqStream, Encoding.UTF8);

                        var reqContent = reqStreamReader.ReadToEnd();

                        var routeMatchDic = UrlHelper.GetRouteDic(reqPathTrimmed, attributePath);
                        var queryDic = HttpUtility.ParseQueryString(req.Url?.Query ?? string.Empty);
                        var formDic = HttpUtility.ParseQueryString(reqContent);

                        var parameters = new List<object?>();

                        foreach (var para in method.GetParameters())
                        {
                            var fromAttribute = para.GetCustomAttribute<FromAttribute>() ?? new FromQueryAttribute();

                            switch (fromAttribute.From)
                            {
                                case FromType.Query:
                                    if (string.IsNullOrEmpty(queryDic.Get(para.Name!)))
                                    {
                                        parameters.Add(!para.HasDefaultValue ? null : para.DefaultValue);
                                        break;
                                    }

                                    parameters.Add(Convert.ChangeType(queryDic.Get(para.Name!), para.ParameterType));
                                    break;
                                case FromType.Form:
                                    if (req.ContentType != "application/x-www-form-urlencoded" ||
                                        string.IsNullOrEmpty(formDic.Get(para.Name!)))
                                    {
                                        parameters.Add(!para.HasDefaultValue ? null : para.DefaultValue);
                                        break;
                                    }

                                    parameters.Add(Convert.ChangeType(formDic.Get(para.Name!), para.ParameterType));
                                    break;
                                case FromType.Body:
                                    if (req.ContentLength64 == 0)
                                    {
                                        parameters.Add(!para.HasDefaultValue ? null : para.DefaultValue);
                                        break;
                                    }

                                    parameters.Add(JsonSerializer.Deserialize(reqContent, para.ParameterType));
                                    break;
                                default:
                                case FromType.Route:
                                    if (!routeMatchDic.TryGetValue(para.Name!, out var val) ||
                                        string.IsNullOrEmpty(val))
                                    {
                                        parameters.Add(!para.HasDefaultValue ? null : para.DefaultValue);
                                        break;
                                    }

                                    parameters.Add(Convert.ChangeType(val, para.ParameterType));
                                    break;
                            }
                        }

                        var returnValue = method.Invoke(controller, parameters.ToArray());

                        if (returnValue is IActionResult acResult)
                        {
                            res.StatusCode = acResult.StatusCode;

                            if (acResult.HasContent)
                            {
                                res.ContentType = attribute.ResponseContentType;
                                acResult.WriteStreamContent(res);
                            }
                        }
                        else
                        {
                            res.StatusCode = 200;
                        }

                        handled = true;
                        res.Close();

                        break;
                    }
                }

                if (!handled)
                {
                    HandleErrorInternal(context, res);
                    continue;
                }

                res.Close();
            }
            catch (Exception e)
            {
                if (context == null || res == null) return;

                HandleErrorInternal(context, res);
                Console.WriteLine(e);
            }
        }
    }
}