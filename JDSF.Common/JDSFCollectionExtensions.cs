using System;
using JDSF.Common.Config;
using JDSF.Common.TraceExtensions;
using JDSF.Common.ServiceRegistry;
using JDSF.Common.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTracing;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Jaeger;
using Jaeger.Samplers;
using OpenTracing.Util;
using OpenTracing.Contrib.NetCore.CoreFx;
using System.Text;
using Jaeger.Senders;
using Jaeger.Reporters;
using JDSF.Common.LoadBalance;

namespace JDSF.Common
{
    public static class JDSFCollectionExtensions
    {
        public static IServiceCollection AddJDSF(this IServiceCollection services, IConfiguration configuration, OpenTracingOptions openTracingOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            var jdsfConfig =new JDSFConfig() ; 
            configuration.GetSection("JDSFConfig").Bind(jdsfConfig);
            services.Configure<JDSFConfig>(configuration.GetSection("JDSFConfig"));
            ConsulConfigOption consulConfigOption = new ConsulConfigOption();
            consulConfigOption.ConsulHost = jdsfConfig.Consul.Address;
            consulConfigOption.ConsulPort = jdsfConfig.Consul.Port;
            consulConfigOption.ConsulSchame = jdsfConfig.Consul.Scheme;
            
            var consulConfigClient = ConsulConfigClient.Init(consulConfigOption);
            services.AddSingleton<IConsulConfigClient>(consulConfigClient);
            if (consulConfigClient == null)
            {
                throw new Exception("please config consul client");
            }
            var consulClient = consulConfigClient.GetConsulClient();
          

            if(jdsfConfig.Consul.Discover.Enable)
            {
                var consulServiceRegistry = new ConsulServiceRegistry(consulClient);
                DiscoverOption discoverOption = new DiscoverOption();
                discoverOption.HealthCheckUrl = jdsfConfig.Consul.Discover.HealthCheckUrl;
                var instacneId = "";
                if (string.IsNullOrWhiteSpace(jdsfConfig.Consul.Discover.ServiceInstanceId))
                {
                    instacneId = $"{jdsfConfig.App.AppName}-{Guid.NewGuid().ToString().Replace("-", "")}"; 
                }
                else {
                    instacneId = jdsfConfig.Consul.Discover.ServiceInstanceId;
                }

                discoverOption.InstanceId = instacneId;
                if(!string.IsNullOrWhiteSpace(jdsfConfig.App.AppAddress))
                {
                    discoverOption.IpAddress = jdsfConfig.App.AppAddress;
                }
                else {
                    discoverOption.IpAddress = NetworkUtil.GetHostIp(consulConfigOption.ConsulHost, consulConfigOption.ConsulPort);
                }
                
                discoverOption.Port = jdsfConfig.App.AppPort;
                discoverOption.PreferIpAddress = jdsfConfig.Consul.Discover.PreferIpAddress;
                discoverOption.ServiceName = jdsfConfig.App.AppName;
                discoverOption.Zone = jdsfConfig.Consul.Discover.InstanceZone;
                consulServiceRegistry.RegistryService(discoverOption);
                services.AddSingleton<IServiceRegistry>(consulServiceRegistry);
                consulConfigClient.ReloadServiceFromRegistry();
            }

            if(jdsfConfig.Trace.Enable)
            {
                if(openTracingOptions == null)
                {
                    openTracingOptions = new OpenTracingOptions(jdsfConfig);
                }  
                services.AddSingleton<ITracer>(serviceProvider =>
                {
                    string serviceName = openTracingOptions.ServiceName;
                    ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                    if (string.IsNullOrWhiteSpace(serviceName))
                    {
                        serviceName = Assembly.GetEntryAssembly().GetName().Name;
                    }
                    var traceBuilder = new Tracer.Builder(serviceName);

                    if (openTracingOptions.EnableLogging)
                    {

                        traceBuilder.WithLoggerFactory(loggerFactory);
                    }


                    ISampler sampler = openTracingOptions.Sampler;
                    
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
                            sender = new UdpSender(openTracingOptions.UDPSenderAddress, openTracingOptions.UDPSenderPort, openTracingOptions.UDPMaxPacketSize);
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
                        if (openTracingOptions.ReporteMetrics != null)
                        {
                            reporterBuilder.WithMetrics(openTracingOptions.ReporteMetrics);
                        }

                    }

                    traceBuilder.WithReporter(reporterBuilder.Build());

                    if (openTracingOptions.Metrics != null)
                    {
                        traceBuilder.WithMetrics(openTracingOptions.Metrics);
                    }
                    if (openTracingOptions.MetricsFactory != null)
                    {
                        traceBuilder.WithMetricsFactory(openTracingOptions.MetricsFactory);
                    }

                    if (openTracingOptions.BaggageRestrictionManager != null)
                    {
                        traceBuilder.WithBaggageRestrictionManager(openTracingOptions.BaggageRestrictionManager);
                    }

                    if (openTracingOptions.Clock != null)
                    {
                        traceBuilder.WithClock(openTracingOptions.Clock);
                    }

                    if (openTracingOptions.ScopeManager != null)
                    {
                        traceBuilder.WithScopeManager(openTracingOptions.ScopeManager);
                    }

                    if (openTracingOptions.Tags != null)
                    {
                        traceBuilder.WithTags(openTracingOptions.Tags);
                    }

                    if (openTracingOptions.JaegerCodecs != null && openTracingOptions.JaegerCodecs.Count > 0)
                    {
                        foreach (var item in openTracingOptions.JaegerCodecs)
                        {
                            if (item.Codec != null)
                            {
                                traceBuilder.RegisterCodec(item.Format, item.Codec);
                            }
                        }
                    }

                    if (openTracingOptions.JaegerExtractors != null && openTracingOptions.JaegerExtractors.Count > 0)
                    {
                        foreach (var item in openTracingOptions.JaegerExtractors)
                        {
                            if (item.Extractor != null)
                            {
                                traceBuilder.RegisterExtractor(item.Format, item.Extractor);
                            }
                        }
                    }

                    if (openTracingOptions.JaegerInjectors != null && openTracingOptions.JaegerInjectors.Count > 0)
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
                Uri jaegerUri = new Uri(string.Format(ConstParam.JAEGER_URI, "localhost:14268"));
                // Prevent endless loops when OpenTracing is tracking HTTP requests to Jaeger.

                if (SenderType.HttpSender == openTracingOptions.SenderType &&
                (openTracingOptions.HttpSenderAddress != "localhost" || openTracingOptions.HttpSenderPort != 14268))
                {
                    StringBuilder senderUrlBuilder = new StringBuilder();
                    senderUrlBuilder.Append(openTracingOptions.HttpSenderAddress);
                    if (openTracingOptions.HttpSenderPort != 80)
                    {
                        senderUrlBuilder.Append(":").Append(openTracingOptions.HttpSenderPort.ToString());
                    }
                    jaegerUri = new Uri(String.Format(ConstParam.JAEGER_URI, senderUrlBuilder.ToString()));
                }
                services.Configure<HttpHandlerDiagnosticOptions>(options =>
                {
                    options.IgnorePatterns.Add(request => jaegerUri.IsBaseOf(request.RequestUri));
                });
            } 
            services.AddTransient<LoadBalanceHttpHandler>();

            return services;
        }
    }
}
