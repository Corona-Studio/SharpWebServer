using System.Net;

namespace SharpWebServer.Interfaces;

public interface IActionResult
{
    int StatusCode { get; }
    bool HasContent { get; }
    object? Content { get; }

    void WriteStreamContent(HttpListenerResponse? res);
}