namespace Inferno.Web.Mvc.Controllers;

internal interface ITypeActivatorCache
{
    /// <summary>
    /// Creates an instance of <typeparamref name="TInstance"/>.
    /// </summary>
    /// <typeparam name="TInstance">The type the instance is returned as.</typeparam>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve dependencies for
    /// <paramref name="optionType"/>.</param>
    /// <param name="optionType">The <see cref="Type"/> of the <typeparamref name="TInstance"/> to create.</param>
    /// <returns>The created instance.</returns>
    TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type optionType);
}