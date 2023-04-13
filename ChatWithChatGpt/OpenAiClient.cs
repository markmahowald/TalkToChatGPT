
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

namespace ChatWithChatGpt
{
    internal class OpenAiClient
    {
        private async Task<string> GetGPTResponse(string inputText)
        {
            string apiKey = Environment.GetEnvironmentVariable("OpenAiApiKey");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var requestBody = new
                {
                    model = "text-davinci-002",
                    prompt = inputText,
                    temperature = 0.7,
                    max_tokens = 150,
                    top_p = 1,
                    frequency_penalty = 0,
                    presence_penalty = 0,
                    stop = new[] { "\n" }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync("https://api.openai.com/v1/engines/davinci-codex/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                    string gptResponse = jsonDocument.RootElement.GetProperty("choices")[0].GetProperty("text").GetString().Trim();

                    return gptResponse;
                }
                else
                {
                    // Handle the error (you may want to throw an exception or return an error message)
                    return string.Empty;
                }
            }
        }
    }
}
