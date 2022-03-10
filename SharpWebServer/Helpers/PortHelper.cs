using System.Net;
using System.Net.Sockets;

namespace SharpWebServer.Helpers;

public static class PortHelper
{
    public static int FreeTcpPort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);

        l.Start();
        var port = ((IPEndPoint) l.LocalEndpoint).Port;
        l.Stop();

        return port;
    }
}