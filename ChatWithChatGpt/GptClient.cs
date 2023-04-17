
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

namespace ChatWithChatGpt
{
    internal class GptClient
    {
        internal string SendToGpt4(string inputText)
        {
            string apiKey = Environment.GetEnvironmentVariable("OpenAiApiKey");
            string apiUrl = "https://api.openai.com/v1/engines/text-davinci-003/completions";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                JObject requestData = new JObject
        {
            { "prompt", inputText },
            { "max_tokens", 50 }, // Adjust as needed
            { "n", 1 },
            { "stop", null },
            { "temperature", 0.5 } // Adjust as needed
        };

                StringContent content = new StringContent(requestData.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                int retries = 0;
                int maxRetries = 5;
                int delayInSeconds = 2;

                while (true)
                {
                    response = httpClient.PostAsync(apiUrl, content).Result; // Synchronous call
                    if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.TooManyRequests && retries < maxRetries)
                    {
                        // Exponential backoff
                        int delay = delayInSeconds * (int)Math.Pow(2, retries);
                        Task.Delay(TimeSpan.FromSeconds(delay)).Wait();
                        retries++;
                    }
                    else
                    {
                        break;
                    }
                }

                string responseJson = response.Content.ReadAsStringAsync().Result; // Synchronous call

                if (response.IsSuccessStatusCode)
                {
                    JObject responseObject = JObject.Parse(responseJson);
                    JArray choices = (JArray)responseObject["choices"];
                    string gptReply = (string)choices[0]["text"];
                    return gptReply.Trim();
                }
                else
                {
                    throw new Exception("Error: " + response.StatusCode);
                }
            }
        }


    }


}
