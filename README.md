[![Build Status](https://travis-ci.org/RipcordSoftware/HttpWebServer.svg?branch=master)](https://travis-ci.org/RipcordSoftware/HttpWebServer)

HttpWebServer
=============
The HttpWebServer is a C# web server implementation designed to drop-in replace Mono's HttpListener. The main goals were:
* Approximate the HttpListener implementation without the unneeded Windows parts
* Keep things simple
* Be fast
HttpWebServer runs under Mono 3.x and .NET 4.5 and above.
