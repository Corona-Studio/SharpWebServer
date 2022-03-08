using System.Net;

namespace SharpWebServer.Interfaces;

public interface IHandler
{
    void Handle(HttpListenerContext context);
}

public interface IErrorHandler : IHandler{}