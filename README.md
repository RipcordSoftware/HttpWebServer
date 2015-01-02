[![Build Status](https://travis-ci.org/RipcordSoftware/HttpWebServer.svg?branch=master)](https://travis-ci.org/RipcordSoftware/HttpWebServer)

HttpWebServer
=============
The HttpWebServer is a C# web server implementation designed to more-or-less replace Mono's HttpListener implementation. The implementation goals were:
* Mono's HttpListener is buggy with lots of unneeded Windows API stuff
* Add native support for deflate/gzip
