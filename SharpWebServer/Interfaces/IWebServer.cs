namespace SharpWebServer.Interfaces;

public interface IWebServer : IDisposable
{
    public string Prefix { get; }

    public void RegisterHandler<T>() where T : IHandler;

    public void RegisterController<T>() where T : IController;
    public void UnRegisterController<T>() where T : IController;
    public bool GetController<T>(out IController? controller) where T : IController;

    public void Start();
    public void Stop();
}