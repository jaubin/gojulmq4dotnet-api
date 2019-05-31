using Serilog;
using Conditions;
using Confluent.Kafka;
using Org.Gojul.GojulMQ4Net.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using Confluent.Kafka.SyncOverAsync;

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

        private static readonly ILogger _log = Serilog.Log.ForContext<GojulMQKafkaMessageConsumer<T>>();

        private readonly ISchemaRegistryClient _schemaRegistry;
        private readonly IConsumer<string, T> _consumer;
        private bool _disposed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">the settings dictionary, which follows
        /// the norm defined by Kafka settings.</param>
        /// <exception cref="ArgumentNullException">if any of the method parameters is null.</exception>
        /// <exception cref="ArgumentException">if any of the mandatory Kafka parameters is not set.</exception>
        public GojulMQKafkaMessageConsumer(Dictionary<string, string> settings)
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

            _disposed = false;

            _schemaRegistry = new CachedSchemaRegistryClient(settings);
            _consumer = new ConsumerBuilder<string, T>(KafkaSettingsList.SanitizeConfiguration(settings))
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(new AvroDeserializer<T>(_schemaRegistry).AsSyncOverAsync())
                .SetErrorHandler((_, error) =>
                {
                    _log.Error(string.Format("Error while processing message %s - Skipping this message !", error));
                })
                .Build();
        }


        /// <see cref="IGojulMQMessageConsumer{T}.ConsumeMessages(string, OnMessage{T}, CancellationToken)"/>
        public void ConsumeMessages(string topic, GojulMQMessageListener<T> messageListener,
            CancellationToken cancellationToken)
        {
            Condition.Requires(topic, "topic").IsNotNull().IsNotEmpty();
            Condition.Requires(messageListener, "messageListener").IsNotNull();
            // CancellationToken cannot be null as it is a struct.

            _consumer.Subscribe(topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int count = 0;

                    var msg = _consumer.Consume(cancellationToken);
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // We try to commit on every 100 messages, or when
                        // the cancellation token has been triggered.
                        if (msg != null)
                        {
                            messageListener(msg.Value);
                            count++;
                            if (count % 100 == 0)
                            {
                                _consumer.Commit();
                                count = 0;
                            }
                        }
                        msg = _consumer.Consume(cancellationToken);
                    }

                    if (count > 0)
                    {
                        _consumer.Commit();
                    }
                }
            }
            catch (KafkaException e)
            {
                _log.Fatal(e, string.Format("A fatal error occured : {0} - aborting the consumer", e.Message));
                throw new GojulMQException("Kafka error encountered", e);
            }            

            cancellationToken.ThrowIfCancellationRequested();
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
                    _consumer.Close();
                }
                finally
                {
                    _schemaRegistry.Dispose();
                }
            }
        }
    }
}
