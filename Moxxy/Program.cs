using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Moxxy
{
	class Program
	{
		static void Main(string[] args)
		{

			var config = LoadConfig();
			if(config == null){
				return;
			}
			MockServer server = new MockServer(config);
			server.Run();
			Console.ReadKey();
			server.Stop();
		}

		private static ServerData LoadConfig()
		{
			ServerData data = null;
			JsonReader jsonReader = null;
			try
			{
				FileStream fs = new FileStream("config.json", FileMode.Open);
				StreamReader reader = new StreamReader(fs);
				jsonReader = new JsonTextReader(reader);
				data = Newtonsoft.Json.JsonSerializer.Create().Deserialize<ServerData>(jsonReader);
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine("Could not find config.json");
			}
			finally
			{
				if (jsonReader != null)
				{
					jsonReader.Close();
				}
			}

			return data;
		}
	}

	public class ServerData {
		public Uri Path;
		public bool PassthroughOnFail;
		public Uri PassthroughPath;
		public RouteData[] Routes;
	}

	public class RouteData
	{
		internal Uri Uri;
		public NamedParameter[] Headers;
		public NamedParameter[] Parameters;
		public string Path;
		public string Method;
		public HttpStatusCode Code;
		public string Response;
	}

	public class NamedParameter{
		public string Key;
		public string Value;
	}
}
