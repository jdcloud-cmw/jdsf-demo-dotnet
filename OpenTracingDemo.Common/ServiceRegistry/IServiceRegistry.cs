using System;
namespace OpenTracingDemo.Common.ServiceRegistry
{
    public interface IServiceRegistry
    {
        void RegistryService(DiscoverOption option);

        void DeRegistryService(string serviceId);
    }
}
