using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;

namespace RipcordSoftware.HttpWebServer
{
    /// <summary>
    /// A simple buffer manager class to reduce GC workload for applications where buffer allocation is common operation
    /// </summary>
    public class HttpWebBufferManager : Interfaces.IHttpWebBufferManager
    {
        #region Constants
        private const int SmallBufferLimit = 4 * 1024;
        private const int MediumBufferLimit = 32 * 1024;
        private const int LargeBufferLimit = 256 * 1024;
        private const int MaxBufferLimit = 512 * 1024;

        private const int MaxSmallBuffers = 512;
        private const int MaxMediumBuffers = 256;
        private const int MaxLargeBuffers = 64;
        #endregion

        #region Private fields
        private readonly ConcurrentQueue<byte[]> _smallBuffers = new ConcurrentQueue<byte[]>();
        private readonly ConcurrentQueue<byte[]> _mediumBuffers = new ConcurrentQueue<byte[]>();
        private readonly ConcurrentQueue<byte[]> _largeBuffers = new ConcurrentQueue<byte[]>();
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the buffer from the manager
        /// </summary>
        public byte[] GetBuffer(int length)
        {
            byte[] buffer = null;

            if (length >= 0)
            {
                var buffers = _smallBuffers;

                if (length > SmallBufferLimit)
                {
                    if (length <= MediumBufferLimit)
                    {
                        buffers = _mediumBuffers;
                    }
                    else if (length <= LargeBufferLimit)
                    {
                        buffers = _largeBuffers;
                    }
                }
                    
                if (buffers != null && buffers.Count > 0)
                {
                    buffers.TryDequeue(out buffer);
                }

                if (buffer == null)
                {
                    buffer = new byte[length];
                }
            }

            return buffer;
        }

        public MemoryStream GetMemoryStream(int length)
        {
            MemoryStream stream = null;

            var buffer = GetBuffer(length);
            if (buffer != null)
            {
                stream = new MemoryStream(buffer, 0, length, true, true);
            }

            return stream; 
        }

        /// <summary>
        /// Returns a buffer for reuse by the manager
        /// </summary>
        public bool ReleaseBuffer(byte[] buffer)
        {
            bool released = false;

            if (buffer != null)
            {
                var length = buffer.Length;
                if (length <= MaxBufferLimit)
                {
                    ConcurrentQueue<byte[]> buffers = null;
                    int maxBufferLength = 0;

                    if (length >= LargeBufferLimit)
                    {
                        buffers = _largeBuffers;
                        maxBufferLength = MaxLargeBuffers;
                    }
                    else if (length >= MediumBufferLimit)
                    {
                        buffers = _mediumBuffers;
                        maxBufferLength = MaxMediumBuffers;
                    }
                    else if (length >= SmallBufferLimit)
                    {
                        buffers = _smallBuffers;
                        maxBufferLength = MaxSmallBuffers;
                    }

                    if (buffers != null && buffers.Count < maxBufferLength)
                    {
                        buffers.Enqueue(buffer);
                        released = true;
                    }
                }
            }

            return released;
        }

        /// <summary>
        /// Returns a set of buffers to the manager
        /// </summary>
        /// <param name="buffers">Buffers.</param>
        public void ReleaseBuffers(IList<byte[]> buffers)
        {
            if (buffers != null)
            {
                foreach (var buffer in buffers)
                {
                    ReleaseBuffer(buffer);
                }
            }
        }

        public bool ReleaseMemoryStream(ref MemoryStream stream)
        {
            var released = false;

            if (stream != null)
            {
                var buffer = stream.GetBuffer();
                if (buffer != null && buffer.Length > 0)
                {
                    released = ReleaseBuffer(buffer);
                    if (released)
                    {
                        stream = null;
                    }
                }
            }

            return released;
        }

        #endregion
    }
}

