using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Logzio.DotNet.Core.Shipping
{
	public class BulkSender
	{
		private const string LogzioHost = "listener.logz.io";
		private const string HttpUrlTemplate = "http://" + LogzioHost + ":8070/?token={0}&type={1}";
		private const string HttpsUrlTemplate = "https://" + LogzioHost + ":8071/?token={0}&type={1}";

		private readonly BulkSenderOptions _options;
		private readonly JsonSerializer _jsonSerializer;

		public BulkSender(BulkSenderOptions options)
		{
			_options = options;
			_jsonSerializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
		}

		public string Serialize(object obj)
		{
			using (var sb = new StringWriter())
			{
				_jsonSerializer.Serialize(sb, obj);
				return sb.ToString();
			}
		}

		public void Send(ICollection<LogEvent> logz, int attempt = 0)
		{
			var url = string.Format(_options.IsSecured ? HttpsUrlTemplate : HttpUrlTemplate, _options.Token, _options.Type);
			try
			{
				using (var client = new WebClient())
				{
					var serializedLogLines = logz.Select(x => Serialize(x.LogData)).ToArray();
					var body = String.Join(Environment.NewLine, serializedLogLines);
					client.UploadString(url, body);
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Logzio.DotNet ERROR: " + ex);

				if (attempt < _options.RetriesMaxAttempts - 1)
					Task.Delay(_options.RetriesInterval).ContinueWith(task => Send(logz, attempt + 1));
			}
		}

		public void SendAsync(ICollection<LogEvent> logz)
		{
			Task.Run(() => Send(logz));
		}
	}
}