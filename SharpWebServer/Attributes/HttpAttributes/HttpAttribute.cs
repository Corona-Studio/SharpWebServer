namespace SharpWebServer.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HttpAttribute : Attribute
{
    public HttpAttribute(string method)
    {
        Method = method;
    }

    public string Method { get; }
    public string? RequestContentType { get; set; }
    public string? ResponseContentType { get; set; }
    public string? Path { get; set; }
}