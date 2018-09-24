using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moxxy.Mock;

namespace Moxxy
{
	public class Program
	{
		public static object MockConfig { get; private set; }

		public static void Main(string[] args)
		{
			StartupParameters parameters;
			var manager = BuildWebHost();
			manager.Start();
			try
			{
				parameters = new StartupParameters(args);
				MockServerBroker.Instance.InitFromParameters(parameters).Wait();
			}
			catch (ArgumentException)
			{
				Console.WriteLine("Usage:");
				Console.WriteLine("-server-files|sf <server-file-path>.json -buildmode|b -server-route|s <server route> -fallback-route|f <passthrough route> -server-name|n <server name> -server-path|p <server file path>");
			}

			
			while(!MockServerBroker.Instance.ShutdownRequired){
				Thread.Sleep(1000);
			}

			manager.StopAsync().Wait();
		}

		public static IWebHost BuildWebHost() =>
			WebHost.CreateDefaultBuilder()
				.UseStartup<Startup>()
				.Build();

	}

	public struct StartupParameters
	{
		public string[] ServerFiles;
		public bool BuildMode;
		public string ServerRoute;
		public string PassthroughRoute;
		public string ServerName;
		public string ServerPath;

		public StartupParameters(string[] args)
		{
			ServerFiles = new string[0];
			BuildMode = false;
			ServerRoute = string.Empty;
			PassthroughRoute = string.Empty;
			ServerName = string.Empty;
			ServerPath = string.Empty;

			for (int i = 0; i < args.Length; i++)
			{
				var arg = args[i].ToLowerInvariant();
				if (arg == "-server-files" || arg == "-sf" && i + 1 < args.Length)
				{
					var file = args[++i];
					if (!File.Exists(file))
					{
						throw new ArgumentException($"Server file {file} does not exist.");
					}
					ServerFiles = ServerFiles.Append(file).ToArray();
					continue;
				}

				if (arg == "-buildmode" || arg == "-b")
				{
					BuildMode = true;
					continue;
				}

				if (arg == "-server-route" || arg == "-s" && i + 1 < args.Length)
				{
					ServerRoute = args[++i];
					continue;
				}

				if (arg == "-passthrough-route" || arg == "-ps" && i + 1 < args.Length)
				{
					PassthroughRoute = args[++i];
					continue;
				}

				if (arg == "-server-name" || arg == "-n" && i + 1 < args.Length)
				{
					ServerName = args[++i];
					continue;
				}

				if (arg == "-server-path" || arg == "-p" && i + 1 < args.Length)
				{
					var file = args[++i];
					if (File.Exists(file))
					{
						throw new ArgumentException($"Server file {file} already exist.");
					}

					ServerPath = file;
					continue;
				}

				throw new ArgumentException();
			}
		}
	}
}
