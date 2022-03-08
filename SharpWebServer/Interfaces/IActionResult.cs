namespace SharpWebServer.Interfaces;

public interface IActionResult
{
    int StatusCode { get; }
    object? Content { get; }

    void WriteStreamContent(Stream stream);
}