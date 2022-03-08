// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using SharpWebServer;
using SharpWebServer.Helpers;
using SharpWebServerDemo;

Console.WriteLine("Hello, World!");

var webServer = new SimpleWebServer("http://127.0.0.1:2222/");

webServer.RegisterHandler<LogHandler>();
webServer.RegisterHandler<ErrorHandler>();

webServer.RegisterController<ControllerDemo>();

webServer.Start();

Console.WriteLine(JsonSerializer.Deserialize<int>("1"));

Console.ReadLine();