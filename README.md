[![Build Status](https://travis-ci.org/RipcordSoftware/HttpWebServer.svg?branch=master)](https://travis-ci.org/RipcordSoftware/HttpWebServer)

HttpWebServer
=============
The HttpWebServer is a C# web server implementation designed to drop-in replace Mono's HttpListener. The main goals were:
* Approximate the HttpListener implementation without the unneeded Windows parts
* Keep things simple
* Be fast

HttpWebServer runs under Mono 3.x and .NET 4.5 and above.

Example
---
It is very simple to get things started:
```C#
public static void Main(string[] args)
{
    var bindings = new HttpWebServer.Binding[] { new HttpWebServer.Binding("127.0.0.1", port, false) };
    var config = new HttpWebServer.Config();

    using (var server = new HttpWebServer(bindings, config))
    {
        server.Start(RequestCallback, RequestContinueCallback);

        Console.WriteLine("Listening on port {0}", port);

        System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
    }
}
```
