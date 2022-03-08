using System.Net;
using SharpWebServer.Interfaces;

namespace SharpWebServerDemo;

public class LogHandler : IHandler
{
    public void Handle(HttpListenerContext context)
    {
        Console.WriteLine($"[+] 接收到请求 \n\t请求类型：{context.Request.HttpMethod} \n\t路径：{context.Request.Url?.AbsolutePath ?? "/"} \n\t内容长度：{context.Request.ContentLength64} \n\t内容类型：{context.Request.ContentType ?? "-"}");
    }
}