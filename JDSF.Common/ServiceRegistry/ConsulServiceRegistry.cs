using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Consul;
using JDSF.Common.Util;
using System.Collections.Concurrent;

namespace JDSF.Common.ServiceRegistry
{
    public class ConsulServiceRegistry:IServiceRegistry
    {
        private ConsulClient _ConsulClient;

        private static ConcurrentDictionary<string,List<HealthServiceInfo>> cacheServiceInfos = new ConcurrentDictionary<string, List<HealthServiceInfo>>();

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
            if(option.PreferIpAddress)
            {
                agentServiceRegistration.Address = option.IpAddress;
            }
            else {
                agentServiceRegistration.Address = NetworkUtil.GetSelfHostName();
            }

            agentServiceRegistration.Port = option.Port;
            agentServiceRegistration.Name = serviceName;
            agentServiceRegistration.ID = instanceId;
            if(!string.IsNullOrWhiteSpace(option.Zone))
            {
                agentServiceRegistration.Meta.Add("zone", option.Zone);
            }
            agentServiceRegistration.Check = new AgentServiceCheck { HTTP=$"http://{option.IpAddress}:{option.Port}{option.HealthCheckUrl}",Interval = TimeSpan.FromSeconds(10) };
            _ConsulClient.Agent.ServiceRegister(agentServiceRegistration);
        }

        public void DeRegistryService(string serviceId)
        {
            _ConsulClient.Agent.ServiceDeregister(serviceId);
        }

        public List<HealthServiceInfo> GetHealthServices(string serviceName)
        {
            if(cacheServiceInfos.ContainsKey(serviceName))
            { 
                if(cacheServiceInfos[serviceName].Count>0)
                {
                    return cacheServiceInfos[serviceName];
                }
            }

            //var service = _ConsulClient.Agent.(host);
            throw new NotImplementedException();
        }

        public void UpdateHealthService()
        {
            throw new NotImplementedException();
        }
    }
}
