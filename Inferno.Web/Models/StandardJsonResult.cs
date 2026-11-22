namespace Inferno.Web.Models;

/// <summary>A JSON payload that reports success or a list of errors.</summary>
public class StandardJsonResult
{
    /// <summary>Creates an empty result. <see cref="Succeeded"/> and <see cref="Errors"/> are left unset.</summary>
    public StandardJsonResult()
    {
    }

    internal StandardJsonResult(bool succeeded, string error)
    {
        Succeeded = succeeded;
        Errors = [error];
    }

    internal StandardJsonResult(bool succeeded, ICollection<object> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    internal StandardJsonResult(bool succeeded, ICollection<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToList<object>();
    }

    /// <summary>Whether the operation succeeded.</summary>
    public bool Succeeded { get; set; }

    /// <summary>Errors to show when <see cref="Succeeded"/> is <see langword="false"/>.</summary>
    public ICollection<object> Errors { get; set; }

    /// <summary>A success with no errors.</summary>
    /// <returns>The result.</returns>
    public static StandardJsonResult Success() => new(true, new List<object> { });

    /// <summary>A failure carrying <paramref name="errors"/>.</summary>
    /// <param name="errors">The error objects.</param>
    /// <returns>The result.</returns>
    public static StandardJsonResult Failure(ICollection<object> errors) => new(false, errors);

    /// <summary>A failure carrying <paramref name="errors"/> as objects.</summary>
    /// <param name="errors">The error messages.</param>
    /// <returns>The result.</returns>
    public static StandardJsonResult Failure(ICollection<string> errors) => new(false, errors);

    /// <summary>A failure carrying one <paramref name="error"/>.</summary>
    /// <param name="error">The error message.</param>
    /// <returns>The result.</returns>
    public static StandardJsonResult Failure(string error) => new(false, error);
}

/// <summary>A <see cref="StandardJsonResult"/> that also carries a value.</summary>
/// <typeparam name="T">The value type.</typeparam>
public class StandardJsonResult<T>
{
    internal StandardJsonResult(bool succeeded, string error)
    {
        Succeeded = succeeded;
        Errors = [error];
    }

    internal StandardJsonResult(bool succeeded, ICollection<object> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    internal StandardJsonResult(bool succeeded, T data, ICollection<object> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
        Data = data;
    }

    /// <summary>Whether the operation succeeded.</summary>
    public bool Succeeded { get; set; }

    /// <summary>The value when the operation succeeded.</summary>
    public T Data { get; set; }

    /// <summary>Errors to show when <see cref="Succeeded"/> is <see langword="false"/>.</summary>
    public ICollection<object> Errors { get; set; }

    /// <summary>A success carrying <paramref name="data"/>.</summary>
    /// <param name="data">The value.</param>
    /// <returns>The result.</returns>
    public static StandardJsonResult<T> Success(T data) => new(true, data, []);

    /// <summary>A failure carrying one <paramref name="error"/> and no data.</summary>
    /// <param name="error">The error message.</param>
    /// <returns>The result.</returns>
    public static StandardJsonResult<T> Failure(string error) => new(false, error);

    /// <summary>A failure carrying <paramref name="errors"/> and no data.</summary>
    /// <param name="errors">The error objects.</param>
    /// <returns>The result.</returns>
    public static StandardJsonResult<T> Failure(ICollection<object> errors) => new(false, errors);
}