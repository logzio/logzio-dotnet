using System.Collections.Generic;
using System.Threading.Tasks;

namespace Logzio.DotNet.Core.Shipping
{
	public class BulkSender
	{
		public void Send(ICollection<LogEvent> logz)
		{
			
		}

		public void SendAsync(ICollection<LogEvent> logz)
		{
			Task.Run(() => Send(logz));
		}
	}
}