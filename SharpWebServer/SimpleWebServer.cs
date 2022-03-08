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
    private readonly ConcurrentBag<IHandler> _handlers = new();
    private readonly ConcurrentDictionary<IController, string> _controllers = new();

    private readonly Thread _runThread;
    private readonly HttpListener _listener;

    public string Prefix { get; }

    public SimpleWebServer(string prefix)
    {
        if(string.IsNullOrEmpty(prefix))
            throw new ArgumentNullException(nameof(prefix));

        Prefix = prefix;

        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);

        _runThread = new Thread(StartInternal)
        {
            IsBackground = true
        };
    }

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

        var apiRoot = attribute.ApiRoot.TrimStart('/');

        _controllers.AddOrUpdate(Activator.CreateInstance<T>(), apiRoot, (_, s) => s);
    }

    public void UnRegisterController<T>() where T : IController
    {
        var controller = _controllers.Keys.FirstOrDefault(c => c.GetType() == typeof(T));

        if (controller == default) return;

        _controllers.TryRemove(controller, out _);
    }

    public bool GetController<T>(out IController? controller) where T : IController
    {
        var result = _controllers.Keys.FirstOrDefault(c => c.GetType() == typeof(T));

        controller = result;
        return result != default;
    }

    public void Start()
    {
        _listener.Start();
        _runThread.Start();
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
            try
            {
                var context = _listener.GetContext();
                var req = context.Request;
                var res = context.Response;
                var reqPath = req.Url?.AbsolutePath ?? "/";

#if DEBUG
                Console.WriteLine(reqPath);
#endif

                var availableControllers = _controllers
                    .Where(p => reqPath.StartsWith(p.Value, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Key)
                    .ToList();

                foreach (var handler in _handlers.Where(h => h is not IErrorHandler))
                {
                    handler.Handle(context);
                }
                
                if (!availableControllers.Any())
                {
                    HandleErrorInternal(context, res);
                    continue;
                }

                foreach (var controller in availableControllers)
                {
                    var controllerMethods = controller.GetType().GetMethods()
                        .Where(m => m.GetCustomAttributes().Any(a => a is HttpAttribute))
                        .ToList();

                    foreach (var method in controllerMethods)
                    {
                        var attribute = (HttpAttribute)method.GetCustomAttributes(typeof(HttpAttribute)).First();
                        var attributePath = attribute.Path ?? "/";
                        var apiRoot = _controllers[controller];

                        if (!attribute.Method.Equals(req.HttpMethod, StringComparison.OrdinalIgnoreCase))
                        {
                            HandleErrorInternal(context, res);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(attribute.RequestContentType) &&
                            !(req.ContentType?.Equals(attribute.RequestContentType,
                                StringComparison.OrdinalIgnoreCase) ?? false))
                        {
                            HandleErrorInternal(context, res);
                            continue;
                        }

                        var pathMatchPattern = $"{apiRoot}/{Regex.Replace(attributePath.TrimStart('/'), "\\{(\\w+)\\}", "(\\w+)")}";

                        if (!Regex.IsMatch(reqPath, pathMatchPattern))
                        {
                            HandleErrorInternal(context, res);
                            continue;
                        }

                        using var reqStream = req.InputStream;
                        using var reqStreamReader = new StreamReader(reqStream, Encoding.UTF8);

                        var reqContent = reqStreamReader.ReadToEnd();

                        var routeMatchDic = UrlHelper.GetRouteDic(reqPath, attributePath);
                        var queryDic = HttpUtility.ParseQueryString(req.Url?.Query ?? string.Empty);
                        var formDic = HttpUtility.ParseQueryString(reqContent);

                        var parameters = new List<object?>();

                        foreach (var para in method.GetParameters())
                        {
                            var fromAttribute = para.GetCustomAttribute<FromAttribute>() ?? new FromQueryAttribute();

                            switch (fromAttribute.From)
                            {
                                case FromType.Route:
                                    if (!routeMatchDic.TryGetValue(para.Name!, out var val) || string.IsNullOrEmpty(val))
                                    {
                                        if(!para.HasDefaultValue)
                                            parameters.Add(null);

                                        break;
                                    }

                                    parameters.Add(Convert.ChangeType(val, para.ParameterType));
                                    break;
                                case FromType.Query:
                                    if (string.IsNullOrEmpty(queryDic.Get(para.Name!)))
                                    {
                                        if (!para.HasDefaultValue)
                                            parameters.Add(null);

                                        break;
                                    }

                                    parameters.Add(Convert.ChangeType(queryDic.Get(para.Name!), para.ParameterType));
                                    break;
                                case FromType.Form:
                                    if (req.ContentType != "application/x-www-form-urlencoded" ||
                                        string.IsNullOrEmpty(formDic.Get(para.Name!)))
                                    {
                                        if (!para.HasDefaultValue)
                                            parameters.Add(null);

                                        break;
                                    }

                                    parameters.Add(Convert.ChangeType(formDic.Get(para.Name!), para.ParameterType));
                                    break;
                                case FromType.Body:
                                    if (req.ContentLength64 == 0)
                                    {
                                        if (!para.HasDefaultValue)
                                            parameters.Add(null);

                                        break;
                                    }

                                    parameters.Add(JsonSerializer.Deserialize(reqContent, para.ParameterType));
                                    break;
                            }
                        }

                        var returnValue = method.Invoke(controller, parameters.ToArray());

                        if (returnValue is IActionResult acResult)
                        {
                            using var resWriter = new StreamWriter(res.OutputStream, Encoding.UTF8);

                            res.StatusCode = acResult.StatusCode;
                            res.ContentType = attribute.ResponseContentType;

                            acResult.WriteStreamContent(res.OutputStream);
                        }
                        else
                        {
                            res.StatusCode = 200;
                        }

                        res.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Thread.Sleep(5);
            }
        }
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
}