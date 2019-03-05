using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using IntegrationTests.Listener;

namespace Logzio.DotNet.IntegrationTests.Listener
{
    public class LogzioListenerDummy
    {
        public const string ContentEncodingHeader = "Content-Encoding";
        public const string ContentEncodingGzip = "gzip";

        private readonly static Random _random = new Random();

        public string DefaultUrl { get; } = string.Concat("http://127.0.0.1", ":", _random.Next(7601, 7699).ToString(), "/");

        public IList<HttpRequest> Requests { get; set; }

        private bool _isActive;
        private HttpListener _httpListener;

        public void Start(string url = null)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url ?? DefaultUrl);
            _httpListener.Start();
            _isActive = true;
            Requests = new List<HttpRequest>();
            _httpListener.BeginGetContext(OnContext, null);
        }

        private void OnContext(IAsyncResult ar)
        {
            if (!_isActive)
                return;

            var context = _httpListener.EndGetContext(ar);

            _httpListener.BeginGetContext(OnContext, null);

            var request = context.Request;
            var encodingHeader = context.Request.Headers[ContentEncodingHeader];
            var inStream = request.InputStream;

            if (!string.IsNullOrEmpty(encodingHeader) && encodingHeader == ContentEncodingGzip)
            {
                inStream = new GZipStream(inStream, CompressionMode.Decompress);
            }

            using (var reader = new StreamReader(inStream, request.ContentEncoding))
            {
                var httpRequest = new HttpRequest(reader.ReadToEnd(), context.Request.Headers);
                Requests.Add(httpRequest);
            }

            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        public void Stop()
        {
            _isActive = false;
            _httpListener.Close();
            _httpListener.Abort();

            // TODO: remove sleep ater bug is fixed (milestone of dotnet 2.1.0)
            // Disposing Socket then rebinding fails with SocketError.AddressAlreadyInUse on Unix
            // https://github.com/dotnet/corefx/issues/25016     
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }
}