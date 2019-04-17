using System;
using System.Collections.Generic;

namespace JDSF.Common.ServiceRegistry
{
    public class HealthServiceInfo
    {
        public HealthServiceInfo()
        {
        }

        public string ServiceName { get; set; }

        public string InstanceId { get; set; }

        public string ServiceHost { get; set; }

        public int ServicePort { get; set; }
    }


    public class CacheServiceInfo {

        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public string ServiceName { get; set; }

        public List<HealthServiceInfo> HealthServiceList { get; set; }
    }
}
