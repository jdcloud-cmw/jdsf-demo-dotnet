using System;
using System.Reflection;
using System.Text;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Contrib.NetCore.CoreFx;
using OpenTracing.Util; 

namespace OpenTracingDemo.Common.ServiceCollectionExtensions
{
    public static class JaegerServiceCollectionExtensions
    {
        private static readonly string _jaegerUri =  "http://{0}/api/traces";

        public static IServiceCollection AddJaeger(this IServiceCollection services,OpenTracingOptions openTracingOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ITracer>(serviceProvider =>
            {
                string serviceName = openTracingOptions.ServiceName;
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    serviceName = Assembly.GetEntryAssembly().GetName().Name;
                }
                var traceBuilder = new Tracer.Builder(serviceName);

                if(openTracingOptions.EnableLogging)
                {
                   
                    traceBuilder.WithLoggerFactory(loggerFactory);
                }


                ISampler sampler = openTracingOptions.Sampler;
                if(sampler == null)
                {
                    sampler = new ConstSampler(true);
                }
                traceBuilder.WithSampler(sampler);

                ISender sender;
                switch (openTracingOptions.SenderType)
                {
                    case SenderType.HttpSender:
                        sender = new HttpSender($"http://{openTracingOptions.HttpSenderAddress}:{openTracingOptions.HttpSenderPort}/api/traces");
                        break;
                    case SenderType.ThriftSender:
                        sender = openTracingOptions.ThriftSender;
                        break;
                    case SenderType.UDPSender:
                        sender = new UdpSender(openTracingOptions.UDPSenderAddress,openTracingOptions.UDPSenderPort,openTracingOptions.UDPMaxPacketSize);
                        break;
                    default:
                        sender = new UdpSender();
                        break;
                }
                var reporterBuilder = new RemoteReporter.Builder().WithSender(sender);
                if (openTracingOptions.EnableLogging)
                {
                    reporterBuilder.WithLoggerFactory(loggerFactory);
                    reporterBuilder.WithFlushInterval(openTracingOptions.FlushInterval);
                    reporterBuilder.WithMaxQueueSize(openTracingOptions.ReporteMaxQueueSize);
                    if(openTracingOptions.ReporteMetrics != null)
                    {
                        reporterBuilder.WithMetrics(openTracingOptions.ReporteMetrics);
                    }

                }

                traceBuilder.WithReporter(reporterBuilder.Build());

                if(openTracingOptions.Metrics != null)
                {
                    traceBuilder.WithMetrics(openTracingOptions.Metrics);
                }
                if(openTracingOptions.MetricsFactory != null)
                {
                    traceBuilder.WithMetricsFactory(openTracingOptions.MetricsFactory);
                }

                if(openTracingOptions.BaggageRestrictionManager != null)
                {
                    traceBuilder.WithBaggageRestrictionManager(openTracingOptions.BaggageRestrictionManager);
                }

                if(openTracingOptions.Clock!= null)
                {
                    traceBuilder.WithClock(openTracingOptions.Clock);
                }

                if(openTracingOptions.ScopeManager != null)
                {
                    traceBuilder.WithScopeManager(openTracingOptions.ScopeManager);
                }

                if(openTracingOptions.Tags!= null)
                {
                    traceBuilder.WithTags(openTracingOptions.Tags);
                }

                if(openTracingOptions.JaegerCodecs != null&& openTracingOptions.JaegerCodecs.Count > 0)
                { 
                    foreach(var item in openTracingOptions.JaegerCodecs)
                    { 
                        if(item.Codec!= null)
                        {
                            traceBuilder.RegisterCodec(item.Format, item.Codec);
                        } 
                    }
                }

                if(openTracingOptions.JaegerExtractors!=null && openTracingOptions.JaegerExtractors.Count>0)
                {
                    foreach (var item in openTracingOptions.JaegerExtractors)
                    {
                        if (item.Extractor != null)
                        {
                            traceBuilder.RegisterExtractor(item.Format, item.Extractor);
                        }
                    }
                }

                if(openTracingOptions.JaegerInjectors != null && openTracingOptions.JaegerInjectors.Count>0)
                {
                    foreach (var item in openTracingOptions.JaegerInjectors)
                    {
                        if (item.Injector != null)
                        {
                            traceBuilder.RegisterInjector(item.Format, item.Injector);
                        }
                    }
                } 
                var tracer = traceBuilder.Build();
                GlobalTracer.Register(tracer);

                return tracer;
            });
            Uri jaegerUri = new Uri(string.Format(_jaegerUri, "localhost:14268"));
            // Prevent endless loops when OpenTracing is tracking HTTP requests to Jaeger.

            if(SenderType.HttpSender == openTracingOptions.SenderType && 
            (openTracingOptions.HttpSenderAddress!= "localhost"||openTracingOptions.HttpSenderPort!= 14268))
            {
                StringBuilder senderUrlBuilder = new StringBuilder();
                senderUrlBuilder.Append(openTracingOptions.HttpSenderAddress);
                if(openTracingOptions.HttpSenderPort != 80) {
                    senderUrlBuilder.Append(":").Append(openTracingOptions.HttpSenderPort.ToString());
                }
                jaegerUri = new Uri(String.Format(_jaegerUri, senderUrlBuilder.ToString()));
            }
            services.Configure<HttpHandlerDiagnosticOptions>(options =>
            {
                options.IgnorePatterns.Add(request => jaegerUri.IsBaseOf(request.RequestUri));
            });

            return services;
        }
    }
}
