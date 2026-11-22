using System.Runtime.Serialization;

namespace Inferno.Exceptions;

/// <summary>Base exception for Inferno.</summary>
public class InfernoException : Exception
{
    /// <summary>Creates an exception with no message.</summary>
    public InfernoException()
    {
    }

    /// <summary>Creates an exception with the given message.</summary>
    /// <param name="message">Text stored on the exception. May be <see langword="null"/>.</param>
    public InfernoException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and the exception that caused it.</summary>
    /// <param name="message">Text stored on the exception. May be <see langword="null"/>.</param>
    /// <param name="innerException">The exception that caused this one. May be <see langword="null"/>.</param>
    public InfernoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates an exception from serialized data.</summary>
    /// <param name="info">The serialized fields.</param>
    /// <param name="context">The source and destination of the serialized stream.</param>
    protected InfernoException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
