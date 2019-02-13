using System;
using System.IO;
using System.Text;

namespace RipcordSoftware.HttpWebServer
{
    public class HttpWebResponseStream : Stream
    {
        #region Types
        private class ResponseStream : Stream
        {
            #region Private fields
            private readonly Interfaces.IHttpWebBufferManager _bufferManager;
            private HttpWebSocket _socket;
            private readonly bool _keepSocketAlive;

            private byte[] _streamBuffer;
            private int _streamBufferPosition = 0;
            private long _position = 0;
            #endregion

            #region Constructor
            public ResponseStream(HttpWebSocket socket, bool keepSocketAlive, Interfaces.IHttpWebBufferManager bufferManager)
            {
                _socket = socket;
                _bufferManager = bufferManager;
                _streamBuffer = _bufferManager.GetBuffer(HttpWebServer.ResponseStreamBufferSize);
                _keepSocketAlive = keepSocketAlive;
            }
            #endregion

            #region implemented abstract members of Stream
            public override void Flush()
            {
                if (_socket != null)
                {
                    _socket.Flush();
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (_socket != null)
                {
                    if ((_streamBufferPosition + count) >= _streamBuffer.Length)
                    {
                        SendBuffer(_socket, _streamBuffer, 0, _streamBufferPosition);
                        SendBuffer(_socket, buffer, offset, count);
                        _streamBufferPosition = 0;
                    }
                    else
                    {
                        Array.Copy(buffer, offset, _streamBuffer, _streamBufferPosition, count);
                        _streamBufferPosition += count;
                    }

                    _position += count;
                }
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }
            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override long Length
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override long Position
            {
                get
                {
                    return _position;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
            #endregion

            #region Public methods
            public override void Close()
            {
                if (_socket != null)
                {
                    if (_streamBufferPosition > 0)
                    {
                        SendBuffer(_socket, _streamBuffer, 0, _streamBufferPosition);
                        _position += _streamBufferPosition;
                        _streamBufferPosition = 0;
                    }

                    Flush();

                    if (!_keepSocketAlive)
                    {
                        _socket.Close();
                    }

                    _socket = null;
                }

                if (_streamBuffer != null)
                {
                    _bufferManager.ReleaseBuffer(_streamBuffer);
                    _streamBuffer = null;
                }
            }
            #endregion

            #region Private methods
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Close();
                }
            }

            private static int SendBuffer(HttpWebSocket socket, byte[] buffer, int offset, int count)
            {
                var written = 0;

                if (socket != null)
                {
                    while (written < count)
                    {
                        var sentBytes = socket.Send(0, buffer, offset + written, count - written);
                        written += sentBytes;
                    }
                }

                return written;
            }
            #endregion
        }

        private class ChunkedResponseStream : Stream
        {
            #region Private fields
            private static byte[] _maxBlockSizeHeader = GetChunkHeader(HttpWebServer.MaxResponseChunkSize);
            private static byte[] _endResponseHeader = GetChunkHeader(0);
            private static byte[] _endOfLine = Encoding.ASCII.GetBytes(HttpWebServer.EndOfLine);

            private readonly byte[] _streamBuffer;

            private readonly ResponseStream _stream;
            #endregion

            #region Constructor
            public ChunkedResponseStream(ResponseStream stream)
            {
                _stream = stream;
                _streamBuffer = new byte[_maxBlockSizeHeader.Length + HttpWebServer.MaxResponseChunkSize + _endOfLine.Length];
            }
            #endregion
                
            #region Public methods
            public override void Write(byte[] buffer, int offset, int count)
            {
                var blocks = count / HttpWebServer.MaxResponseChunkSize;
                var overflow = count % HttpWebServer.MaxResponseChunkSize;

                if (blocks > 0)
                {
                    // copy the chunk header into the stream buffer
                    Array.Copy(_maxBlockSizeHeader, _streamBuffer, _maxBlockSizeHeader.Length);

                    // copy the chunk trailer into the stream buffer
                    Array.Copy(_endOfLine, 0, _streamBuffer, _streamBuffer.Length - _endOfLine.Length, _endOfLine.Length);

                    for (int i = 0; i < blocks; i++)
                    {
                        // copy in the chunk data
                        Array.Copy(buffer, offset, _streamBuffer, _maxBlockSizeHeader.Length, HttpWebServer.MaxResponseChunkSize);
                        offset += HttpWebServer.MaxResponseChunkSize;

                        // write the buffer
                        _stream.Write(_streamBuffer, 0, _streamBuffer.Length);
                    }
                }

                if (overflow > 0)
                {
                    // get the chunk overflow header
                    var header = GetChunkHeader(overflow);

                    // copy the header into the stream buffer
                    Array.Copy(header, _streamBuffer, header.Length);
                    int overflowLength = header.Length;

                    // copy the chunk body
                    Array.Copy(buffer, offset, _streamBuffer, overflowLength, overflow);
                    overflowLength += overflow;

                    // copy the chunk trailer
                    Array.Copy(_endOfLine, 0, _streamBuffer, overflowLength, _endOfLine.Length);
                    overflowLength += _endOfLine.Length;

                    // write the overflow data into the socket
                    _stream.Write(_streamBuffer, 0, overflowLength);
                }                        
            }

            public override void Close()
            {
                // the response finishes with a \r\n
                _stream.Write(_endResponseHeader, 0, _endResponseHeader.Length);

                _stream.Close();
            }
            #endregion

            #region Private methods
            private static byte[] GetChunkHeader(int size)
            {
                var format = "{0:X}" + HttpWebServer.EndOfLine + (size == 0 ? HttpWebServer.EndOfLine : string.Empty);
                var text = string.Format(format, size);
                return Encoding.ASCII.GetBytes(text);
            }
            #endregion

            #region implemented abstract members of Stream
            public override void Flush()
            {
                _stream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override long Length
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override long Position
            {
                get
                {
                    return _stream.Position;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
            #endregion

            #region Protected methods
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Close();
                }
            }
            #endregion
        }
        #endregion

        #region Private fields
        private Stream _stream;
        #endregion

        #region Constructor
        internal HttpWebResponseStream(HttpWebSocket socket, byte[] headers, bool keepSocketAlive, bool isChunked, Interfaces.IHttpWebBufferManager bufferManager)
        {
            var responseStream = new ResponseStream(socket, keepSocketAlive, bufferManager);

            responseStream.Write(headers, 0, headers.Length);

            if (isChunked)
            {
                _stream = new ChunkedResponseStream(responseStream);
            }
            else
            {
                _stream = responseStream;
            }
        }
        #endregion

        #region implemented abstract members of Stream
        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Public methods
        public override void Close()
        {
            _stream.Close();
        }
        #endregion

        #region Private methods
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }
        #endregion
    }
}

