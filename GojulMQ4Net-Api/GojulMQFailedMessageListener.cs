using System;
using System.Collections.Generic;
using System.Text;

using Serilog;
using Conditions;

namespace Org.Gojul.GojulMQ4Net.Api
{
    /// <summary>
    /// Class <code>GojulMQFailedMessageListener</code> is a simple wrapper
    /// around an usual message consumer listener which redirects failed
    /// messages to an hospital topic.
    /// </summary>
    public class GojulMQFailedMessageListener<T>
    {

        private static readonly ILogger log = Serilog.Log.ForContext<GojulMQFailedMessageListener<T>>();

        private readonly IGojulMQMessageProducer<T> _producer;
        private readonly GojulMQMessageListener<T> _listener;
        private readonly string _errorTopic;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="producer">the producer in charge of redirecting erroneous objects.</param>
        /// <param name="listener">the listener to decorate.</param>
        /// <param name="errorTopic">the error topic used.</param>
        /// <exception cref="ArgumentNullException">if any of the method parameters is <code>null</code>.</exception>
        public GojulMQFailedMessageListener(IGojulMQMessageProducer<T> producer,
            GojulMQMessageListener<T> listener, string errorTopic)
        {
            Condition.Requires(producer).IsNotNull();
            Condition.Requires(listener).IsNotNull();
            Condition.Requires(errorTopic).IsNotNullOrWhiteSpace();

            this._producer = producer;
            this._listener = listener;
            this._errorTopic = errorTopic;
        }

        /// <summary>
        /// Process the message <code>message</code>.
        /// </summary>
        /// <param name="message">the message to process.</param>
        /// <exception cref="ArgumentNullException">if <code>message</code> is null.</exception>
        /// <exception cref="GojulMQException">if an error occurs while processing the message due to the message queue.</exception>
        public void OnMessage(T message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message is null");
            }

            try
            {
                _listener(message);
            }
            catch (GojulMQException e)
            {
                log.Error(e, "Error with the MQ system");
                throw;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                log.Error(e, "Error processing message");
                _producer.SendMessage(_errorTopic, msg => null, message);
            }
#pragma warning restore CA1031 // Do not catch general exception types

        }
    }
}
