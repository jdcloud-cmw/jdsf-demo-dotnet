using System;
using System.Text;
using Consul;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Threading;

namespace JDSF.Common.ServiceRegistry
{
    public class ConsulConfigClient : IConsulConfigClient
    {
        private static Dictionary<String, CacheServiceInfo> serviceCacheDic = new Dictionary<string, CacheServiceInfo>();

        private static ConsulClient _ConsulClient;

        private static ConsulConfigOption _ConsulConfigOption;

        private static string _LastConnectionIp = "";
        public static readonly Object obj = new Object();

        private Timer timer;


        private ConsulConfigClient(ConsulConfigOption consulConfig)
        {
            _ConsulConfigOption = consulConfig;
        }

        public static ConsulConfigClient Init(ConsulConfigOption consulConfig)
        {
            return new ConsulConfigClient(consulConfig);
        }


        public ConsulClient GetConsulClient()
        {
            if (_ConsulClient == null)
            {
                if (_ConsulConfigOption == null)
                {
                    throw new Exception("ConsulClient not init");
                }

                lock (obj)
                {
                    if (_ConsulClient == null)
                    {
                        StringBuilder consulUriBuilder = new StringBuilder();
                        consulUriBuilder.Append(_ConsulConfigOption.ConsulSchame);
                        consulUriBuilder.Append("://");
                        consulUriBuilder.Append(_ConsulConfigOption.ConsulHost);
                        if (!((_ConsulConfigOption.ConsulPort == 80 && _ConsulConfigOption.ConsulSchame.ToLower() == "http") &&
                        (_ConsulConfigOption.ConsulPort == 443 && _ConsulConfigOption.ConsulSchame.ToLower() == "https")))
                        {
                            consulUriBuilder.Append(":").Append(_ConsulConfigOption.ConsulPort.ToString());
                        }
                        _ConsulClient = new ConsulClient(opt => { opt.Address = new Uri(consulUriBuilder.ToString()); });

                        _LastConnectionIp = _ConsulConfigOption.ConsulHost;
                    }

                }
            }
            return _ConsulClient;
        }


        public void ReloadConsulConsulClient(string hostIp)
        {

            if (_ConsulClient != null)
            {
                lock (obj)
                {
                    StringBuilder consulUriBuilder = new StringBuilder();
                    consulUriBuilder.Append(_ConsulConfigOption.ConsulSchame);
                    consulUriBuilder.Append("://");
                    consulUriBuilder.Append(hostIp);
                    if (!((_ConsulConfigOption.ConsulPort == 80 && _ConsulConfigOption.ConsulSchame.ToLower() == "http") &&
                    (_ConsulConfigOption.ConsulPort == 443 && _ConsulConfigOption.ConsulSchame.ToLower() == "https")))
                    {
                        consulUriBuilder.Append(":").Append(_ConsulConfigOption.ConsulPort.ToString());
                    }
                    _ConsulClient = new ConsulClient(opt => { opt.Address = new Uri(consulUriBuilder.ToString()); });
                    _LastConnectionIp = hostIp;
                }
            }
        }

        public void ReloadServiceFromRegistry()
        {
            var autoEvent = new AutoResetEvent(true);
            timer = new Timer(p => ReloadService(), autoEvent, 30 * 1000, 30 * 1000);


        }

        private void ReloadService()
        {
            if (serviceCacheDic.Count > 0)
                lock (obj)
                {
                    
                    Dictionary<string, CacheServiceInfo> localCacheServiceInfo = new Dictionary<string, CacheServiceInfo>();
                    foreach (var key in serviceCacheDic.Keys)
                    {
                        var service = _ConsulClient.Health.Service(key).Result;
                        if (service != null && service.StatusCode == HttpStatusCode.OK && service.Response != null)
                        {
                            var healthService = service.Response;
                            if (healthService != null)
                            {
                                List<HealthServiceInfo> healthServiceInfos = new List<HealthServiceInfo>();
                                foreach (var item in healthService)
                                {
                                    if (item.Checks.All(p => p.Status == HealthStatus.Passing))
                                    {
                                        HealthServiceInfo healthServiceInfo = new HealthServiceInfo();
                                        healthServiceInfo.InstanceId = item.Service.ID;
                                        healthServiceInfo.ServiceHost = item.Service.Address;
                                        healthServiceInfo.ServiceName = item.Service.Service;
                                        healthServiceInfo.ServicePort = item.Service.Port;
                                        healthServiceInfos.Add(healthServiceInfo);
                                    }
                                }

                                if (healthServiceInfos.Count > 0)
                                {
                                    CacheServiceInfo cacheServiceInfo = new CacheServiceInfo();
                                    cacheServiceInfo.HealthServiceList = healthServiceInfos;
                                    cacheServiceInfo.ServiceName = key;
                                    localCacheServiceInfo.Add(key, cacheServiceInfo);


                                }
                                 
                            }

                        }
                    }

                    serviceCacheDic.Clear();

                    if (localCacheServiceInfo.Count > 0)
                    {
                        foreach (var item in localCacheServiceInfo)
                        {
                            if (serviceCacheDic.Keys.Contains(item.Key))
                            {
                                serviceCacheDic[item.Key] = item.Value;
                            }
                            else
                            {
                                serviceCacheDic.Add(item.Key, item.Value);
                            }
                        }
                    }
                }
        }

        public async Task<HealthServiceInfo> GetHealthService(string serviceName)
        {

            if (serviceCacheDic.ContainsKey(serviceName))
            {
                var healthServiceList = serviceCacheDic[serviceName].HealthServiceList;
                if (healthServiceList != null && healthServiceList.Count > 0)
                {
                    Random r = new Random(Guid.NewGuid().ToString().GetHashCode());
                    var index = r.Next(0, healthServiceList.Count - 1);
                    return healthServiceList[index];
                }
            }
            else
            {
                var service = await _ConsulClient.Health.Service(serviceName);
                if (service != null && service.StatusCode == HttpStatusCode.OK && service.Response != null)
                {
                    var healthService = service.Response;
                    if (healthService != null)
                    {
                        List<HealthServiceInfo> healthServiceInfos = new List<HealthServiceInfo>();
                        foreach (var item in healthService)
                        {
                            if (item.Checks.All(p => p.Status == HealthStatus.Passing))
                            {
                                HealthServiceInfo healthServiceInfo = new HealthServiceInfo();
                                healthServiceInfo.InstanceId = item.Service.ID;
                                healthServiceInfo.ServiceHost = item.Service.Address;
                                healthServiceInfo.ServiceName = item.Service.Service;
                                healthServiceInfo.ServicePort = item.Service.Port;
                                healthServiceInfos.Add(healthServiceInfo);
                            }
                        }

                        if (healthServiceInfos.Count > 0)
                        {
                            CacheServiceInfo cacheServiceInfo = new CacheServiceInfo();
                            cacheServiceInfo.HealthServiceList = healthServiceInfos;
                            cacheServiceInfo.ServiceName = serviceName;
                            serviceCacheDic.Add(serviceName, cacheServiceInfo);
                            Random r = new Random(Guid.NewGuid().ToString().GetHashCode());
                            var index = r.Next(0, healthServiceInfos.Count - 1);
                            return healthServiceInfos[index];
                        }
                    }
                }
            }
            return null;
        }


    }
}
