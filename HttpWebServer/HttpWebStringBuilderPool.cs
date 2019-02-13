using System;
using System.Text;
using System.Threading;

namespace RipcordSoftware.HttpWebServer
{
    /// <summary>
    /// A *simple* StringBuilder instance pool
    /// </summary>
    public class HttpWebStringBuilderPool
    {
        #region Private fields
        private readonly StringBuilder[] _pool;
        private int _index = 0;
        private readonly int _stringLength;
        #endregion

        #region Constructor
        public HttpWebStringBuilderPool(int poolSize = 16, int stringLength = 1024)
        {
            _pool = new StringBuilder[poolSize];
            _stringLength = stringLength;

            for (int i = 0; i < _pool.Length; i++)
            {
                _pool[i] = new StringBuilder(_stringLength);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Get an instance of StringBuilder from the pool
        /// </summary>
        public StringBuilder Acquire()
        {
            int thisIndex = Interlocked.Increment(ref _index) - 1;
            thisIndex %= _pool.Length;

            var builder = Interlocked.Exchange(ref _pool[thisIndex], null);
            if (builder == null)
            {
                builder = new StringBuilder(_stringLength);
            }

            // make sure the builder is clear
            builder.Clear();

            return builder;
        }

        /// <summary>
        /// Release an instance of StringBuilder back into the pool
        /// </summary>
        /// <param name="builder">Builder.</param>
        public bool Release(StringBuilder builder)
        {
            bool released = false;

            if (builder != null)
            {                   
                // get the current index into the pool
                int currIndex = _index % _pool.Length;

                // the most likely null entry in the pool is the last entry we were at, so try there first
                if (currIndex > 0)
                {
                    currIndex--;
                }

                for (int i = currIndex; !released && i < _pool.Length; i++)
                {
                    if (_pool[i] == null)
                    {
                        released = Interlocked.CompareExchange(ref _pool[i], builder, null) == null;
                    }
                }

                for (int i = 0; !released && i < currIndex; i++)
                {
                    if (_pool[i] == null)
                    {
                        released = Interlocked.CompareExchange(ref _pool[i], builder, null) == null;
                    }
                }
            }

            return released;
        }
        #endregion
    }
}

