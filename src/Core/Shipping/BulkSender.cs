using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Logzio.Community.Core.InternalLogger;
using Logzio.Community.Core.WebClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Logzio.Community.Core.Shipping
{
	public interface IBulkSender
	{
		Task SendAsync(ICollection<LogzioLoggingEvent> logz, BulkSenderOptions options);
		void Send(ICollection<LogzioLoggingEvent> logz, BulkSenderOptions options, int attempt = 0);
	}

	public class BulkSender : IBulkSender
	{
		private const string UrlTemplate = "{0}/?token={1}&type={2}";

		private readonly IWebClientFactory _webClientFactory;
		private readonly IInternalLogger _internalLogger;

		private readonly JsonSerializer _jsonSerializer;

		public BulkSender(IWebClientFactory webClientFactory, IInternalLogger internalLogger)
		{
		    _webClientFactory = webClientFactory;
		    _internalLogger = internalLogger;
		    _jsonSerializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
		}

		public Task SendAsync(ICollection<LogzioLoggingEvent> logz, BulkSenderOptions options)
		{
			return Task.Run(() => Send(logz, options));
		}

		public void Send(ICollection<LogzioLoggingEvent> logz, BulkSenderOptions options, int attempt = 0)
		{
		    if (logz == null || logz.Count == 0)
		        return;

			var url = string.Format(UrlTemplate, options.ListenerUrl, options.Token, options.Type);
			try
			{
				using (var client = _webClientFactory.GetWebClient())
				{
					var serializedLogLines = logz.Select(x => Serialize(x.LogData)).ToArray();
					var body = String.Join(Environment.NewLine, serializedLogLines);
					client.UploadString(url, body);
					if (options.Debug)
						_internalLogger.Log("Sent bulk of [{0}] log messages to [{1}] successfully.", logz.Count, url);
				}
			}
			catch (Exception ex)
			{
				if (options.Debug)
					_internalLogger.Log("Logzio.DotNet ERROR: " + ex);

				if (attempt < options.RetriesMaxAttempts - 1)
					Task.Delay(options.RetriesInterval).ContinueWith(task => Send(logz, options, attempt + 1));
			}
		}

		private string Serialize(object obj)
		{
			using (var sb = new StringWriter())
			{
				_jsonSerializer.Serialize(sb, obj);
				return sb.ToString();
			}
		}
	}
}