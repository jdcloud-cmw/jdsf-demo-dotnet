using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;
using Consul;
using System.Text;
using OpenTracingDemo.Common.ServiceRegistry;

namespace OpenTracingDemo.Common.LoadBalance
{
    public class LoadBalanceHttpHandler : DelegatingHandler
    {

        private IConsulConfigClient _ConsulConfigClient;

        public LoadBalanceHttpHandler(IConsulConfigClient consulConfigClient)
        {
            _ConsulConfigClient = consulConfigClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
                                HttpRequestMessage request,
                                CancellationToken cancellationToken)
        {

            var uri = request.RequestUri;
            var host = uri.Host;
            Regex rx = new Regex(@"((?:(?:25[0-5]|2[0-4]\\d|[01]?\\d?\\d)\\.){3}(?:25[0-5]|2[0-4]\\d|[01]?\\d?\\d))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if(rx.IsMatch(host))
            {
                return await base.SendAsync(request, cancellationToken);
            } 
            var consulClient = _ConsulConfigClient.GetConsulClient();
            var service = await _ConsulConfigClient.GetHealthService(host);
            if(service!=null)
            {
                StringBuilder requestUrlBuilder = new StringBuilder();
                requestUrlBuilder.Append(request.RequestUri.Scheme);
                requestUrlBuilder.Append("://");

                string address = service.ServiceHost;
                requestUrlBuilder.Append(address);
                int port = service.ServicePort;
                if ((port != 80 && request.RequestUri.Scheme.ToLower() != "http")
                || (port != 443 && request.RequestUri.Scheme.ToLower() != "https"))
                {
                    requestUrlBuilder.Append(":").Append(port);
                } 
                requestUrlBuilder.Append(request.RequestUri.PathAndQuery);
                if (!string.IsNullOrWhiteSpace(request.RequestUri.Fragment))
                {
                    requestUrlBuilder.Append("#").Append(request.RequestUri.Fragment);
                }

                var requestUri = new Uri(requestUrlBuilder.ToString());
                request.RequestUri = requestUri;
                return await base.SendAsync(request, cancellationToken);
            }
             
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
