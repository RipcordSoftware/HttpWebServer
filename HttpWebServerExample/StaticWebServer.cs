using System;
using System.IO;
using System.Collections.Generic;

using RipcordSoftware.HttpWebServer;

namespace HttpWebServerExample
{
    public class StaticWebServer
    {
        private class ContentTypeInfo
        {
            public ContentTypeInfo(string extension, string contentType, bool compressible)
            {
                Extension = extension;
                ContentType = contentType;
                Compressible = compressible;
            }

            public string Extension { get; protected set; }

            public string ContentType { get; protected set; }

            public bool Compressible { get; protected set; }
        }

        private static readonly Dictionary<string, ContentTypeInfo> contentTypeLookup = new Dictionary<string, ContentTypeInfo>()
        {
            { "jpg", new ContentTypeInfo("jpg", @"image/jpg", false) },
            { "png", new ContentTypeInfo("png", @"image/png", false) },
            { "gif", new ContentTypeInfo("gif", @"image/gif", false) },
            { "ico", new ContentTypeInfo("ico", @"image/ico", true) },
            { "bmp", new ContentTypeInfo("bmp", @"image/x-ms-bmp", true) },
            { "html", new ContentTypeInfo("html", @"text/html", true) },
            { "htm", new ContentTypeInfo("htm", @"text/html", true) },
            { "css", new ContentTypeInfo("css", @"text/css", true) },
            { "js", new ContentTypeInfo("js", @"text/javascript", true) },
            { "txt", new ContentTypeInfo("txt", @"text/plain", true) },
            { "xml", new ContentTypeInfo("xml", @"text/xml", true) },
            { "woff", new ContentTypeInfo("woff", @"application/font-woff", false) },
            { "svg", new ContentTypeInfo("svg", @"image/svg+xml", true) }
        };

        private static ContentTypeInfo GetContentTypeInfo(string extn)
        {
            ContentTypeInfo info = null;

            if (extn.Contains("/"))
            {
                extn = System.IO.Path.GetExtension(extn);
            }

            if (extn.StartsWith("."))
            {
                extn = extn.Substring(1);
            }

            contentTypeLookup.TryGetValue(extn, out info);
            return info;
        }

        private static string UrlAppendPath(string url, string path)
        {
            var appendedUrl = url;

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(path))
            {
                var terminated = url.EndsWith("/");
                var prefixed = path.StartsWith("/");

                if (terminated && prefixed)
                {
                    appendedUrl = url + path.Substring(1);
                }
                else if (!terminated && !prefixed)
                {
                    appendedUrl = url + "/" + path;
                }
                else
                {
                    appendedUrl = url + path;
                }
            }

            return appendedUrl;
        }

        public static bool HandleRequest(HttpWebRequest request, HttpWebResponse response)
        {
            bool handled = false;

            if (request.HttpMethod == "GET")
            {
                try
                {
                    var filePath = "www" + request.Uri;
                    var fileInfo = new FileInfo(filePath);

                    if (fileInfo.Attributes == FileAttributes.Directory)
                    {
                        var redirect = UrlAppendPath(request.Uri, "index.html");
                        response.Redirect(redirect);
                        handled = true;
                    }
                    else
                    {
                        var contentTypeInfo = GetContentTypeInfo(filePath);
                        if (contentTypeInfo != null)
                        {
                            if (fileInfo != null)
                            {
                                // we are going to return something, so let the caller know it happened
                                handled = true;

                                // the client may already have the file, so check the timestamps
                                bool clientHasFile = false;
                                var ifModifiedSince = request.Headers["If-Modified-Since"];
                                if (!string.IsNullOrEmpty(ifModifiedSince))
                                {
                                    try
                                    {
                                        var lastModifiedTimestamp = DateTime.Parse(ifModifiedSince);
                                        clientHasFile = fileInfo.LastWriteTime <= lastModifiedTimestamp;
                                    }
                                    catch
                                    {
                                    }
                                }

                                if (!clientHasFile)
                                {
                                    // read the file from disk and return the contents to the client
                                    using (var inFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        // send the last modified file time so we can cache any further requests from this client
                                        response.Headers["Last-Modified"] = fileInfo.LastWriteTimeUtc.ToString("r");

                                        // set the content type
                                        response.ContentType = contentTypeInfo.ContentType;

                                        using (var stream = response.GetResponseStream(request.AcceptEncoding))
                                        {
                                            inFile.CopyTo(stream);
                                        }
                                    }
                                }
                                else
                                {
                                    // the client has the file, so send a 304
                                    response.StatusCode = 304;
                                    response.ContentType = contentTypeInfo.ContentType;
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // TODO: do something more useful here
                    Console.WriteLine("ERROR: " + ex.Message);
                }
            }       

            return handled;
        }
    }
}

