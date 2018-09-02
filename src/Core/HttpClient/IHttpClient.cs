using System;
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
        private readonly HttpClient _client = new HttpClient();
        private static readonly System.Text.Encoding _encodingUtf8 = new System.Text.UTF8Encoding(false);
        private readonly MediaTypeHeaderValue _headerValue = new MediaTypeHeaderValue("application/json") { CharSet = _encodingUtf8.WebName };
       
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
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = encoding.WebName };
            return await _client.PostAsync(url, content);
        }
    }

}