﻿namespace SharpWebServer.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ApiControllerAttribute : Attribute
{
    public ApiControllerAttribute()
    {
        ApiRoot = "/";
    }

    public ApiControllerAttribute(string apiRoot)
    {
        ApiRoot = $"/{apiRoot.Trim('/')}";
    }

    public string ApiRoot { get; }
}