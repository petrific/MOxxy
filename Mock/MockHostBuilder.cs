using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Moxxy.Mock
{
    public class MockHostBuilder
    {
		public const string MockSourceKey = "MockSource";
		public static IWebHost CreateMockHostBuilder (string serverName, string serverPath)
		{
			var webHost = new WebHostBuilder()
				 .UseKestrel()
				 .UseContentRoot(Directory.GetCurrentDirectory())
				 .ConfigureAppConfiguration((hostingContext, config) =>
				 {
					 var env = hostingContext.HostingEnvironment;
					 config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
						   .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
					 config.AddEnvironmentVariables();
					 config.AddCommandLine(new string[] { $"/{MockSourceKey}={serverName}" });
				 }).UseStartup<MockStartup>()
				   .UseUrls(serverPath)
				   .Build();
			
			return webHost;
		}

	}
}
