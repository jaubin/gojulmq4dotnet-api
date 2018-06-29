using System;

namespace Org.Gojul.GojulMQ4Net_Api
{

    /// <summary>
    /// Method invoked when a message is received. Note that it
    /// is definitely not a good idea to throw an exception from
    /// the listener, as it could have some unpleasant side effects.
    /// </summary>
    /// <param name="message">the message to process.</param>
    public delegate void GojulMQMessageListener<T>(T message);

    /// <summary>
    /// Interface <code>IGojulMQMessageConsumer</code> is used
    /// in order to make it possible to consume messages easily.
    /// It is not up to you to implement this interface, it is provided
    /// by various implementations of the API. Note that depending on the
    /// implementation instances of this interface may or may not be thread-safe.
    /// However it is generally thought that a consumer should not be shared between
    /// different threads, contrary to a producer. An instance of this class should
    /// basically be seen as a service that you inject in your code using an IoC mechanism.
    /// Note that in case you do not run this consumer as a daemon you must
    /// close it explicitely.
    /// </summary>
    /// <typeparam name="T">the type of messages to listen to.</typeparam>
    public interface IGojulMQMessageConsumer<T>: IDisposable
    {
        /// <summary>
        /// Consume the messages from topic with name <code>topic</code>.
        /// </summary>
        /// <param name="topic">the name of the topic from which messages must be consumed.</param>
        /// <param name="messageListener">the listener implementation used to listen to messages.</param>
        /// <exception cref="ArgumentNullException">if any of the method parameters is <code>Null</code>.</exception>
        void ConsumeMessages(string topic, GojulMQMessageListener<T> messageListener);

        /// <summary>
        /// Notify the consumer to stop doing stuff. Note that once a consumer has
        /// been stopped it cannot be reused.
        /// </summary>
        void StopConsumer();
    }
}
