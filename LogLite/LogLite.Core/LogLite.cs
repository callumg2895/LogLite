using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogLite.Core
{
	public sealed class Logger : IHostedService
	{
		private bool isStarted = false;

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return new Task(() =>
			{
				isStarted = true;
			});
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return new Task(() =>
			{
				isStarted = false;
			});
		}

		public bool IsStarted()
		{
			return isStarted;
		}
	}
}
