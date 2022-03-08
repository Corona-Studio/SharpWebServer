using System.Net;
using System.Text.Json;
using SharpWebServer.Interfaces;

namespace SharpWebServerDemo;

public class ErrorHandler : IErrorHandler
{
    public void Handle(HttpListenerContext context)
    {
        context.Response.StatusCode = 404;
        JsonSerializer.Serialize(context.Response.OutputStream, new
        {
            info = "pp"
        });
    }
}