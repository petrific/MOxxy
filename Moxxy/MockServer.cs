using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Moxxy
{
	public class MockServer
	{
		private HttpListener listener;
		private ServerData server;
		private Random rng = new Random(1);

		public MockServer(ServerData server) {
			this.server = server;
			this.listener = new HttpListener();
			this.listener.Prefixes.Add(this.server.Path.AbsoluteUri);
			foreach(RouteData route in this.server.Routes){
				route.Uri = new Uri(server.Path, route.Path.ToLowerInvariant());
			}
			this.listener.Start();
		}

		public void Run()
		{
			ThreadPool.QueueUserWorkItem(w =>
			{
				Console.WriteLine("Running...");
				try {
					while(this.listener.IsListening){
						ThreadPool.QueueUserWorkItem(c =>
						{
							var context = c as HttpListenerContext;
							try
							{
								Console.WriteLine("SERVING");
								if(!this.ProcessRequest(context) && this.server.PassthroughOnFail){
									var passthroughUri = new Uri(this.server.PassthroughPath, context.Request.Url.AbsolutePath);
									var passthroughRequest = WebRequest.Create(passthroughUri);
									foreach (string kvp in context.Request.Headers.Keys) {
										HttpRequestHeader headerName;
										bool isDefault = Enum.TryParse<HttpRequestHeader>(kvp, out headerName);
										if (isDefault)
										{
											continue;
										}
										try
										{
											passthroughRequest.Headers.Add(kvp, context.Request.Headers[kvp]);
										}
										catch { }
									}
									var response = passthroughRequest.GetResponse().GetResponseStream();
									response.CopyTo(context.Response.OutputStream);
								}
							}
							catch (Exception e) { }
							finally {
								context.Response.OutputStream.Close();
							}

						}, this.listener.GetContext());
					}
				}catch{ }
			});
		}

		public void Stop()
		{
			this.listener.Stop();
			this.listener.Close();
		}

		private bool ProcessRequest(HttpListenerContext context)
		{
			var request = context.Request;
			var uri = request.Url;
			RouteData selectedRoute = null;
			bool isExactMatch = false;
			foreach(RouteData route in this.server.Routes.Where(r => r.Method == request.HttpMethod)){
				if(selectedRoute != null && isExactMatch){
					break;
				}

				bool allowedWildcards = false;
				if(!MatchUri(route.Uri, uri, ref allowedWildcards))
				{
					continue;
				}

				if(!MatchHeaders(route.Headers, request.Headers, ref allowedWildcards)){
					continue;
				}

				isExactMatch = !allowedWildcards;
				selectedRoute = route;
			}

			if(selectedRoute == null)
			{
				return false;
			}

			string str = selectedRoute.Response;
			byte[] buffer = Encoding.UTF8.GetBytes(str);
			context.Response.StatusCode = (int) selectedRoute.Code;
			context.Response.ContentLength64 = buffer.Length;
			context.Response.OutputStream.Write(buffer, 0, buffer.Length);
			return true;
		}

		private bool MatchHeaders(HeaderData[] routeHeaders, NameValueCollection requestHeaders, ref bool allowedWildcards)
		{
			if(routeHeaders == null || routeHeaders.Length == 0){
				bool matchesHeaderCount = requestHeaders.Count == 0;
				allowedWildcards |= !matchesHeaderCount;
				return true;
			}

			bool allowedWildcardsInHeaders = false;
			foreach (HeaderData header in routeHeaders) {
				string value = requestHeaders[header.Key];
				if (string.IsNullOrEmpty(value)) {
					return false;
				}

				bool allowWildcard = header.Value.Contains("*");
				if (!allowWildcard && header.Value != value) {
					return false;
				}

				allowedWildcardsInHeaders |= allowWildcard;
			}

			allowedWildcards |= allowedWildcardsInHeaders;
			return true;
		}

		private bool MatchUri(Uri routeUri, Uri requestUri, ref bool usedWildcard)
		{
			if (routeUri.Segments.Length != requestUri.Segments.Length)
			{
				return false;
			}
			for (int i = 0; i < requestUri.Segments.Length; i++)
			{
				string routeSegment = routeUri.Segments[i];
				string uriSegment = requestUri.Segments[i];
				var currentSegmentAllowsWildcards = routeSegment.StartsWith("*");
				usedWildcard |= currentSegmentAllowsWildcards;

				if (!this.Compare(routeSegment, uriSegment, ref usedWildcard))
				{
					return false;
				}
			}
			return true;
		}

		private bool Compare(string target, string value, ref bool allowedWildcards)
		{
			var allowWildcards = target.StartsWith("*");
			allowedWildcards |= allowWildcards;
			return allowWildcards || target.ToLowerInvariant() == value;
		}
	}
}
