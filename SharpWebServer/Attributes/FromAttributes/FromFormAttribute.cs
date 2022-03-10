using SharpWebServer.Models;

namespace SharpWebServer.Attributes.FromAttributes;

public class FromFormAttribute : FromAttribute
{
    public FromFormAttribute() : base(FromType.Form)
    {
    }
}