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
        Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding);
    }

    public class HttpClientHandler : IHttpClient
    {
        private readonly HttpClient _client;
        private static readonly System.Text.Encoding _encodingUtf8 = new System.Text.UTF8Encoding(false);
        private readonly MediaTypeHeaderValue _headerValue = new MediaTypeHeaderValue("text/html") { CharSet = _encodingUtf8.WebName };

        public HttpClientHandler()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, MemoryStream body, System.Text.Encoding encoding)
        {
            var content = new StreamContent(body);
            if (_encodingUtf8.WebName == encoding.WebName)
                content.Headers.ContentType = _headerValue;
            else
                content.Headers.ContentType = new MediaTypeHeaderValue("text/html") { CharSet = encoding.WebName };

            content.Headers.Add("Content-Encoding", "gzip");

            return await _client.PostAsync(url, content);
        }
    }

}