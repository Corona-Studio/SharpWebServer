using SharpWebServer.Models;

namespace SharpWebServer.Attributes.FromAttributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromAttribute : Attribute
{
    public FromAttribute(FromType fromType)
    {
        From = fromType;
    }

    public FromType From { get; }
}