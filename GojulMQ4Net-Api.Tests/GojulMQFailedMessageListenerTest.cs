using System;

using Moq;
using Xunit;

using Org.Gojul.GojulMQ4Net.Api;

namespace Org.Gojul.GojulMQ4Net.Api.Tests
{
    public class GojulMQFailedMessageListenerTest: IDisposable
    {
        private bool consumed;

        public GojulMQFailedMessageListenerTest()
        {
            consumed = false;
        }

        private void ListenMessage(object message)
        {
            consumed = true;
        }

        [Fact]
        public void TestConstructorWithNullProducerThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new GojulMQFailedMessageListener<object>(null, ListenMessage, "hello"));
        }

        [Fact]
        public void TestConstructorWithNullListenerThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new GojulMQFailedMessageListener<object>(new Mock<IGojulMQMessageProducer<object>>().Object, 
                null, "hello"));
        }

        [Fact]
        public void TestConstructorWithNullTopicThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new GojulMQFailedMessageListener<object>(new Mock<IGojulMQMessageProducer<object>>().Object,
                ListenMessage, null));
        }

        [Fact]
        public void TestConstructorWithBlankTopicThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new GojulMQFailedMessageListener<object>(new Mock<IGojulMQMessageProducer<object>>().Object,
                ListenMessage, "   "));
        }

        [Fact]
        public void TestOnMessageWithNullMessageThrowsException()
        {
            var listener = new GojulMQFailedMessageListener<object>(new Mock<IGojulMQMessageProducer<object>>().Object,
                ListenMessage, "hello");
            Assert.Throws<ArgumentNullException>(() => listener.OnMessage(null));
        }

        [Fact]
        public void TestOnMessageWithMQExceptionForwardsException()
        {
            var listener = new GojulMQFailedMessageListener<object>(new Mock<IGojulMQMessageProducer<object>>().Object,
                e => throw new GojulMQException(), "hello");

            Assert.Throws<GojulMQException>(() => listener.OnMessage("world"));
        }

        [Fact]
        public void TestOnMessageWithStandardExceptionThrowsMessageToErrorQueue()
        {
            Mock<IGojulMQMessageProducer<object>> producer = new Mock<IGojulMQMessageProducer<object>>();

            var listener = new GojulMQFailedMessageListener<object>(producer.Object,
                e => throw new Exception(), "hello");
            listener.OnMessage("world");

            producer.Verify(x => x.SendMessage("hello", It.IsAny<GojulMQMessageKeyProvider<object>>(), "world"));
        }

        [Fact]
        public void TestOnMessage()
        {
            Mock<IGojulMQMessageProducer<object>> producer = new Mock<IGojulMQMessageProducer<object>>();

            var listener = new GojulMQFailedMessageListener<object>(producer.Object, ListenMessage, "hello");
            listener.OnMessage("world");

            Assert.True(consumed);
        }

        public void Dispose()
        {
        }
    }
}
