using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Core.HttpClient;
using Logzio.DotNet.Core.Shipping;
using Logzio.DotNet.Core.InternalLogger;

namespace Logzio.DotNet.Core.WebClient
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding, bool useGzip = false);
    }
        public class HttpClientHandler : IHttpClient
        {
            private static HttpClient _staticClient;
            private readonly HttpClient _client;
            public bool IsStaticClient { get; private set; }
            private readonly IInternalLogger _internalLogger;

            public HttpClientHandler(BulkSenderOptions options)
            {
                _internalLogger = new InternalLogger.InternalLogger(options.DebugLogFile);
                if (options.UseStaticHttpClient)
                {
                    if (_staticClient == null)
                    {
                        _staticClient = CreateHttpClient(options.ProxyAddress);

                    }
                    _client = _staticClient;
                    IsStaticClient = true;
                    if (options.Debug)
                    {
                        _internalLogger.Log("Logz.io: Created static HTTP/s client.");        
                    }
                
                }
                else
                {
                    _client = CreateHttpClient(options.ProxyAddress);
                    IsStaticClient = false;
                    if (options.Debug)
                    {
                        _internalLogger.Log("Logz.io: Created HTTP/s client.");

                    }
                }
            }

            private HttpClient CreateHttpClient(string proxyAddress)
            {
                HttpClient client;
                string logzioVersion = Environment.GetEnvironmentVariable("LOGZIO_VERSION");
                if (!string.IsNullOrEmpty(proxyAddress))
                {
                    var handler = new System.Net.Http.HttpClientHandler
                    {
                        UseProxy = true,
                        Proxy = new Proxy(new Uri(proxyAddress))
                    };
                    client = new HttpClient(handler: handler);
                }
                else
                {
                    client = new HttpClient();
                }
                client.DefaultRequestHeaders.Add("User-Agent", $"dotnet/{logzioVersion} logs");
                return client;
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
