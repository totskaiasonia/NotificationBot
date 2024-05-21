using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Bot.Schema;
using NotificationBot.Models;
using System.Collections.Concurrent;

namespace NotificationBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public MainDialog(ILogger<MainDialog> logger, ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(nameof(MainDialog))
        {
            _logger = logger;
            _conversationReferences = conversationReferences;

            var waterfallSteps = new WaterfallStep[]
            {
                AskEmailStep,
                ConfirmEmail,
                EndDialog,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskEmailStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your Email in order to connect to your profile in <Name>Portal")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmEmail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string email = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Confirm your input")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialog(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!(bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(nameof(TextPrompt), "Let's try again.", cancellationToken);
            }

            /*stepContext.Context.Activity.From.Id;*/
            // http request to update user -add id-


            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you. Now you are able to receive notifications about new events, reminders about event on which you have subscribed and metarials from events"));

            return await stepContext.EndDialogAsync(stepContext.Result, cancellationToken);
        }
    }

}
