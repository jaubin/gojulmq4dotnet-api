using System;
using System.Collections.Generic;

namespace Org.Gojul.GojulMQ4Net.Api
{

    /// <summary>
    /// Return the key for object <code>msg</code>. Depending on your
    /// needs this method may return <code>Null</code>.
    /// </summary>
    /// <param name="msg">the message for which a key must be generated.</param>
    /// <returns>the key for object <code>msg</code>.</returns>
    public delegate string GojulMQMessageKeyProvider<T>(T msg);

    /// <summary>
    /// Interface <code>IGojulMQMessageProducer</code> is used to produce messages
    /// to send to a message broker. Note that it's not up to you to implement
    /// this class, it is implemented by the various library implementations. Implementors
    /// should generally considered as being thread-safe, contrary to what happens with
    /// producers. Note that in case you do not run this producer as a daemon you must
    /// close it explicitely.
    /// </summary>
    /// <typeparam name="T">the type of messages to be produced.</typeparam>
    public interface IGojulMQMessageProducer<T> : IDisposable
    {
        /// <summary>
        /// Send the message <code>message</code> to the MQ on topic with name <code>topic</code>.
        /// </summary>
        /// <param name="topic">the topic to which messages must be sent.</param>
        /// <param name="messageKeyProvider">the message key provider used.</param>
        /// <param name="message">the message to send itself.</param>
        /// <exception cref="ArgumentNullException">if any of the method parameters is <code>Null</code>.</exception>
        /// <exception cref="GojulMQException">if a transfer error occured.</exception>
        void SendMessage(string topic, GojulMQMessageKeyProvider<T> messageKeyProvider, T message);

        /// <summary>
        /// Send the messages from iterable <code>messages</code>. This method allows to make batch
        /// transmissions, which usually tend to be faster than single message transmissions.
        /// </summary>
        /// <param name="topic">the topic to which messages must be sent.</param>
        /// <param name="messageKeyProvider">the message key provider used.</param>
        /// <param name="messages">the messages to send.</param>
        /// <exception cref="ArgumentNullException">if any of the method parameters is <code>Null</code>.</exception>
        /// <exception cref="GojulMQException">if a transfer error occured.</exception>
        void SendMessages(string topic, GojulMQMessageKeyProvider<T> messageKeyProvider, IEnumerable<T> messages);
    }
}

