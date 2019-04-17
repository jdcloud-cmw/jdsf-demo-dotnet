using System;
using Microsoft.Extensions.DependencyInjection;

namespace JDSF.Common.ServiceRegistry
{
    public static class ServiceRegistryServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceRegistry(this IServiceCollection services, ConsulConfigOption consulConfigOption,DiscoverOption discoverOption)
        {

            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var consulConfigClient = ConsulConfigClient.Init(consulConfigOption);
            services.AddSingleton<IConsulConfigClient>(consulConfigClient); 
            if (consulConfigClient == null)
            { 
                throw new Exception("please config consul client");
            } 
            var consulClient = consulConfigClient.GetConsulClient();
            var consulServiceRegistry = new ConsulServiceRegistry(consulClient);
            consulServiceRegistry.RegistryService(discoverOption);
            services.AddSingleton<IServiceRegistry>(consulServiceRegistry);
            consulConfigClient.ReloadServiceFromRegistry();
            return services;
        }
    }
}
