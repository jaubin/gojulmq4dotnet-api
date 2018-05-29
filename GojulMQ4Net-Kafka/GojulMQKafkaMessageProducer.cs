using Common.Logging;
using Conditions;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Org.Gojul.GojulMQ4Net_Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Gojul.GojulMQ4Net_Kafka
{
    /// <summary>
    /// Class <code>GojulMQKafkaMessageProducer</code> is the Kafka implementation
    /// of interface <code>IGojulMQMessageProducer</code>. Note that
    /// this class is thread-safe, and that messages are automatically flushed
    /// after being sent. Although this is not the fastest configuration, it is
    /// the safest one for services intended to run as daemons like this one.
    /// So in order to increase performance you should try to favor
    /// batch sending whenever possible.
    /// </summary>
    /// <typeparam name="T">the type of messages to be produced. Note that this type must
    /// have been generated following Avro conventions, as defined
    /// <a href="https://dzone.com/articles/kafka-avro-serialization-and-the-schema-registry">there</a>.
    /// </typeparam>
    public class GojulMQKafkaMessageProducer<T> : IGojulMQMessageProducer<T>
    {
        /// <summary>
        /// The bootstrap servers property used for configuration.
        /// </summary>
        public static readonly string BOOTSTRAP_SERVERS = "bootstrap.servers";

        /// <summary>
        /// The schema registry URL property used for configuration.
        /// </summary>
        public static readonly string SCHEMA_REGISTRY_URL = "schema.registry.url";

        /// <summary>
        /// The client ID property used for configuration.
        /// </summary>
        public static readonly string CLIENT_ID = "client.id";

        private static readonly ILog log = LogManager.GetLogger<GojulMQKafkaMessageProducer<T>>();

        private readonly Producer<string, T> producer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">the settings dictionary, which follows
        /// the norm defined by Kafka settings.</param>
        /// <exception cref="ArgumentNullException">if any of the method parameters is null.</exception>
        /// <exception cref="ArgumentException">if any of the mandatory Kafka parameters is not set.</exception>
        public GojulMQKafkaMessageProducer(Dictionary<string, object> settings)
        {
            Condition.Requires(settings, "settings").IsNotNull();
            Condition.Requires((string) settings[BOOTSTRAP_SERVERS], BOOTSTRAP_SERVERS)
                .IsNotNull()
                .IsNotEmpty();
            Condition.Requires((string)settings[CLIENT_ID], CLIENT_ID)
                .IsNotNull()
                .IsNotEmpty();
            Condition.Requires((string)settings[SCHEMA_REGISTRY_URL], SCHEMA_REGISTRY_URL)
                .IsNotNull()
                .IsNotEmpty();

            producer = new Producer<string, T>(settings, new StringSerializer(Encoding.UTF8), new AvroSerializer<T>());
        }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            producer.Dispose();
        }

        /// <see cref="IGojulMQMessageProducer{T}.SendMessage(string, GojulMQMessageKeyProvider{T}, T)"/>
        public void SendMessage(string topic, GojulMQMessageKeyProvider<T> messageKeyProvider, T message)
        {
            Condition.Requires( (object) message, "message").IsNotNull();

            SendMessages(topic, messageKeyProvider, new[] { message });
        }

        /// <see cref="IGojulMQMessageProducer{T}.SendMessages(string, GojulMQMessageKeyProvider{T}, IEnumerable{T})"/>
        public void SendMessages(string topic, GojulMQMessageKeyProvider<T> messageKeyProvider, IEnumerable<T> messages)
        {
            Condition.Requires(topic, "topic").IsNotNull().IsNotEmpty();
            Condition.Requires(messageKeyProvider, "messageKeyProvider").IsNotNull();
            Condition.Requires(messages, "messages").IsNotNull();

            log.Info(string.Format("Starting to send messages to topic %s", topic));

            int i = 0;
            foreach (T msg in messages)
            {
                Condition.Requires((object)msg, "msg").IsNotNull();
		// We force the producer to produce synchronously. The goal here is to avoid
		// hundreds of thread producing items in the loop, which would be a nightmare
		// in term for performance.
                producer.ProduceAsync(topic, messageKeyProvider(msg), msg).Wait();
                i++;
            }
            producer.Flush(-1);
            log.Info(string.Format("Successfully sent %d messages to topic %s", i, topic));

        }
    }
}
