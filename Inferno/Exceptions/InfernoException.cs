using System.Runtime.Serialization;

namespace Inferno.Exceptions
{
    public class InfernoException : Exception
    {
        public InfernoException()
        {
        }

        public InfernoException(string message)
            : base(message)
        {
        }

        public InfernoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InfernoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}