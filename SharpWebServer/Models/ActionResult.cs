using SharpWebServer.Interfaces;
using System.Text.Json;

namespace SharpWebServer.Models;

public class ActionResult : IActionResult
{
    public int StatusCode { get; }
    public object? Content { get; }
    
    public ActionResult(int statusCode, object? returnContent = null)
    {
        StatusCode = statusCode;
        Content = returnContent;
    }

    public void WriteStreamContent(Stream? stream)
    {
        if (stream == null) return;
        if (Content == null) return;

        JsonSerializer.Serialize(stream, Content);
    }
}