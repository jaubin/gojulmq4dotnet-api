using System;

namespace Org.Gojul.GojulMQ4Net.Api
{
    /// <summary>
    /// Class <code>GojulMQException</code> represents the
    /// type of exception to be seen when a message transfer error
    /// occurs.
    /// </summary>
	public class GojulMQException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public GojulMQException()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">the error message.</param>
        public GojulMQException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">the error message.</param>
        /// <param name="cause">the exception which caused the error.</param>
        public GojulMQException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}
