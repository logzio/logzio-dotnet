using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Logzio.DotNet.Core.WebClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Logzio.DotNet.Core.Shipping
{
    public interface IBulkSender
    {
        Task<HttpResponseMessage> SendAsync(ICollection<LogzioLoggingEvent> logz, BulkSenderOptions options);
    }

    public class BulkSender : IBulkSender
    {
        private const string UrlTemplate = "{0}/?token={1}&type={2}";

        private readonly JsonSerializer _jsonSerializer;
        private static readonly System.Text.Encoding _encodingUtf8 = new System.Text.UTF8Encoding(false);
        private readonly IHttpClient _httpClient;

        public BulkSender(IHttpClient httpClient)
        {
            var jsonSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _jsonSerializer = JsonSerializer.CreateDefault(jsonSettings);
            _httpClient = httpClient;
        }

        public Task<HttpResponseMessage> SendAsync(ICollection<LogzioLoggingEvent> logz, BulkSenderOptions options)
        {
            if (logz == null || logz.Count == 0)
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var url = string.Format(UrlTemplate, options.ListenerUrl, options.Token, options.Type);
            var body = SerializeLogEvents(logz, _encodingUtf8);
            return _httpClient.PostAsync(url, body, _encodingUtf8);
        }

        private MemoryStream SerializeLogEvents(ICollection<LogzioLoggingEvent> logz, System.Text.Encoding encodingUtf8)
        {
            var ms = new MemoryStream(logz.Count * 512);
            var zipStream = new GZipStream(ms, CompressionLevel.Optimal);
            using (var sw = new StreamWriter(zipStream, encodingUtf8, 1024, true))
            {
                bool firstItem = true;
                foreach (var logEvent in logz)
                {
                    if (!firstItem)
                        sw.WriteLine();
                    _jsonSerializer.Serialize(sw, logEvent.LogData);
                    firstItem = false;
                }
                sw.Flush();
            }
            ms.Position = 0;
            return ms;
        }
    }
}