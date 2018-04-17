using System;

namespace Org.Gojul.GojulMQ4Net_Api
{
    /// <summary>
    /// Some message brokers strongly suggest that you provide a key for your messages.
    /// The purpose of this interface is to provide a simple way to generate this key.
    /// </summary>
    /// <typeparam name="T">the type of object for which you must provide a key.</typeparam>
    public interface IGojulMQMessageKeyProvider<T>
    {
        /// <summary>
        /// Return the key for object <code>msg</code>. Depending on your
        /// needs this method may return <code>Null</code>.
        /// </summary>
        /// <param name="msg">the message for which a key must be generated.</param>
        /// <returns>the key for object <code>msg</code>.</returns>
        string GetKey(T msg);
    }
}
