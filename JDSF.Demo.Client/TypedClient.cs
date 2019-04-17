using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JDSF.Demo.Client
{
    class TypedClient:HttpClient
    {
        private readonly HttpClient _httpClient;
        public TypedClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task CallServerAsync()
        {
            var response = await _httpClient.GetAsync("/downloads/Racers.xml");
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
        }
    }
}
