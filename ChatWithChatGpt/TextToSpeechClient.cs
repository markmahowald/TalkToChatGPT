using Google.Cloud.TextToSpeech.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWithChatGpt
{
    internal class TextToSpeechClient
    {
        public async Task<MemoryStream> ConvertTextToSpeech(string text)
        {
            var clientBuilder = new TextToSpeechClientBuilder();
            var client = await clientBuilder.BuildAsync();

            var input = new SynthesisInput
            {
                Text = text
            };

            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = "en-US",
                Name = "en-US-Wavenet-A"
            };

            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Linear16,
                SampleRateHertz = 16000
            };

            var response = await client.SynthesizeSpeechAsync(input, voiceSelection, audioConfig);

            var audioStream = new MemoryStream(response.AudioContent.ToByteArray());

            return audioStream;
        }
    }

}
