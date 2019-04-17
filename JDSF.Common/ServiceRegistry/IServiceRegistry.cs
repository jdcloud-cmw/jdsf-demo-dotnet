using System;
using System.Collections.Generic;

namespace JDSF.Common.ServiceRegistry
{
    public interface IServiceRegistry
    {
        void RegistryService(DiscoverOption option);

        void DeRegistryService(string serviceId);

        List<HealthServiceInfo> GetHealthServices(string serviceName);

        void UpdateHealthService();
    }
}
