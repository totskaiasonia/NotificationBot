using API.Models;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text.Json;

namespace API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private string _connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        const string tableName = "users";

        private TableClient _tableClient;

        public UserController()
        {
            _tableClient = new TableClient(_connectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        [HttpGet]
        [Route("email")]
        public async Task<IActionResult> GetByEmail([FromBody] UserModel user)
        {
            try
            {
                AsyncPageable<TableEntity> results = _tableClient.QueryAsync<TableEntity>(filter: $"Email eq '{user.Email}'");
                await foreach (TableEntity entity in results)
                {
                    Console.WriteLine(entity.GetString("RowKey"));
                    return Ok(entity.GetString("RowKey"));
                }
                return NotFound();
            }
            catch (Exception err)
            {
                return BadRequest(err);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserModel user)
        {

            UserEntity userEntity = new UserEntity
            {
                PartitionKey = "User",
                RowKey = Guid.NewGuid().ToString(),
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
            };
            await _tableClient.AddEntityAsync(userEntity);

            return Ok();
        }


        [HttpPatch("add-tg/{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UserModel user)
        {
            await Console.Out.WriteLineAsync(id);

            UserEntity userEntity = await _tableClient.GetEntityAsync<UserEntity>("User", id);
            
            if (userEntity != null)
            {
                userEntity.TelegramId = user.TelegramId;

                await _tableClient.UpdateEntityAsync(userEntity, userEntity.ETag);
            }

            return Ok();
        }
    }
}
