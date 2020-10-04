using System;
using System.Runtime.Serialization;

namespace Sketch.Infrastructure.Connection
{
    [Serializable]
    internal class CannotRemovePlayerConnectionException : Exception
    {
        public CannotRemovePlayerConnectionException()
        {
        }

        public CannotRemovePlayerConnectionException(string? message) : base(message)
        {
        }

        public CannotRemovePlayerConnectionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CannotRemovePlayerConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}