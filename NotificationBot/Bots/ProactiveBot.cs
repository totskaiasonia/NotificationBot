using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using NotificationBot.Models;
using Microsoft.Bot.Builder.Dialogs;

namespace NotificationBot.Bots
{
    public class ProactiveBot<T> : ActivityHandler
        where T : Dialog
    {
        private const string WelcomeMessage = "Welcome to the Proactive Bot sample.  Navigate to http://localhost:3978/api/notify to proactively message everyone who has previously messaged this bot.";

        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private readonly UserState _userState;
        protected readonly BotState _conversationState;

        private IStatePropertyAccessor<UserModel> _userModelAccessor;

        private readonly Dialog _dialog;

        public ProactiveBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, ConversationState conversationState, UserState userState, T dialog)
        {
            _conversationReferences = conversationReferences;
            _userState = userState;
            _conversationState = conversationState;

            _userModelAccessor = _userState.CreateProperty<UserModel>(nameof(UserModel));

            _dialog = dialog;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var userObject = await _userModelAccessor.GetAsync(turnContext, () => new UserModel());
            
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    if (string.IsNullOrEmpty(userObject.Email))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(WelcomeMessage), cancellationToken);
                        await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                    }
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}
