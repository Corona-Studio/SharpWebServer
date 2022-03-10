using SharpWebServer.Models;

namespace SharpWebServer.Attributes.FromAttributes;

public class FromBodyAttribute : FromAttribute
{
    public FromBodyAttribute() : base(FromType.Body)
    {
    }
}