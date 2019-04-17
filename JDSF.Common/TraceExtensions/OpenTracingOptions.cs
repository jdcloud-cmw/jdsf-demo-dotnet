using System;
using System.ComponentModel;
using Jaeger.Metrics;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Util;
using OpenTracing;
using System.Collections;
using System.Collections.Generic;
using Jaeger.Baggage;
using JDSF.Common.Config;

namespace JDSF.Common.TraceExtensions
{
    public class OpenTracingOptions
    {
        public OpenTracingOptions()
        {

        }

        public OpenTracingOptions(string serviceName)
        {
            ServiceName = serviceName;
        }

        public OpenTracingOptions(JDSFConfig jDSFConfig)
        {
            ServiceName = jDSFConfig.App.AppName;
            var traceConfig = jDSFConfig.Trace;
            switch(traceConfig.SimpleType.ToUpper())
            {
                case "CONST":
                    Sampler = new ConstSampler(true);
                    break;
                case "PROBABILISTIC":
                    Sampler = new ProbabilisticSampler(traceConfig.SimpleRate);
                    break;
                case "RATELIMITING":
                    Sampler = new RateLimitingSampler(traceConfig.SimpleRate);
                    break;
                case "REMOTE":
                    throw new NotSupportedException();
                default:
                    Sampler = new ProbabilisticSampler(0.1);
                    break;
            }
            if(!string.IsNullOrWhiteSpace(traceConfig.TraceHttpAddress))
            {
                SenderType = SenderType.HttpSender;
                HttpSenderAddress = traceConfig.TraceHttpAddress;
                if(traceConfig.TraceHttpPort > 0)
                {
                    HttpSenderPort = traceConfig.TraceHttpPort;
                }
            }else if(!string.IsNullOrWhiteSpace(traceConfig.TraceUdpAddress))
            {
                SenderType = SenderType.UDPSender;
                UDPSenderAddress = traceConfig.TraceUdpAddress;
                if (traceConfig.TraceUdpPort > 0)
                {
                    UDPSenderPort = traceConfig.TraceUdpPort;
                }
            }
            FlushInterval = TimeSpan.FromSeconds(traceConfig.FlushInterval);

        }

        public ISampler Sampler { get; set; }

        public SenderType SenderType { get; set; } = SenderType.HttpSender;

        public string HttpSenderAddress { get; set; } = "localhost";

        public int HttpSenderPort { get; set; } = 14268;

        public string UDPSenderAddress { get; set; } = "localhost";

        public int UDPSenderPort { get; set; } = 6831;

        public int UDPMaxPacketSize { get; set; } = 0;

        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(1);

        public ThriftSender ThriftSender { get; set; }

        public IMetrics Metrics { get; set; }

        public IMetrics ReporteMetrics { get; set; }

        public int ReporteMaxQueueSize { get; set; } = 100;

        public IMetricsFactory MetricsFactory { get; set; }

        public IScopeManager ScopeManager { get; set; }

        public IClock Clock { get; set; }

        public Dictionary<string,string> Tags { get; set; }

        public IBaggageRestrictionManager BaggageRestrictionManager;

        public string ServiceName { get; set; }

        public List<JaegerCodec<Object>> JaegerCodecs { get; set; }

        public List<JaegerInjector<Object>> JaegerInjectors { get; set; } 

        public List<JaegerExtractor<Object>> JaegerExtractors { get; set; }

        public bool EnableLogging { get; set; } = true;

    }
 

    public enum SenderType { 
    
        HttpSender = 1,

        ThriftSender = 2,

        UDPSender = 3
    }
}
