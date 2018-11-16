using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BotSurvey.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System.Linq;
using System.Net.Mail;

namespace BotSurvey
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {


        internal static IDialog<MovieForm> MakeRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(MovieForm.BuildForm))
                .Do(async (context, survey) =>
                {
                    try
                    {
                        var completed = await survey;
                        
                        await context.PostAsync("Merci pour votre participation !");
                    }
                    catch (FormCanceledException<MovieForm> e)
                    {
                        string reply;
                        if (e.InnerException == null)
                        {
                            reply = "Vous n’avez pas complété l’enquête !";
                            if (e.LastForm.LocationTheater == LocationTheaterOptions.Autre)
                            {
                                reply = "Désolé nous ne gérons pas encore toutes les localisations";
                            }
                        }
                        else
                        {
                            reply = "Erreur. Essayez plus tard !.";
                        }
                        await context.PostAsync(reply);
                    }
                });
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, MakeRootDialog);
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                if (message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                {
                    var reply = message.CreateReply("Bonjour");

                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));

                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}