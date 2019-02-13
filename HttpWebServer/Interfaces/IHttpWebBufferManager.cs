using System;
using System.IO;

namespace RipcordSoftware.HttpWebServer.Interfaces
{
    public interface IHttpWebBufferManager
    {
        byte[] GetBuffer(int length);
        bool ReleaseBuffer(byte[] buffer);

        MemoryStream GetMemoryStream(int length);
        bool ReleaseMemoryStream(ref MemoryStream stream);
    }
}
