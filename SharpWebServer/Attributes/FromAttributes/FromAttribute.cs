using SharpWebServer.Models;

namespace SharpWebServer.Attributes.FromAttributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromAttribute : Attribute
{
    public FromType From { get; }

    public FromAttribute(FromType fromType)
    {
        From = fromType;
    }
}