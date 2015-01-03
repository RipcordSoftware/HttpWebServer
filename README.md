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
The Start() method accepts two delegate parameters. When a request arrives `RequestCallback` is invoked with  `HttpWebRequest` and `HttpWebResponse` parameters. Both these types are very similar to their namesake types in `System.Net`. If you handled the request in the delegate then return true, otherwise return false.

`RequestContinueCallback` is invoked when a `POST` request is received with an `Expect: 100-Continue` header. If the delegate returns true then the web server will allow the client to continue to `POST` the body, otherwise the connection is closed. If the parameter passed to Start() is null then the request is always handled. For more info on `100-Continue` see http://www.w3.org/Protocols/rfc2616/rfc2616-sec8.html.

In the `HttpWebServerExample` project in the solution there is a static web server example. The code serves static files from the `www` sub-directory under the application root directory. See `HandleRequest()` in `StaticWebServer.cs` for the implementation.
