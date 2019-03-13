using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jaeger.Samplers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTracingDemo.Service; 
using OpenTracingDemo.Common.ServiceRegistry;
using OpenTracingDemo.Common.ServiceCollectionExtensions;
using OpenTracingDemo.Common.LoadBalance; 

namespace OpenTracingDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddJaeger(new OpenTracingOptions { Sampler = new ConstSampler(true), SenderType = SenderType.UDPSender });
            services.AddOpenTracing();

            services.AddServiceRegistry(new ConsulConfigOption
            {
                ConsulHost = "10.12.209.43",
                ConsulPort = 8500,
                ConsulSchame = "http"
            }, new DiscoverOption 
            { 
                ServiceName = "dotnet-core-test-demo",
                IpAddress = "10.12.140.173",
                Port = 5000,
                InstanceId = "dotnet-core-test-demo-1",
                PreferIpAddress = true 
            });
            services.AddTransient<LoadBalanceHttpHandler>();
            services.AddHttpClient("LoadBalance").AddHttpMessageHandler<LoadBalanceHttpHandler>();

            services.AddHttpClient("db-service", (obj) =>
            {
                obj.BaseAddress = new Uri("http://db-service");
            }).AddTypedClient(c=> Refit.RestService.For<IDBService>(c))
            .AddHttpMessageHandler<LoadBalanceHttpHandler>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2); 

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
