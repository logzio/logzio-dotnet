using System;
using System.Diagnostics;

namespace Logzio.DotNet.Core.InternalLogger
{
	public interface IInternalLogger
	{
		void Log(string message, params object[] args);
	}

	public class InternalLogger : IInternalLogger
	{
		public void Log(string message, params object[] args)
		{
			var formattedMessage = DateTime.Now.ToString("HH:mm:ss.ff") + " - Logz.io - " + String.Format(message, args);
			Trace.WriteLine(formattedMessage);
			Console.WriteLine(formattedMessage);
		}
	}
}