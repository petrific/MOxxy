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
			var manager = BuildWebHost(args);

			manager.Start();
			
			foreach(string arg in args){
				if(!File.Exists(arg)){
					Console.WriteLine($"Could not find file {arg}.");
					continue;
				}

				var server = ServerData.LoadFrom(arg);
				if(MockServerBroker.Instance.TryAddServer(server)){
					MockServerBroker.Instance.ActivateServer(server.Name).Wait();
				}
			}

			while(!MockServerBroker.Instance.ShutdownRequired){
				Thread.Sleep(1000);
			}

			manager.StopAsync().Wait();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder()
				.UseStartup<Startup>()
				.Build();

	}
}
