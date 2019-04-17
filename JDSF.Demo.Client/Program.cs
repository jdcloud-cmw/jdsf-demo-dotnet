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

namespace JDSF.Demo.Client
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
                    var port = content.Configuration.GetSection("JDSFConfig").GetSection("App").GetValue<int>("AppPort");
                    if(port == 0 )
                    {
                        port = 5000;
                    }
                    options.Listen(System.Net.IPAddress.Parse("0.0.0.0"), port);
                });
    }
}
