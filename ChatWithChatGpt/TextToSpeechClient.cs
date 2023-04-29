using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatWithChatGpt
{
    internal class TextToSpeechClient
    {
        public async Task<MemoryStream> ConvertTextToSpeech(string text)
        {
            string credentialsFilePath = Environment.GetEnvironmentVariable("GoogleTextToSpeechAndBackAgainServiceAccountCredientalPath");

            var clientBuilder = new TextToSpeechClientBuilder();
            clientBuilder.CredentialsPath= credentialsFilePath;
            var client = await clientBuilder.BuildAsync();

            var input = new SynthesisInput
            {
                Text = text
            };

            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = "en-US",
                Name = "en-US-Standard-I"
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
        public async Task PlayAudioStream(Stream audioStream)
        {
            // Ensure the stream position is at the beginning
            audioStream.Position = 0;

            // Create a WaveStream from the input stream
            using var waveStream = new WaveFileReader(audioStream);

            // Create a WaveOutEvent to play the audio
            using var waveOut = new WaveOutEvent();

            // Set the waveOut's input stream to the waveStream
            waveOut.Init(waveStream);

            // Start playing the audio
            waveOut.Play();

            // Use a TaskCompletionSource to await the playback end
            var tcs = new TaskCompletionSource<bool>();
            waveOut.PlaybackStopped += (s, e) => tcs.SetResult(true);

            // Wait until playback is complete
            await tcs.Task;
        }
    }


}
