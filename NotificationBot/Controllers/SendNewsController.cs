using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using NotificationBot.Models;
using System;

namespace NotificationBot.Controllers
{
    [Route("api/send-news")]
    [ApiController]
    public class SendNewsController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;


        public SendNewsController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] NewsModel data)
        {

            foreach (var conversationReference in _conversationReferences.Values)
            {
                // - Sonia - get conversation refernce by user chat id from table stroage
                await ((BotAdapter)_adapter).ContinueConversationAsync(
                    _appId, 
                    conversationReference, 
                    async (turnContext, cancellationToken) =>
                    {
                        await BotCallback(turnContext, cancellationToken, data);
                    }, 
                    default(CancellationToken));
            }

            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }


        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken, NewsModel data)
        {
            //await turnContext.SendActivitiesAsync(MessageFactory.Attachment())
            // - Sonia - load Card
            await turnContext.SendActivityAsync(MessageFactory.Text($"This event may be interesting for you.\n\n{data.Category}: \"**{data.Title}**\"\n\n*{data.Date.ToShortDateString()} at {data.Date.ToShortTimeString()}*"));
        }
    }
}
