using Org.Gojul.GojulMQ4Net_Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Gojul.GojulMQ4Net_Kafka
{
    public class GojulMQKafkaMessageProducer<T> : IGojulMQMessageProducer<T>
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string topic, IGojulMQMessageKeyProvider<T> messageKeyProvider, T message)
        {
            throw new NotImplementedException();
        }

        public void SendMessages(string topic, IGojulMQMessageKeyProvider<T> messageKeyProvider, IEnumerable<T> messages)
        {
            throw new NotImplementedException();
        }
    }
}
