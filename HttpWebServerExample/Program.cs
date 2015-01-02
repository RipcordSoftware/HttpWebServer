using System;

using RipcordSoftware.HttpWebServer;

namespace HttpWebServerExample
{
    class MainClass
    {
        private const int port = 3010;

        private static bool RequestCallback(HttpWebRequest request, HttpWebResponse response)
        {
            var status = StaticWebServer.HandleRequest(request, response);

            if (status)
            {            
                Console.WriteLine("{0}: {1}, {2}", request.HttpMethod, request.Uri, response.StatusCode);
            }

            return status;
        }
            
        private static bool RequestContinueCallback(HttpWebRequest request)
        {
            return true;
        }

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
    }
}
