using System;

namespace Org.Gojul.GojulMQ4Net_Api
{
    /// <summary>
    /// Class <code>GojulMQMessageListener</code> class is in charge
    /// of providing a simple and reusable interface to deal
    /// with consumed message. Basically it is a stupid simple listener.
    /// </summary>
    /// <typeparam name="T">the message type.</typeparam>
    public interface IGojulMQMessageListener<T>
    {
        /// <summary>
        /// Method invoked when a message is received. Note that it
        /// is definitely not a good idea to throw an exception from
        /// the listener, as it could have some unpleasant side effects.
        /// </summary>
        /// <param name="message">the message to process.</param>
        void OnMessage(T message);
    }
}
