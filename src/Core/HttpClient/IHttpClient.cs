using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Logzio.DotNet.Core.WebClient
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding, bool optionsUseCompression = false);
    }

    public class HttpClientHandler : IHttpClient
    {
        private readonly HttpClient _client;
        private static readonly System.Text.Encoding _encodingUtf8 = new System.Text.UTF8Encoding(false);
        private readonly MediaTypeHeaderValue _headerCompressed = new MediaTypeHeaderValue("text/html") { CharSet = _encodingUtf8.WebName };
        private readonly MediaTypeHeaderValue _headerJson = new MediaTypeHeaderValue("text/html") { CharSet = _encodingUtf8.WebName };

        public HttpClientHandler()
        {
            _client = new HttpClient();
        }
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding, bool useCompression = false)
        {
            var content = new StreamContent(body);
            if (useCompression)
            {
                content.Headers.ContentType = _headerCompressed;
                content.Headers.Add("Content-Encoding", "gzip");
                _client.DefaultRequestHeaders.AcceptEncoding.Clear();
                _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            }
            else
            {
                content.Headers.ContentType = _headerJson;
            }


            return await _client.PostAsync(url, content);
        }
    }

}