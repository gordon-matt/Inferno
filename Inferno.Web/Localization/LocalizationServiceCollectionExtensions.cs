using System;
using Inferno.Web.Localization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LocalizationServiceCollectionExtensions
    {
        public static IServiceCollection AddInfernoLocalization(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(new ServiceDescriptor(typeof(IStringLocalizerFactory), typeof(InfernoStringLocalizerFactory), ServiceLifetime.Scoped));
            services.TryAdd(new ServiceDescriptor(typeof(IStringLocalizer), typeof(InfernoStringLocalizer), ServiceLifetime.Scoped));

            return services;
        }
    }
}