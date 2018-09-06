using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moxxy.Mock
{
	public class MockRouter : IRouter
	{
		private ServerData serverData;

		public MockRouter(ServerData serverData)
		{
			this.serverData = serverData;
		}

		public VirtualPathData GetVirtualPath(VirtualPathContext context)
		{
			// UNUSED FOR NOW
			return null;
		}

		public async Task RouteAsync(RouteContext context)
		{
			var result = await this.ProcessRequest(context.HttpContext);
			if (result || !this.serverData.PassthroughOnFail){
				context.Handler = async (c) =>
				{
					if (!this.serverData.PassthroughOnFail)
					{
						c.Response.StatusCode = 404;
						await c.Response.WriteAsync("Could not find a valid route, and passthrough is disabled.");
					}
				};
			}
		}

		public async Task<bool> ProcessRequest(HttpContext context, bool generateResponse = true)
		{
			var request = context.Request;
			var uri = request.Path;
			MockRouteData? selectedRoute = null;
			bool isExactMatch = false;
			foreach (MockRouteData route in this.serverData.Routes.Where(r => r.Method == request.Method))
			{
				if (selectedRoute.HasValue && isExactMatch)
				{
					break;
				}

				bool allowedWildcards = false;
				if (!MatchUri(route.Path, uri, ref allowedWildcards) ||
				   !MatchValues(route.Headers, request.Headers, ref allowedWildcards) ||
				   !MatchValues(route.Parameters, request.Query, ref allowedWildcards))
				{
					continue;
				}

				isExactMatch = !allowedWildcards;
				selectedRoute = route;
			}

			if (selectedRoute == null)
			{
				return false;
			}

			if (generateResponse)
			{
				string str = selectedRoute?.Response;
				byte[] buffer = Encoding.UTF8.GetBytes(str);
				context.Response.StatusCode = (int)selectedRoute?.Code;
				context.Response.ContentLength = buffer.Length;
				context.Response.Body.Write(buffer, 0, buffer.Length);
			}

			return true;
		}

		private bool MatchValues(NamedParameter[] targets, IEnumerable<KeyValuePair<string, StringValues>> values, ref bool allowedWildcards)
		{
			if (targets == null || targets.Length == 0)
			{
				bool matchesHeaderCount = values.Count() == (targets?.Length ?? -1);
				allowedWildcards |= !matchesHeaderCount;
				return true;
			}

			if (values.Count() < targets.Length)
			{
				return false;
			}

			bool allowedWildcardsInParameters = false;
			foreach (NamedParameter parameter in targets)
			{
				string value = values.Where(v => v.Key == parameter.Key).Select(v => v.Value).FirstOrDefault();
				if (string.IsNullOrEmpty(value))
				{
					return false;
				}

				bool allowWildcard;
				if (!this.Compare(parameter.Value, value, out allowWildcard))
				{
					return false;
				}
				allowedWildcardsInParameters |= allowWildcard;
			}

			allowedWildcards |= allowedWildcardsInParameters;
			return true;
		}

		private bool MatchUri(string routePath, string requestPath, ref bool usedWildcard)
		{
			string[] routeSegments = routePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
			string[] requestSegments = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

			if (routeSegments.Length != requestSegments.Length)
			{
				return false;
			}
			for (int i = 0; i < requestSegments.Length; i++)
			{
				string routeSegment = routeSegments[i];
				string uriSegment = requestSegments[i];
				bool currentSegmentAllowsWildcards;
				if (!this.Compare(routeSegment.ToLowerInvariant(), uriSegment.ToLowerInvariant(), out currentSegmentAllowsWildcards))
				{
					return false;
				}
				usedWildcard |= currentSegmentAllowsWildcards;
			}
			return true;
		}

		private bool Compare(string target, string value, out bool allowedWildcards)
		{
			var allowWildcards = target.StartsWith("*");
			allowedWildcards = allowWildcards;
			return allowWildcards || target == value;
		}

	}
}
