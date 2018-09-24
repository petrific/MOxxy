using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Moxxy.Mock
{
	public class MockStartup
	{
		private MockRouter router;

		public MockStartup(IConfiguration configuration)
		{

			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddRouting();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			var serverData = MockServerBroker.Instance.Find(Configuration[MockHostBuilder.MockSourceKey]);
			this.router = new MockRouter(serverData);
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseRouter(this.router);
			app.UseMiddleware(typeof(ResponseRewindMiddleware), serverData).MapWhen((context) =>
			{
				return !this.router.ProcessRequest(context, false).Result;
			}, builder => builder.RunProxy(serverData.PassthroughPath))
			.UseMvc();
		}

	}

	public class ResponseRewindMiddleware
	{
		private readonly RequestDelegate next;
		private readonly ServerData server;

		public ResponseRewindMiddleware(RequestDelegate next, ServerData server)
		{
			this.server = server;
			this.next = next;
		}

		public async Task Invoke(HttpContext context)
		{

			Stream originalBody = context.Response.Body;

			try
			{
				using (var memStream = new MemoryStream())
				{
					context.Response.Body = memStream;

					await next(context);

					memStream.Position = 0;
					string responseBody = new StreamReader(memStream).ReadToEnd();
					// Not very optimal, but does the trick for now.
					var routeData = MockRouteData.GeneratePassthroughRecord(context);
					routeData.Code = (HttpStatusCode) context.Response.StatusCode;
					routeData.Response = responseBody;
					if(!server.PassthroughRecords.Add(routeData)){
						// Do something here
					}
					if (server.BuildMode)
					{
						MockServerBroker.Instance.PromotePassthroughRouteToPermanent(server, routeData);
						MockServerBroker.Instance.SaveBuildServer();
					}
					memStream.Position = 0;
					await memStream.CopyToAsync(originalBody);
				}

			}
			finally
			{
				context.Response.Body = originalBody;
			}

		}
	}
}
