using System.Net;
using System.Text.Json;
using SharpWebServer.Interfaces;

namespace SharpWebServer.Models;

public class ActionResult : IActionResult
{
    public ActionResult(int statusCode, object? returnContent = null)
    {
        StatusCode = statusCode;
        Content = returnContent;
    }

    public int StatusCode { get; }
    public object? Content { get; }
    public bool HasContent => Content != null;

    public void WriteStreamContent(HttpListenerResponse? res)
    {
        if (res == null) return;
        if (Content == null) return;

        var bytes = JsonSerializer.SerializeToUtf8Bytes(Content);

        res.ContentLength64 = bytes.Length;

        res.OutputStream.WriteAsync(bytes);
    }
}