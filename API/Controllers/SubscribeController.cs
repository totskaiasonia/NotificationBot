using API.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscribeController : ControllerBase
    {
        private string _connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        public SubscribeController()
        {
        }
        // POST api/<SubscribeController>
        [EnableCors("_myAllowSpecificOrigins")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]SubscribeEventModel subscribeEvent)
        {
            const string queueName = "eventsnotifications";

            var queueClient = new QueueClient(this._connectionString, queueName);

            try
            {
                await queueClient.CreateIfNotExistsAsync();

                var sendResponse = await queueClient.SendMessageAsync(JsonSerializer.Serialize(subscribeEvent));

                return Ok(new { msgId = sendResponse.Value.MessageId, popReceipt = sendResponse.Value.PopReceipt });
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
