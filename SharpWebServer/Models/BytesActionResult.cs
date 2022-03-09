using SharpWebServer.Interfaces;
using System.Net;

namespace SharpWebServer.Models;

public class BytesActionResult : IActionResult
{
    public int StatusCode { get; }
    public object? Content { get; }
    public bool HasContent => Content != null;

    public BytesActionResult(int statusCode, byte[]? returnBytes = null)
    {
        StatusCode = statusCode;
        Content = returnBytes;
    }

    public void WriteStreamContent(HttpListenerResponse? res)
    {
        if (res == null) return;
        if (Content == null) return;

        var content = (byte[]) Content;

        res.ContentLength64 = content.Length;

        res.OutputStream.Write(content);
    }
}