using System;

namespace RipcordSoftware.HttpWebServer
{
    public class HttpWebBuffer : IDisposable
    {
        #region Private fields
        private byte[] _buffer;
        private int _dataLength;
        private readonly Interfaces.IHttpWebBufferManager _bufferManager;
        #endregion

        #region Constructor
        public HttpWebBuffer(byte[] buffer, int bufferOffset, int dataLength) :
            this(buffer, bufferOffset, dataLength, HttpWebServerContext.BufferManager)
        {

        }

        public HttpWebBuffer(byte[] buffer, int bufferOffset, int dataLength, Interfaces.IHttpWebBufferManager bufferManager)
        {
            if (bufferOffset + dataLength - buffer.Length > 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _bufferManager = bufferManager;
            _buffer = _bufferManager.GetBuffer(dataLength);
            _dataLength = dataLength;

            Array.Copy(buffer, bufferOffset, _buffer, 0, dataLength);
        }
        #endregion

        #region Public methods
        public void Append(byte[] appendBuffer, int appendBufferOffset, int appendDataLength)
        {
            var newDataLength = _dataLength + appendDataLength;

            if (newDataLength > _buffer.Length)
            {
                var newBuffer = _bufferManager.GetBuffer(newDataLength);

                Array.Copy(_buffer, 0, newBuffer, 0, _dataLength);
                Array.Copy(appendBuffer, appendBufferOffset, newBuffer, _dataLength, appendDataLength);

                _bufferManager.ReleaseBuffer(_buffer);

                _buffer = newBuffer;
                _dataLength = newDataLength;
            }
            else
            {
                Array.Copy(appendBuffer, appendBufferOffset, _buffer, _dataLength, appendDataLength);
                _dataLength += appendDataLength;
            }
        }

        public void ReleaseBuffer()
        {
            if (_buffer != null)
            {
                _bufferManager.ReleaseBuffer(_buffer);
                _buffer = null;
                _dataLength = 0;
            }
        }
        #endregion

        #region Public properties
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= _dataLength)
                {
                    throw new IndexOutOfRangeException();
                }

                return _buffer[index];
            }
        }
            
        public int DataLength { get { return _dataLength; } }

        public byte[] Buffer { get { return _buffer; } }
        public int BufferLength { get { return _buffer.Length; } }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            ReleaseBuffer();
        }
        #endregion
    }
}

