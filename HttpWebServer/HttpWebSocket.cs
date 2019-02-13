using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace RipcordSoftware.HttpWebServer
{
    public class HttpWebSocket
    {
        #region Private fields
        /// <summary>
        /// The TCP socket
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// A locking object for the TCP socket
        /// </summary>
        private readonly object _socketLock = new object();

        /// <summary>
        /// An event we use to synchronize with an asynchronous socket send response
        /// </summary>
        private readonly AutoResetEvent completedEvent = new AutoResetEvent(false);

        private readonly System.Net.IPEndPoint _localEndPoint;
        private readonly System.Net.IPEndPoint _remoteEndPoint;
        #endregion

        #region Constructors
        public HttpWebSocket(Socket socket)
        {
            _localEndPoint = (System.Net.IPEndPoint)socket.LocalEndPoint;
            _remoteEndPoint = (System.Net.IPEndPoint)socket.RemoteEndPoint;

            _socket = socket;
        }
        #endregion

        #region Public methods
        public void Close()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Disconnect(true);
        }

        public int Send(int timeout, byte[] buffer, int offset, int length)
        {
            return Send(timeout, buffer, offset, length, SocketFlags.None);
        }

        public int Send(int timeout, byte[] buffer, int offset, int count, SocketFlags flags)
        {
            int sentBytes = -1;

            if (timeout > 0)
            {
                lock (_socketLock)
                {
                    var args = new SocketAsyncEventArgs();
                    args.SetBuffer(buffer, offset, count);
                    args.SocketFlags = flags;

                    args.Completed += (object sender, SocketAsyncEventArgs e) =>
                    {
                        if (e.BytesTransferred > 0)
                        {
                            completedEvent.Set();
                        }
                    };                

                    _socket.SendAsync(args);

                    if (completedEvent.WaitOne(timeout))
                    {
                        sentBytes = args.BytesTransferred;
                    }
                }
            }
            else
            {
                lock (_socketLock)
                {
                    sentBytes = SyncSend(_socket, buffer, offset, count, flags);
                }
            }

            return sentBytes;
        }

        public int Receive(int timeout, byte[] buffer)
        {
            return Receive(timeout, buffer, 0, buffer.Length);
        }

        public int Receive(int timeout, byte[] buffer, int offset, int length)
        {
            return Receive(timeout, buffer, offset, length, SocketFlags.None);
        }

        public int Receive(int timeout, byte[] buffer, int offset, int count, SocketFlags flags)
        {
            int receivedBytes = 0;

            try
            {
                if (timeout > 0)
                {
                    _socket.ReceiveTimeout = timeout;
                    receivedBytes = _socket.Receive(buffer, offset, count, flags);
                }
                else if (timeout == 0)
                {
                    var available = _socket.Available;
                    if (available > 0)
                    {
                        _socket.ReceiveTimeout = -1;
                        var receiveBytes = Math.Min(available, count);
                        receivedBytes = _socket.Receive(buffer, offset, receiveBytes, flags);
                    }
                }
                else
                {
                    _socket.ReceiveTimeout = -1;
                    receivedBytes = _socket.Receive(buffer, offset, count, flags);
                }
            }
            catch (SocketException ex)
            {
                // pass on all except blocking errors
                if (ex.SocketErrorCode != SocketError.WouldBlock)
                    throw;
            }

            return receivedBytes;
        }

        public void Flush()
        {
            if (NoDelay == false)
            {
                NoDelay = true;
                NoDelay = false;
            }
        }
        #endregion

        #region Public properties
        public bool Connected { get { return _socket.Connected; } }
        public int Available { get { return _socket.Available; } }
        public bool NoDelay { get { return _socket.NoDelay; } set { _socket.NoDelay = value; } }

        public System.Net.IPEndPoint LocalEndPoint { get { return _localEndPoint; } }
        public System.Net.IPEndPoint RemoteEndPoint { get { return _remoteEndPoint; } }
        #endregion

        #region Private methods
        /// <summary>
        /// Sends the buffer synchronously to the remote, guarantees the buffer is sent or an exception will be thrown by the socket layer
        /// </summary>
        private static int SyncSend(Socket socket, byte[] buffer, int offset, int count, SocketFlags flags)
        {
            int sentBytes = 0;

            while (sentBytes < count)
            {
                sentBytes += socket.Send(buffer, offset + sentBytes, count - sentBytes, flags);
            }

            return sentBytes;
        }
        #endregion
    }
}

