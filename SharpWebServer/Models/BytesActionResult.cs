using SharpWebServer.Interfaces;

namespace SharpWebServer.Models;

public class BytesActionResult : IActionResult
{
    public int StatusCode { get; }
    public object? Content { get; }

    public BytesActionResult(int statusCode, byte[]? returnBytes = null)
    {
        StatusCode = statusCode;
        Content = returnBytes;
    }

    public void WriteStreamContent(Stream stream)
    {
        if (Content == null) return;

        stream.Write(((byte[]) Content).AsSpan());
    }
}