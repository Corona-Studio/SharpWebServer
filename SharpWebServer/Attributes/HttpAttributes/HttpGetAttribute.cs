namespace SharpWebServer.Attributes;


public class HttpGetAttribute : HttpAttribute
{
    public HttpGetAttribute() : base("GET"){}
}