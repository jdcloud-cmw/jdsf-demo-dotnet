using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OpenTracingDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();
            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>().ConfigureKestrel((content, options) =>
                {
                    var ipAddress = content.Configuration.GetSection("Server").GetValue<string>("Host");
                    var port = content.Configuration.GetSection("Server").GetValue<int>("Port");
                    Console.WriteLine(ipAddress + ":" + port);
                    if(string.IsNullOrWhiteSpace(ipAddress))
                    {
                        ipAddress = IPAddress.Loopback.ToString();
                    }

                    if(port == 0 )
                    {
                        port = 5000;
                    }
                    options.Listen(System.Net.IPAddress.Parse(ipAddress), port);
                });
    }
}
