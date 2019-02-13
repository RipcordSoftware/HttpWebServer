using System;
using NUnit.Framework;

using RipcordSoftware.HttpWebServer;

namespace RipcordSoftware.HttpWebServer.UnitTests
{
    [TestFixture()]
    public class TestHttpWebBufferManager
    {
        private Interfaces.IHttpWebBufferManager _bufferManager;

        [SetUp()]
        public void SetUp()
        {
            _bufferManager = new HttpWebBufferManager();
        }

        [Test()]
        public void TestGetBuffer()
        {
            var buffer = _bufferManager.GetBuffer(1024);

            Assert.IsNotNull(buffer);
            Assert.AreEqual(1024, buffer.Length);
        }

        [Test()]
        public void TestGetBufferDuplicate()
        {
            var buffer1 = _bufferManager.GetBuffer(1024);
            var buffer2 = _bufferManager.GetBuffer(1024);

            Assert.IsNotNull(buffer1);
            Assert.AreEqual(1024, buffer1.Length);

            Assert.IsNotNull(buffer2);
            Assert.AreEqual(1024, buffer2.Length);

            Assert.AreNotSame(buffer1, buffer2);
        }
    }
}
