
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Completions;
using OpenAI_API;
using Newtonsoft.Json.Linq;
using System.Net;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API.Completions;
using System.Threading;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using System.IO;
using OpenAI.Files;
using System.Data;
using System.Windows.Forms.VisualStyles;

namespace ChatWithChatGpt
{
    internal class GptClient
    {
        private readonly OpenAIAPI _api;
        private Conversation _chat;
        private string _apiKey;
        public GptClient()
        {
            //todo: give users a way to login if there is no such environment variable. 
            this._apiKey = Environment.GetEnvironmentVariable("OpenAiApiKey");
            this._api= new OpenAIAPI(_apiKey);
        }

        public string StartConversation(string role, string initialPrompt)
        {
            this._chat = this._api.Chat.CreateConversation();
            _chat.AppendSystemMessage(role);
            _chat.AppendUserInput(initialPrompt);
            return GetGPTResponse(_chat);

        }
        public string StartNewConversation(string role)
        {
            this._chat = this._api.Chat.CreateConversation();
            _chat.Model = Model.ChatGPTTurbo;
            _chat.AppendSystemMessage("In this conversation, I need you to be "+role);
            return GetGPTResponse(_chat);

        }

        private string GetGPTResponse(Conversation _chat)
        {
            Task<string> gptResponse = Task.Run(() => _chat.GetResponseFromChatbotAsync());
            gptResponse.Wait();
            return gptResponse.Result;
        }

        public  string ContinueConversation(string userInput)
        {
            if (_chat is null)
            {
                this._chat = this._api.Chat.CreateConversation();
            }
            _chat.AppendUserInput(userInput);
            return GetGPTResponse(_chat);
        }
        
        public void SaveConversation( string filePath)
        {
            string jsonString = "";
            List<Message> messages = this._chat.Messages.Select(x => new Message() {User = x.Role, Text = x.Content }).ToList();

            jsonString = JsonConvert.SerializeObject(new ConversationRecord() { Messages = messages }, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
          
            //var json = JsonConvert.SerializeObject(this._chat, Formatting.Indented);
            //File.WriteAllText(filePath, json);
        }
        public List<Tuple<string, string>> LoadConversation(string fileName)
        {
            if (!File.Exists(fileName)) return null;
            var json = File.ReadAllText(fileName);

            //var convo = JsonConvert.DeserializeObject<Conversation>(json);
            var convo = JsonConvert.DeserializeObject<ConversationRecord>(json);
            this._chat = this._api.Chat.CreateConversation();
            foreach (Message m in convo.Messages)
            {
                _chat.AppendMessage(new ChatMessage() { Role = (m.User == "assistant" ? ChatMessageRole.Assistant : ChatMessageRole.User), Content = m.Text });
            }

            var result = new List<Tuple<string, string>>();
            foreach (var message in convo.Messages)
            {
                Tuple<string, string> roleAndMessage = new Tuple<string, string>(message.User, message.Text);
                result.Add(roleAndMessage);
            }
            return result;

        }
    }


}
