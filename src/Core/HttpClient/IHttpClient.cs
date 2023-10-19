using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Core.HttpClient;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.Logging;

namespace Logzio.DotNet.Core.WebClient
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding, bool useGzip = false);
    }

    public class HttpClientHandler : IHttpClient
    {
        private static HttpClient _client;
        private static readonly ILogger msLogger = new Logger<HttpClientHandler>(new LoggerFactory());
        public HttpClientHandler()
        {
            _client = new HttpClient();
            msLogger.LogInformation("HttpClient created.");

        }

        public HttpClientHandler(string proxyAddress)
        {
            if (proxyAddress != string.Empty)
            {
                var handler = new System.Net.Http.HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = new Proxy(new Uri(proxyAddress))
                };
                _client = new HttpClient(handler: handler);
                msLogger.LogInformation("HttpClient created with proxy address: {proxyAddress}", proxyAddress);

            }
            else
            {
                _client = new HttpClient();
            }
        }
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _client.GetAsync(url).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding, bool useGzip = false)
        {
            var content = new StreamContent(body);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = encoding.WebName };

            if (useGzip)
            {
                using (var gzipContent = new GzipContent(content))
                    return await _client.PostAsync(url, gzipContent).ConfigureAwait(false);
            }
          
            return await _client.PostAsync(url, content).ConfigureAwait(false);
        }
    }
}
