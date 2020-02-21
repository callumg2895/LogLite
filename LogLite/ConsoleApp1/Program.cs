using LogLite.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace LogLite.Scratchpad
{
	class Program
	{
		static void Main(string[] args)
		{
			LogLiteLogger logger = LogLiteLogger.Instance;

			logger.Log("Hello, World!");


			using (var scope = logger.BeginScope("new scope"))
			{
				logger.Log("Hello, World!");
			};


			logger.Log("Hello, World!");

			logger.Dispose();
		}
	}
}
