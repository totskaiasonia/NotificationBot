using API.Models;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private string _connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        private string _tableName = "events";
        private string _queueName = "events-news-notifications";

        private TableClient _tableClient;
        private QueueClient _queueClient;

        public EventController()
        {
            _tableClient = new TableClient(_connectionString, _tableName);
            _tableClient.CreateIfNotExists();

            _queueClient = new QueueClient(_connectionString, _queueName);
            _queueClient.CreateIfNotExists();
        }

        // POST api/<SubscribeController>
        [EnableCors("_myAllowSpecificOrigins")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]EventModel subscribeEvent)
        {
            await Console.Out.WriteLineAsync();
            try
            {
                EventEntity eventEntity = new EventEntity
                {
                    PartitionKey = subscribeEvent.Category.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    Title = subscribeEvent.Title,
                    Date = subscribeEvent.Date,
                    Category = subscribeEvent.Category,
                    UsersIds = JsonSerializer.Serialize(subscribeEvent.UsersIds),
                };
                await Console.Out.WriteLineAsync(eventEntity.ToString());
                await _tableClient.AddEntityAsync(eventEntity);

                var sendResponse = await _queueClient.SendMessageAsync(JsonSerializer.Serialize(subscribeEvent));

                return Ok(new { msgId = sendResponse.Value.MessageId, popReceipt = sendResponse.Value.PopReceipt });
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpPatch("subscribe/{categoryId}/{eventId}")]
        public async Task<IActionResult> Subscribe(string categoryId, string eventId, [FromBody] UserEntity subscriber)
        {
            // create entity in table storage
            const string tableName = "events";
            var tableClient = new TableClient(_connectionString, tableName);

            tableClient.CreateIfNotExists();


            EventEntity eventEntity = await _tableClient.GetEntityAsync<EventEntity>(categoryId, eventId);

            if (eventEntity == null)
            {
                return NotFound();
            }

            //eventEntity.UsersIds.Append(subscriber.RowKey);
            List <string> usersIds = JsonSerializer.Deserialize<List<string>>("[]");
            usersIds.Add(subscriber.RowKey);
            eventEntity.UsersIds = JsonSerializer.Serialize(usersIds);
            await tableClient.UpdateEntityAsync(eventEntity, eventEntity.ETag);

            return Ok();
        }
    }
}
