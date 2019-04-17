using System;
namespace JDSF.Common.Config
{
    public class JDSFConfig
    {
        public App App { get; set; }

        public Consul Consul { get; set; }

        public Trace Trace { get; set; }
    }


    public class App
    {

        public string AppName { get; set; }


        public int AppPort { get; set; } = 5000;

        public string AppAddress { get; set; } 
    }

    public class Consul {

        public string Scheme { get; set; } = "Http";

        public int Port { get; set; } = 8500;

        public string Address { get; set; } = "127.0.0.1";

        public ConsulDiscover Discover { get; set; }

        public ConsulConfig Config { get; set; }

    }

    public class ConsulDiscover {

      
        public bool Enable { get; set; } = true;


        public string ServiceInstanceId { get; set; }


        public string HealthCheckUrl { get; set; } = "/api/health/check";


        public string InstanceZone { get; set; }


        public int ServiceTTLTime { get; set; } = 30;

        public bool PreferIpAddress { get; set; } = false;


    }

    public class ConsulConfig
    {
        public bool Enable { get; set; } = false;

        public bool Profile { get; set; }  
    }


    public class Trace {
        public bool Enable { get; set; } = false;

        public string SimpleType { get; set; } = "";

        public double SimpleRate { get; set; } = 0.1;

        public string SimpleParam { get; set; }


        public string TraceHttpAddress {get;set;}

        public int TraceHttpPort { get; set; }

        public string TraceUdpAddress { get; set; }

        public int TraceUdpPort { get; set; }

        public int FlushInterval { get; set; } = 1;
    }
}
