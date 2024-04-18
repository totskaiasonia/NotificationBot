using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MonitorQueueFunction
{
    public class MonitorQueue
    {
        private static readonly HttpClient httpClient = new HttpClient();
        [FunctionName("MonitorQueue")]
        public async static Task Run(
            [TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
            ILogger log)
        {
            string connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            const string queueName = "eventsnotifications";
            var queueClient = new QueueClient(connectionString, queueName);
            await queueClient.CreateIfNotExistsAsync();

            int queueLength = queueClient.MaxPeekableMessages;
            await Console.Out.WriteLineAsync($"{queueLength}");
            PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(queueLength);

            bool f = false;
            foreach (PeekedMessage message in peekedMessages)
            {
                var obj = JsonSerializer.Deserialize<EventModel>(message.Body);
                DateTime today = DateTime.Now;
                DateTime toCheck = obj.Date;

                TimeSpan diff = toCheck - today;
                if (diff.TotalDays < 1)
                {
                    f = true;
                    Console.WriteLine("Less then 1 day " + obj.Name);
                }
                Console.WriteLine($"Message info. Name {obj.Name}, Date: {obj.Date}");
            }
            if (f)
            {
                Console.WriteLine("Inside sending");
                string url = "http://localhost:3978/api/notify";
                HttpResponseMessage response = await httpClient.GetAsync(url);

            }
            log.LogInformation($"Queue checked");
        }
    }
}
