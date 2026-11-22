namespace Inferno.Web;

/// <summary>Supplies one named value for <see cref="IWorkContext"/>.</summary>
public interface IWorkContextStateProvider
{
    /// <summary>A resolver for <paramref name="name"/>, or <see langword="null"/> when this provider does not own that name.</summary>
    /// <typeparam name="T">The type <see cref="IWorkContext.GetState{T}"/> asked for.</typeparam>
    /// <param name="name">The state name.</param>
    /// <returns>A function that reads the value, or <see langword="null"/>.</returns>
    Func<IWorkContext, T> Get<T>(string name);
}