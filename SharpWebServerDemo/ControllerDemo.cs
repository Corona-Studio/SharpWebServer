using SharpWebServer.Attributes;
using SharpWebServer.Attributes.FromAttributes;
using SharpWebServer.Interfaces;
using SharpWebServer.Models;

namespace SharpWebServerDemo;

[ApiController]
public class ControllerDemo : Controller
{
    [HttpGet(Path="test/test/{x}")]
    public IActionResult Test([FromRoute]int x, [FromBody] IEnumerable<string> texts)
    {
        Console.WriteLine($"{x} - Test, {string.Join(',', texts)}!");

        return ServiceUnavailable(new
        {
            info = "xie!"
        });
    }
}