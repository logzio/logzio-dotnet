using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.HttpClient
{
    public class GzipContent : HttpContent
    {
        private readonly HttpContent originalContent;

        public GzipContent(HttpContent content)
        {
            originalContent = content ?? throw new ArgumentNullException("content");
            Headers.ContentEncoding.Add("gzip");
            foreach (KeyValuePair<string, IEnumerable<string>> header in originalContent.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Stream compressedStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
            return originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
            {
                compressedStream.Dispose();
            });
        }
    }

}
