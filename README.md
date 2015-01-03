[![Build Status](https://travis-ci.org/RipcordSoftware/HttpWebServer.svg?branch=master)](https://travis-ci.org/RipcordSoftware/HttpWebServer)

HttpWebServer
=============
The HttpWebServer is a C# web server implementation designed to drop-in replace Mono's HttpListener. The main goals were:
* Approximate the HttpListener implementation without the unneeded Windows parts
* Keep things simple
* Be fast

HttpWebServer runs under Mono 3.x and .NET 4.5 and above.

Building
----
Build from `MonoDevelop` or from the command line:
```shell
xbuild HttpWebServer.sln
```
To build in release mode execute `xbuild` with the additional parameter: `/p:configuration=Release`.

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

In the `HttpWebServerExample` project in the solution there is a static web server example. The code serves static files from the `www` sub-directory under the application root directory. To get the example running you should get a copy of `MonoDevelop` 5.x, open the sln and start the `HttpWebServerExample` in debug mode. When the web server is running you should see output on the console like this:
```
Listening on port 3010
GET: /index.html, 200
GET: /css/bootstrap.min.css, 200
GET: /css/bootstrap-theme.min.css, 200
GET: /css/font-awesome.min.css, 200
GET: /css/main.css, 200
GET: /js/angular-route.min.js, 200
GET: /js/bootstrap.min.js, 200
GET: /js/jquery.min.js, 200
GET: /js/angular.min.js, 200
GET: /js/main.js, 200
GET: /templates/header.html, 200
GET: /templates/footer.html, 200
GET: /partials/home.html, 200
GET: /fonts/fontawesome-webfont.woff, 304
GET: /partials/pricing.html, 304
GET: /partials/about.html, 304
GET: /partials/faq.html, 304
GET: /partials/services.html, 304
```
The example site is part of an AngularJS Tutorial series by Nike Kaye. You can find a complete live running instance here: http://httpwebserver.ripcordsoftware.com.

Differences and Missing Parts
-------------
* Exceptions are derived from `HttpWebServerException`/ApplicationException and not from the `System.Net` exception types
* There is currently no support for SSL
* HTTP/1.0 clients are not supported, for example pointing `wget` at `HttpWebServer` may result in chunked responses which wget doesn't know how to handle. Use `curl` instead - or PR 1.0 support in if it really matters to you.
