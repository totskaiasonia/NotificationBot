using API.Models;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private string _connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        public EventController()
        {
        }
        // POST api/<SubscribeController>
        [EnableCors("_myAllowSpecificOrigins")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]EventModel subscribeEvent)
        {
            // create entity in table storage
            const string tableName = "events";
            var tableClient = new TableClient(_connectionString, tableName);

            tableClient.CreateIfNotExists();


            EventEntity eventEntity = new EventEntity
            {
                PartitionKey=subscribeEvent.Category.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Title = subscribeEvent.Title,
                Date = subscribeEvent.Date,
                Category = subscribeEvent.Category,
            };
            await tableClient.AddEntityAsync(eventEntity);



            // send msg to queue
            const string queueName = "events-news-notifications";

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
