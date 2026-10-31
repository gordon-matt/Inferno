using Dependo;
using Microsoft.Extensions.Configuration;

namespace InfernoCMS.Services;

public class DependencyRegistrar : IDependencyRegistrar
{
    public int Order => 999;

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
    }
}
