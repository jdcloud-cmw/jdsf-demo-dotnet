using System;
namespace JDSF.Common.ServiceRegistry
{
    public class ConsulConfigOption
    {
        public ConsulConfigOption()
        {
        }

        public string ConsulHost { get; set; } = "localhost";

        public int ConsulPort { get; set; } = 8500;

        public string ConsulSchame { get; set; } = "http";

        public string ConsulAclToken { get; set; }
    }

    public class DiscoverOption { 

        public bool DiscoverEnable { get; set; }

        public bool PreferIpAddress { get; set; }

        public string ServiceName { get; set; }

        public string HealthCheckUrl { get; set; } = "/api/health/check";
            
        public string InstanceId { get; set; }

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public string Zone { get; set; }
    }
}
