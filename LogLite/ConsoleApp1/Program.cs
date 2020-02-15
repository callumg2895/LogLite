﻿using LogLite.Core;
using System;
using System.Threading;

namespace LogLite.Scratchpad
{
	class Program
	{
		static void Main(string[] args)
		{
			Logger logger = Logger.Instance;

			logger.Log("Hello, World!");

			logger.Dispose();
		}
	}
}
