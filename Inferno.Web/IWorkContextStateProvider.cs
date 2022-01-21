namespace Inferno.Web
{
    public interface IWorkContextStateProvider
    {
        Func<IWorkContext, T> Get<T>(string name);
    }
}