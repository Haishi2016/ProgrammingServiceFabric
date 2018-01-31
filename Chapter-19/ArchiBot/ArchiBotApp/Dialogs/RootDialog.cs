using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Linq;
using Microsoft.Rest;

namespace ArchiBotApp.Dialogs
{
    [LuisModel("9f6ab1bd-c055-45fd-ac89-7848fe503f5c", "9145fc9f61e048dc9e935d4d641ded2d")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        public RootDialog()
        {

        }
        public RootDialog(ILuisService service)
            :base(service)
        {

        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            saveState(context, "");
            await context.PostAsync("Hi there! This is Azure ArchiBot. I can help you to design your cloud services using proven patterns and practices.");
            await context.PostAsync("What do you want to build today?");
            context.Wait(MessageReceived);
        }

        [LuisIntent("CreateNewApp")]
        public async Task CreateNewWebApp(IDialogContext context, LuisResult result)
        {
            saveState(context, "UseDatabase");
            await context.PostAsync("Okay, I've add a web app with multiple hosting options. Click on the host icon to see the options. You don't have to make a decision now. You can keep exploring and decide later. ");
            await context.PostAsync("Here's an idea... Does your web app use a database?");
            context.Wait(MessageReceived);            
            //await context.PostAsync("Does your web app use a database?");
            //context.Wait(MessageReceived);
        }

        [LuisIntent("Confirm")]
        public async Task Confirm(IDialogContext context, LuisResult result)
        {
            var myLastQuestion = readState(context);
            if (myLastQuestion == "UseDatabase")
                await context.PostAsync("Great! I've added a database node. If you want, you can choose what database to use now. As we work together, I'll remove options that don't fit your scenario anymore. You can change your choice at any time.");
            else if (myLastQuestion == "GlobalApp")
                await context.PostAsync("I've extended your achitecture to have multiple deployments in different regions, and a Traffice Manager to load balance across regions.");
            else
                await context.PostAsync("I'm glad you agreed :).");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Deny")]
        public async Task Deny(IDialogContext context, LuisResult result)
        {
            
            await context.PostAsync("Whatever...");
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetGuidance")]
        public async Task GetGuidance(IDialogContext context, LuisResult result)
        {
            var myLastQuestion = readState(context);
            
            if (myLastQuestion == "UseDatabase")
            {
                saveState(context, "GlobalApp");
                await context.PostAsync("Let's see... Is your web application a global application?");
            }
            else
            {
                await context.PostAsync("I don't have another suggestion at this time...");
            }
            context.Wait(MessageReceived);
        }

        private void saveState(IDialogContext context, string state)
        {
            var data = context.UserData;
            data.SetValue<string>("MyLastQuestion", state);
            //var stateClient = context.Activity.GetStateClient();
            //context.UserData.SetValue<string>("MyLastQuestion", state);
            //var userData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
            //userData.SetProperty<string>("MyLastQuestion", state);
            //await stateClient.BotState.SetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id, userData);
        }
        private string readState(IDialogContext context)
        {
            return context.UserData.GetValue<string>("MyLastQuestion");
        }

        

        [LuisIntent("StartOver")]
        public async Task StartOver(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Are you sure?");
            context.Wait(MessageReceived);
        }

        [LuisIntent("WrapUp")]
        public async Task WrapUp(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Awesome! I've created an ARM template that you can download. Or, you can click on the Deploy to Azure button to directly deploy it to Azure!");
            context.Wait(MessageReceived);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

    }
}