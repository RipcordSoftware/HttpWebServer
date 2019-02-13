using System;

namespace RipcordSoftware.HttpWebServer
{
    public static class HttpWebServerContext
    {
        static HttpWebServerContext()
        {
            BufferManager = new HttpWebBufferManager();
        }

        public static Interfaces.IHttpWebBufferManager BufferManager { get; private set; }
    }
}
