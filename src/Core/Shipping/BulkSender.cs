using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Logzio.DotNet.Core.Shipping
{
	public class BulkSender
	{
		private const string LogzioHost = "listener.logz.io";
		private const string HttpUrlTemplate = "http://" + LogzioHost + ":8070/?token={0}&type={1}";
		private const string HttpsUrlTemplate = "https://" + LogzioHost + ":8071/?token={0}&type={1}";

		private readonly BulkSenderOptions _options;

		public BulkSender(BulkSenderOptions options)
		{
			_options = options;
		}

		public void Send(ICollection<LogEvent> logz, int attempt = 0)
		{
			var url = string.Format(_options.IsSecured ? HttpsUrlTemplate : HttpUrlTemplate, _options.Token, _options.Type);
			try
			{
				using (var client = new WebClient())
				{
					client.UploadString(url, JsonConvert.SerializeObject(logz.Select(x => x.LogData)));
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Logzio.DotNet ERROR: " + ex);

				if (attempt < _options.RetriesMaxAttempts - 1)
					Task.Delay(_options.RetriesInterval).ContinueWith((task) => Send(logz, attempt + 1));
			}
		}

		public void SendAsync(ICollection<LogEvent> logz)
		{
			Task.Run(() => Send(logz, 0));
		}
	}
}