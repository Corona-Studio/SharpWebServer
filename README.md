# SharpWebServer

## Description

This is a simple web server framework power by .NET 6 using HttpListener.  
It has a syntax similar to AspNet Core.  
You can use it to run a simple webserver on any platform without Server bundle!

## Features

| Feature | Status |
| - | - |
| Http Get | ✅ |
| Http Post | ✅ |
| Custom Http Method | ✅ |
| Multiple Controllers | ✅ |
| Middlewares (Handlers) | ✅ |
| JSON return value | ✅ |
| Acquire arguments from body | ✅ |
| Acquire arguments from form | ✅ |
| Acquire arguments from query | ✅ |
| Acquire arguments from route | ✅ |
| HTTPS support | WIP |

## Usage

You can take a look at SharpWebServerDemo project for the example code.

```c#

var webServer = new SimpleWebServer("http://[BIND_ADDRESS]:[BIND_PORT]/");

webServer.RegisterHandler<LogHandler>();          // normal handler
webServer.RegisterHandler<ErrorHandler>();        // error handler

webServer.RegisterController<ControllerDemo>();   // register you controller

webServer.Start();                                // start!

```

## Add a controller

```c#

[ApiController]                                                                         // add ApiController attribute
public class ControllerDemo : Controller
{
    [HttpGet(Path="test/test/{x}")]                                                     // define HTTP method
    public IActionResult Test([FromRoute]int x, [FromBody] IEnumerable<string> texts)   // define your arguments
    {
        Console.WriteLine($"{x} - Test, {string.Join(',', texts)}!");

        return ServiceUnavailable(new                                                   // define status code and the response content!
        {
            info = "xie!"
        });
    }
}

```

## Add a handler

```c#

public class LogHandler : IHandler
{
    public void Handle(HttpListenerContext context)
    {
        // your code goes here
    }
}

```
