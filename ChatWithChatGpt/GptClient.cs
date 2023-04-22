
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

namespace ChatWithChatGpt
{
    internal class GptClient
    {
        private readonly OpenAIAPI _api;
        private Conversation _chat;
        public GptClient()
        {
            this._api= new OpenAIAPI(Environment.GetEnvironmentVariable("OpenAiApiKey"));
        }

        public string StartConversation(string role, string initialPrompt)
        {
            Conversation _chat = this._api.Chat.CreateConversation();
            _chat.AppendSystemMessage(role);
            _chat.AppendUserInput(initialPrompt);
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
            _chat.AppendUserInput(userInput);
            return GetGPTResponse(_chat);
        }
    }


}
