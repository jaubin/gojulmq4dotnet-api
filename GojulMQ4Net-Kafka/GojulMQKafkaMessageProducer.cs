using Serilog;
using Conditions;
using Confluent.Kafka;
using Org.Gojul.GojulMQ4Net.Api;
using System;
using System.Collections.Generic;
using System.Text;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Confluent.Kafka.SyncOverAsync;

namespace Org.Gojul.GojulMQ4Net.Kafka
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
    public sealed class GojulMQKafkaMessageProducer<T> : IGojulMQMessageProducer<T>
    {
        /// <summary>
        /// The bootstrap servers property used for configuration.
        /// </summary>
        public const string BootstrapServers = "bootstrap.servers";

        /// <summary>
        /// The schema registry URL property used for configuration.
        /// </summary>
        public const string SchemaRegistryUrl = "schema.registry.url";

        /// <summary>
        /// The client ID property used for configuration.
        /// </summary>
        public const string ClientId = "client.id";

        private static readonly ILogger log = Serilog.Log.ForContext<GojulMQKafkaMessageProducer<T>>();

        private bool _disposed;
        private readonly ISchemaRegistryClient _schemaRegistry;
        private readonly IProducer<string, T> _producer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">the settings dictionary, which follows
        /// the norm defined by Kafka settings.</param>
        /// <exception cref="ArgumentNullException">if any of the method parameters is null.</exception>
        /// <exception cref="ArgumentException">if any of the mandatory Kafka parameters is not set.</exception>
        public GojulMQKafkaMessageProducer(Dictionary<string, string> settings)
        {
            Condition.Requires(settings, "settings").IsNotNull();
            Condition.Requires((string)settings[BootstrapServers], BootstrapServers)
                .IsNotNull()
                .IsNotEmpty();
            Condition.Requires((string)settings[ClientId], ClientId)
                .IsNotNull()
                .IsNotEmpty();
            Condition.Requires((string)settings[SchemaRegistryUrl], SchemaRegistryUrl)
                .IsNotNull()
                .IsNotEmpty();

            _disposed = false;
            _schemaRegistry = new CachedSchemaRegistryClient(settings);
            _producer = new ProducerBuilder<string, T>(KafkaSettingsList.SanitizeConfiguration(settings))
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(new AvroSerializer<T>(_schemaRegistry).AsSyncOverAsync())
                .Build();
        }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                try
                {
                    _producer.Dispose();
                }
                finally
                {
                    _schemaRegistry.Dispose();
                }
            }
        }

        /// <see cref="IGojulMQMessageProducer{T}.SendMessage(string, GojulMQMessageKeyProvider{T}, T)"/>
        public void SendMessage(string topic, GojulMQMessageKeyProvider<T> messageKeyProvider, T message)
        {
            Condition.Requires((object)message, "message").IsNotNull();

            SendMessages(topic, messageKeyProvider, new[] { message });
        }

        /// <see cref="IGojulMQMessageProducer{T}.SendMessages(string, GojulMQMessageKeyProvider{T}, IEnumerable{T})"/>
        public void SendMessages(string topic, GojulMQMessageKeyProvider<T> messageKeyProvider, IEnumerable<T> messages)
        {
            Condition.Requires(topic, "topic").IsNotNull().IsNotEmpty();
            Condition.Requires(messageKeyProvider, "messageKeyProvider").IsNotNull();
            Condition.Requires(messages, "messages").IsNotNull();

            log.Information(string.Format("Starting to send messages to topic %s", topic));

            int i = 0;
            foreach (T msg in messages)
            {
                Condition.Requires((object)msg, "msg").IsNotNull();
                // We force the producer to produce synchronously. The goal here is to avoid
                // hundreds of thread producing items in the loop, which would be a nightmare
                // in term for performance.
                var kafkaMessage = new Message<string, T> { Key = messageKeyProvider(msg), Value = msg };
                _producer.Produce(topic, kafkaMessage);
                i++;
            }
            _producer.Flush();
            log.Information(string.Format("Successfully sent {0} messages to topic {1}", i, topic));

        }
    }
}
