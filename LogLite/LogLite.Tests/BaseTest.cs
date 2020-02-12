using LogLite.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace LogLite.Tests
{
	[TestClass]
	public class BaseTest
	{
		private static Logger logger = new Logger();

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			Task task = logger.StartAsync(new CancellationToken());

			task.RunSynchronously();
		}

		[TestMethod]
		public void LoggerHasStarted()
		{
			Assert.IsTrue(logger.IsStarted());
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			Task task = logger.StopAsync(new CancellationToken());

			task.RunSynchronously();
		}
	}
}
