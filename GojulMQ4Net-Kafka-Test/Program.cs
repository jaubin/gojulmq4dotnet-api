using org.gojul.gojulmq4j.kafka.tester;
using Org.Gojul.GojulMQ4Net.Api;
using Org.Gojul.GojulMQ4Net.Kafka;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Org.Gojul.GojulMQ4Net.Kafka_Test
{

    class Program
    {
        private static Dictionary<string, object> CreateConf()
        {
            return new Dictionary<string, object>
            {
                { GojulMQKafkaMessageProducer<object>.BootstrapServers, "localhost:9092" },
                { GojulMQKafkaMessageProducer<object>.ClientId, "TestProducer" },
                { GojulMQKafkaMessageProducer<object>.SchemaRegistryUrl, "http://localhost:8081" },
                { GojulMQKafkaMessageConsumer<object>.GroupId, "TestConsumer" }

            };
        }

        static void Main(string[] args)
        {
            Dictionary<string, object> settings = CreateConf();

            CancellationTokenSource cts = new CancellationTokenSource();

            IGojulMQMessageConsumer<Dummy> consumer = new GojulMQKafkaMessageConsumer<Dummy>(settings);

            new Thread(() => {
                consumer.ConsumeMessages("dummyTopic", (msg) => Console.WriteLine("Consumed : " + msg.value),
                    cts.Token);
            }).Start();

            IGojulMQMessageProducer<Dummy> producer = new GojulMQKafkaMessageProducer<Dummy>(settings);
            Random rnd = new Random();

            while(true)
            {
                Dummy dummy = new Dummy { value = "Hello, " + rnd.Next() };
                producer.SendMessage("dummyTopic", e => e.value, dummy);

                Console.WriteLine("Produced : " + dummy.value);
            }
        }
    }
}
