using Serilog;
using Conditions;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Org.Gojul.GojulMQ4Net.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Org.Gojul.GojulMQ4Net.Kafka
{
    /// <summary>
    /// Class <code>GojulMQKafkaMessageConsumer</code> is the Kafka implementation of interface
    /// <code>IGojulMQMessageConsumer</code>. Note that this implementation is not thread-safe. However
    /// it's not exactly a bright idea ot share the same message listener between threads in most
    /// systems. Thus messages are automatically acked after being consumed. Although it is not the
    /// most efficient behaviour, it is the safest one for services intended to run as daemons like
    /// this one.
    /// </summary>
    /// <typeparam name="T">the type of messages to be read. Note that these messages must follow the norm
    /// defined by Avro so that they're recorded in the schema registry.</typeparam>
    public sealed class GojulMQKafkaMessageConsumer<T> : IGojulMQMessageConsumer<T>
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
        /// The group ID property used for configuration.
        /// </summary>
        public const string GroupId = "group.id";

        private static readonly ILogger log = Serilog.Log.ForContext<GojulMQKafkaMessageConsumer<T>>();

        private readonly Consumer<string, T> consumer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">the settings dictionary, which follows
        /// the norm defined by Kafka settings.</param>
        /// <exception cref="ArgumentNullException">if any of the method parameters is null.</exception>
        /// <exception cref="ArgumentException">if any of the mandatory Kafka parameters is not set.</exception>
        public GojulMQKafkaMessageConsumer(Dictionary<string, object> settings)
        {
            Condition.Requires(settings, "settings").IsNotNull();
            Condition.Requires((string)settings[BootstrapServers], BootstrapServers)
                .IsNotNull()
                .IsNotEmpty();
            Condition.Requires((string)settings[GroupId], GroupId)
                .IsNotNull()
                .IsNotEmpty();
            Condition.Requires((string)settings[SchemaRegistryUrl], SchemaRegistryUrl)
                .IsNotNull()
                .IsNotEmpty();

            consumer = new Consumer<string, T>(settings, new StringDeserializer(Encoding.UTF8),
                new AvroDeserializer<T>());
        }

        /// <see cref="IGojulMQMessageConsumer{T}.ConsumeMessages(string, OnMessage{T}, CancellationToken)"/>
        public void ConsumeMessages(string topic, GojulMQMessageListener<T> messageListener,
            CancellationToken cancellationToken)
        {
            Condition.Requires(topic, "topic").IsNotNull().IsNotEmpty();
            Condition.Requires(messageListener, "messageListener").IsNotNull();
            // CancellationToken cannot be null as it is a struct.

            consumer.Subscribe(topic);

            consumer.OnConsumeError += (_, msg) =>
                log.Error(string.Format("Error while processing message %s - Skipping this message !", msg.Error));
            consumer.OnError += (_, error) =>
            {
                log.Fatal(string.Format("A fatal error occurred - aborting consumer ! Reason : %s", error.Reason));
                throw new GojulMQException(error.Reason);
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                int count = 0;
                Message<string, T> msg;
                while (consumer.Consume(out msg, 100)
                      && !cancellationToken.IsCancellationRequested)
                {
                    messageListener(msg.Value);
                    count++;
                    if (count % 100 == 0)
                    {
                        // We force synchronous commit there.
                        consumer.CommitAsync().Wait();
                        count = 0;
                    }
                }

                if (count > 0)
                {
                    consumer.CommitAsync().Wait();
                }
            }

            consumer.Dispose();

            cancellationToken.ThrowIfCancellationRequested();
        }


        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            consumer.Dispose();
        }
    }
}
