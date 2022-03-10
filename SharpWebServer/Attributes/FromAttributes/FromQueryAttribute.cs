using SharpWebServer.Models;

namespace SharpWebServer.Attributes.FromAttributes;

public class FromQueryAttribute : FromAttribute
{
    public FromQueryAttribute() : base(FromType.Query)
    {
    }
}