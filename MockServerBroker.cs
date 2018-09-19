using Moxxy.Mock;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moxxy
{
	/// <summary>
	/// Singleton in charge of managing mock servers.
	/// </summary>
    public class MockServerBroker
    {
		private static MockServerBroker instance;

		private ConcurrentDictionary<string, ServerData> activeServers;
		public bool ShutdownRequired = false;
		
		public static MockServerBroker Instance {
			get {
				
				if(instance == null){
					instance = new MockServerBroker();
				}
				return instance;
			}
		}

		private MockServerBroker()
		{
			this.activeServers = new ConcurrentDictionary<string, ServerData>();
		}

		public bool TryAddServer(ServerData server)
		{
			if(server == null){
				return false;
			}

			return this.activeServers.TryAdd(server.Name, server);
		}

		public async Task<MockServerActivationTaskResult> ActivateServer(string name, ServerData server = null)
		{
			if(server == null && !this.activeServers.TryGetValue(name, out server)){
				Console.WriteLine($"Could not find server named \"{server.Name}\"");
				return MockServerActivationTaskResult.FailedToStart;
			}

			if(server.Active && server.ServerInstance != null){
				Console.WriteLine($"Server \"{server.Name}\" on {server.Path} is already active.");
				return MockServerActivationTaskResult.FailedToStart;
			}

			try {
				Console.WriteLine($"Creating server host \"{server.Name}\" on {server.Path}");
				server.ServerInstance = MockHostBuilder.CreateMockHostBuilder(server.Name, server.Path);
				Console.WriteLine($"Activating server \"{server.Name}\" on {server.Path}");
				await server.ServerInstance.StartAsync();
				Console.WriteLine($"Activated server \"{server.Name}\" on {server.Path}");
				server.Active = true;
			}catch(Exception e){
				Console.WriteLine($"ERROR: Could not start server \"{server.Name}\". Message is: {e.Message}");
				return MockServerActivationTaskResult.FailedToStart;
			}

			return MockServerActivationTaskResult.Started;
		}

		public async Task<MockServerActivationTaskResult> StopServer(string name, ServerData server = null)
		{
			if(server == null && !this.activeServers.TryGetValue(name, out server)){
				Console.WriteLine($"Could not find server named {name}");
				return  MockServerActivationTaskResult.FailedToStop;
			}

			if (!server.Active)
			{
				Console.WriteLine($"Server \"{server.Name}\" on {server.Path} is already inactive.");
				return MockServerActivationTaskResult.FailedToStop;
			}

			Console.WriteLine($"Stopping server \"{server.Name}\" on {server.Path}");
			await server.ServerInstance.StopAsync();
			Console.WriteLine($"Stopped server \"{server.Name}\" on {server.Path}");
			Console.WriteLine($"Clearing host instance for server \"{server.Name}\" on {server.Path}");
			server.ServerInstance.Dispose();
			server.ServerInstance = null;
			server.Active = false;
			return MockServerActivationTaskResult.Stopped;
		}

		public ServerData Find(string key)
		{
			return this.activeServers.GetValueOrDefault(key);
		}

		public ServerData[] GetServers()
		{
			return this.activeServers.Select(kvp => kvp.Value).ToArray();
		}

		public void PromotePassthroughRouteToPermanent(ServerData server, MockRouteData route)
		{
			if(server == null || !server.PassthroughRecords.Contains(route)){
				Console.WriteLine($"Cannot promote route \"{route.Path}\" to permanent in server \"{server.Name}\" because the server does not contain that route.");
				return;
			}

			server.Routes = server.Routes.Append(route).ToArray();
			Console.WriteLine($"Promoted route \"{route.Path}\" to permanent in server \"{server.Name}\"");
		}

		public void PromotePassthroughRoutesToPermanent(ServerData server)
		{
			if (server == null || !server.PassthroughRecords.Any())
			{
				Console.WriteLine($"Cannot promote routes to permanent in server \"{server.Name}\". there are no routes");
				return;
			}

			foreach(var route in server.PassthroughRecords){
				this.PromotePassthroughRouteToPermanent(server, route);
			}
			Console.WriteLine($"Promoted routes to permanent in server \"{server.Name}\"");
		}
	}
}


public enum MockServerActivationTaskResult {
	Started = 0,
	Stopped,
	FailedToStart,
	FailedToStop
}
