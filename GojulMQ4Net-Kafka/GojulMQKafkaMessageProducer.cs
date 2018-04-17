using Org.Gojul.GojulMQ4Net_Api;
using System;

namespace Org.Gojul.GojulMQ4Net_Kafka
{
    public class GojulMQKafkaMessageProducer<T> : IGojulMQMessageConsumer<T>
    {
        public void ConsumeMessages(string topicName, IGojulMQMessageListener<T> messageListener)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void StopConsumer()
        {
            throw new NotImplementedException();
        }
    }
}
