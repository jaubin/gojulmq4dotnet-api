using org.gojul.gojulmq4j.kafka.tester;
using Org.Gojul.GojulMQ4Net_Api;
using Org.Gojul.GojulMQ4Net_Kafka;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Org.Gojul.GojulMQ4Net_Kafka_Test
{

    class Program
    {
        private static Dictionary<string, object> CreateConf()
        {
            return new Dictionary<string, object>
            {
                { GojulMQKafkaMessageProducer<object>.BOOTSTRAP_SERVERS, "localhost:9092" },
                { GojulMQKafkaMessageProducer<object>.CLIENT_ID, "TestProducer" },
                { GojulMQKafkaMessageProducer<object>.SCHEMA_REGISTRY_URL, "http://localhost:8081" },
                { GojulMQKafkaMessageConsumer<object>.GROUP_ID, "TestConsumer" }

            };
        }

        static void Main(string[] args)
        {
            Dictionary<string, object> settings = CreateConf();

            IGojulMQMessageConsumer<Dummy> consumer = new GojulMQKafkaMessageConsumer<Dummy>(settings);

            new Thread(() => {
                consumer.ConsumeMessages("dummyTopic", (msg) => Console.WriteLine("Consumed : " + msg.value));
            }).Start();

            IGojulMQMessageProducer<Dummy> producer = new GojulMQKafkaMessageProducer<Dummy>(settings);
            Random rnd = new Random();

            while(true)
            {
                Dummy dummy = new Dummy { value = "Hello, " + rnd.Next() };
                producer.SendMessage("dummyTopic", e => e.value, dummy);
            }
        }
    }
}
