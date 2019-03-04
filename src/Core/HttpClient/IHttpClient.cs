using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Core.HttpClient;

namespace Logzio.DotNet.Core.WebClient
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding, bool useGzip = false);
    }

    public class HttpClientHandler : IHttpClient
    {
        private readonly HttpClient _client;
        
        public HttpClientHandler()
        {
            _client = new HttpClient();
        }
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding, bool useGzip = false)
        {
            var content = new StreamContent(body);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = encoding.WebName };

            if (useGzip)
            {
                using (var gzipContent = new GzipContent(content))
                    return await _client.PostAsync(url, gzipContent);
            }
          
            return await _client.PostAsync(url, content);
        }
    }

}