using SharpWebServer.Models;

namespace SharpWebServer.Attributes.FromAttributes;

public class FromRouteAttribute : FromAttribute
{
    public FromRouteAttribute() : base(FromType.Route)
    {
    }
}