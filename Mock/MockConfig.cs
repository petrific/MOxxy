using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Moxxy.Mock
{
	public class ServerData
	{
		public string Name;
		public string Path;
		public bool PassthroughOnFail;
		public ProxyOptions PassthroughPath;
		public MockRouteData[] Routes;

		public bool Active;
		internal HashSet<MockRouteData> PassthroughRecords;

		internal IWebHost ServerInstance;

		public ServerData()
		{
			this.PassthroughRecords = new HashSet<MockRouteData>();
		}

		public static ServerData LoadFrom(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException($"Could not find file {path} to initialize mock server.");
			}

			var fs = new FileStream(path, FileMode.Open);
			var reader = new StreamReader(fs);
			var data = reader.ReadToEnd();
			reader.Close();
			return JsonConvert.DeserializeObject<ServerData>(data);
		}

		public bool IsValid()
		{
			return (!string.IsNullOrEmpty(this.Name) && !string.IsNullOrEmpty(Path));
		}
	}

	public struct MockRouteData
	{
		public NamedParameter[] Headers;
		public NamedParameter[] Parameters;
		public string Path;
		public string Method;
		public HttpStatusCode Code;
		public string Response;

		public static MockRouteData GeneratePassthroughRecord(HttpContext context)
		{
			MockRouteData route = new MockRouteData
			{
				Path = context.Request.Path,
				Method = context.Request.Method,
				Headers = context.Request.Headers.Select(kvp => new NamedParameter { Key = kvp.Key, Value = kvp.Value.ToString() }).ToArray(),
				Parameters = context.Request.Query.Select(kvp => new NamedParameter { Key = kvp.Key, Value = kvp.Value.ToString() }).ToArray()
			};

			return route;
		}
	}

	public struct NamedParameter
	{
		public string Key;
		public string Value;
	}
}
