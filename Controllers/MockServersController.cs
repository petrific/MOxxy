using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moxxy.Mock;
using Newtonsoft.Json;

namespace Moxxy.Controllers
{
    [Route("api/[controller]")]
    public class MockServersController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<ServerData> Get()
        {
			return MockServerBroker.Instance.GetServers();
        }


        [HttpPut("{serverName}/activation")]
        public async Task<ServerData> Put(string serverName, [FromBody]bool active)
        {
			if(active){
				await MockServerBroker.Instance.ActivateServer(serverName);
			}
			else{
				await MockServerBroker.Instance.StopServer(serverName);
			}

			return MockServerBroker.Instance.Find(serverName);
		}

		[HttpGet("{serverName}/Download")]
		public FileResult DownloadServerConfig(string serverName)
		{
			var server = MockServerBroker.Instance.Find(serverName);
			if (server == null)
			{
				return null;
			}
			string toJson = JsonConvert.SerializeObject(server, Formatting.Indented);
			return File(Encoding.UTF8.GetBytes(toJson), "text/plain", $"{serverName}.json");
		}

		[HttpPost]
		public async Task<ActionResult> AddServer([FromBody] ServerData server)
		{
			if(server == null || !server.IsValid())
			{
				return BadRequest();
			}

			if(MockServerBroker.Instance.TryAddServer(server)){
				var res = await MockServerBroker.Instance.ActivateServer(server.Name);
				if(res == MockServerActivationTaskResult.Started)
				{
					return Ok();
				}
			}

			return BadRequest();
		}

		[HttpGet("{serverName}/PromoteRoutes")]
		public ActionResult PromotePassthroughRoutes(string serverName){
			if(string.IsNullOrEmpty(serverName)){
				return BadRequest();
			}

			MockServerBroker.Instance.PromotePassthroughRoutesToPermanent(serverName);
			return Ok();
		}
	}
}
