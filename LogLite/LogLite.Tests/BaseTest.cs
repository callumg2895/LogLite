using LogLite.Core;
using LogLite.Core.Sinks;
using LogLite.Tests.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LogLite.Tests
{
	[TestClass]
	public class BaseTest
	{
		protected static ILoggerFactory loggerFactory;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{

		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{

		}
	}
}
