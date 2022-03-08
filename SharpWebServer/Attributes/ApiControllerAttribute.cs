namespace SharpWebServer.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ApiControllerAttribute : Attribute
{
    public string ApiRoot { get; }

    public ApiControllerAttribute()
    {
        ApiRoot = "/";
    }

    public ApiControllerAttribute(string apiRoot)
    {
        ApiRoot = apiRoot;
    }
}