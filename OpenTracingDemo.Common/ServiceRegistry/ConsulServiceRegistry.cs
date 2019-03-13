using System;
using System.Reflection;
using System.Text;
using Consul;

namespace OpenTracingDemo.Common.ServiceRegistry
{
    public class ConsulServiceRegistry:IServiceRegistry
    {
        private ConsulClient _ConsulClient;

        public ConsulServiceRegistry(ConsulClient consulClient)
        {
            _ConsulClient = consulClient;
        } 

        public void RegistryService(DiscoverOption option)
        {
            AgentServiceRegistration agentServiceRegistration = new AgentServiceRegistration();
            string serviceName = option.ServiceName;
            if (string.IsNullOrWhiteSpace(serviceName))
            { 
                serviceName = Assembly.GetEntryAssembly().GetName().Name;
            }
            string instanceId = option.InstanceId;
            if(string.IsNullOrWhiteSpace(instanceId))
            {
                instanceId = $"{serviceName}-{option.Port}";
            }
            agentServiceRegistration.Address = option.IpAddress;
            agentServiceRegistration.Port = option.Port;
            agentServiceRegistration.Name = serviceName;
            agentServiceRegistration.ID = instanceId;
            agentServiceRegistration.Check = new AgentServiceCheck { HTTP=$"http://{option.IpAddress}:{option.Port}{option.HealthCheckUrl}",Interval = TimeSpan.FromSeconds(10) };
            _ConsulClient.Agent.ServiceRegister(agentServiceRegistration);
        }

        public void DeRegistryService(string serviceId)
        {
            _ConsulClient.Agent.ServiceDeregister(serviceId);
        }
    }
}
