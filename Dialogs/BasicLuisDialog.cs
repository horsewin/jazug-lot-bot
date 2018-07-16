using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Resource;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Sample.LuisBot
{
    [JsonObject("outputSpeech")]
    public class OutputSpeech
    {
        public string type { get; set; }
        public string ssml { get; set; }
    }
    
    [JsonObject("reprompt")]
    public class Reprompt
    {
        public OutputSpeech outputSpeech { get; set; }
    }

    [JsonObject("response")]    
    public class Response
    {
        public bool shouldEndSession { get; set; }
        public OutputSpeech outputSpeech { get; set; }
        public Reprompt reprompt { get; set; }
    }

    [JsonObject("sessionAttribute")]
    public class SessionAttributes
    {
    }

    [JsonObject("alexa")]
    public class AlexaResponse
    {
        public string version { get; set; }
        public Response response { get; set; }
        public SessionAttributes sessionAttributes { get; set; }
        public string userAgent { get; set; }
    }


// For more information about this template visit http://aka.ms/azurebots-csharp-luis
[Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        static HttpClient httpClient = new HttpClient();
        public string alexaID { get; set; }

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            this.alexaID = ConfigurationManager.AppSettings["AlexaSKillID"];
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("TypeIntent")]
        public async Task TypeIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisBye(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        //private async Task ConnectAlexaSkill(IDialogContext context)
        //{
        //}

        private async Task<string> sendHttpRequest(string url, string json)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();

        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            //await context.PostAsync($"{result.Intents[0].Intent} : {result.Query}");

            string intentName;
            string intentType;
            string responseText;
            switch (result.Intents[0].Intent)
            {
                case "TypeIntent":
                    intentType = "LaunchRequest";
                    intentName = "";
                    responseText = "Welcome to Japan Azure User Group Lightning talk!";
                    break;
                case "Help":
                    intentType = "IntentRequest";
                    intentName = "AMAZON.HelpIntent";
                    responseText = "This response is for help the user.";
                    break;
                case "None":
                    intentType = "IntentRequest";
                    intentName = "Unhandled";
                    responseText = "This skill unhandled your input...";
                    break;
                case "Cancel":
                    intentType = "IntentRequest";
                    intentName = "AMAZON.CancelIntent";
                    responseText = "Bye-bye";
                    break;
                default:
                    intentType = "";
                    intentName = "";
                    responseText = "This skill unhandled your input...";
                    break;
            }
            var response = context.MakeMessage();

            // Request to Alexa skill
            string url = ConfigurationManager.AppSettings["AlexaEndpoint"];
            
            JObject body = new JObject();
            body["version"] = "1.0";
            body["session"] = new JObject();
            body["session"]["new"] = true;
            body["session"]["sessionId"] = "amzn1.echo-api.session.75fdb45d-5f8c-411e-9bbb-31405953cbb4";
            body["session"]["application"] = new JObject();
            body["session"]["application"]["applicationId"] = this.alexaID;
            body["session"]["user"] = new JObject();
            body["session"]["user"]["userId"] = "amzn1.ask.account.AGGG3WUBQDS5F4STDSUX7QXLWYO5OOBTBBSK56FUL7PAXFRAEV2AXM3I5ETVDDQSVTHSLLMS54YH3D7ASI4IEPJKFH3VKD5J3C5Z4QPCGNK73YHISINSPJG354Q6CTRA3K5OM62JHF2372HLAIGP4EPZYIE3UUV75BRF5PGZ35KZSA5ROYEIW3WW5Y3XXE6RIYUPTD2B3KL43ZY";
            body["context"] = new JObject();
            body["context"]["System"] = new JObject();
            body["context"]["System"]["application"] = new JObject();
            body["context"]["System"]["application"]["applicationId"] = this.alexaID;
            body["context"]["System"]["user"] = new JObject();
            body["context"]["System"]["user"]["userId"] = "amzn1.ask.account.AGGG3WUBQDS5F4STDSUX7QXLWYO5OOBTBBSK56FUL7PAXFRAEV2AXM3I5ETVDDQSVTHSLLMS54YH3D7ASI4IEPJKFH3VKD5J3C5Z4QPCGNK73YHISINSPJG354Q6CTRA3K5OM62JHF2372HLAIGP4EPZYIE3UUV75BRF5PGZ35KZSA5ROYEIW3WW5Y3XXE6RIYUPTD2B3KL43ZY";
            body["request"] = new JObject();
            body["request"]["type"] = intentType;
            body["request"]["requestId"] = "amzn1.echo-api.request.3943ddad-1ba5-4202-9a42-9413c65a95b1";
            body["request"]["timestamp"] = "2018-06-24T13:25:19Z";
            body["request"]["locale"] = "ja-JP";
            body["request"]["intent"] = new JObject();
            body["request"]["intent"]["name"] = intentName;
            body["request"]["intent"]["slot"] = "";

            await sendHttpRequest(url, body.ToString()).ContinueWith((task) =>
            {
                string res = task.Result;
                var json = JsonConvert.DeserializeObject<AlexaResponse>(res);
                var ssml = json.response.outputSpeech.ssml;

                response.Speak = ssml;
                response.Text = responseText;
                response.InputHint = InputHints.ExpectingInput;
            });

            await context.PostAsync(response);
            context.Wait(MessageReceived);
        }

        private async Task ShowLuisBye(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"{result.Intents[0].Intent} : {result.Query}");
            var response = context.MakeMessage();
            var text = "Thank you for your attendance of this lightning talk! Bye!!";
            response.Text = text;

            var ssml = SSMLHelper.Speak(text, false, null);
            response.Speak = ssml;

            response.InputHint = InputHints.AcceptingInput;
            await context.PostAsync(response);

            context.Done<object>(null);
        }
    }
}