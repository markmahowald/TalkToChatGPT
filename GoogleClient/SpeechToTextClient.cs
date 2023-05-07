using Google.Cloud.Speech.V1;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Grpc.Core;
using Grpc.Net.Client;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;

namespace ChatWithChatGpt.GoogleClient
{
    public class SpeechToTextClient
    {
        public string ConvertSpeechToText(MemoryStream audioStream)
        {
            // Set your Google Cloud credentials JSON file path.
            string credentialsFilePath = Environment.GetEnvironmentVariable("GoogleTextToSpeechAndBackAgainServiceAccountCredientalPath");

            GoogleCredential googleCredential;
            using (Stream credentialStream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                googleCredential = GoogleCredential.FromStream(credentialStream);
            }
            
        ChannelCredentials channelCredentials = googleCredential.ToChannelCredentials();
            GrpcChannel channel = GrpcChannel.ForAddress("https://" + SpeechClient.DefaultEndpoint.ToString(), new GrpcChannelOptions { Credentials = channelCredentials });

            // Create a CallInvoker instance using the channel.
            CallInvoker callInvoker = channel.CreateCallInvoker();

            // Create a SpeechClient instance using the CallInvoker.
            SpeechClient speechClient = new SpeechClientBuilder { CallInvoker = callInvoker }.Build();

            RecognitionConfig config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 16000, // Set the sample rate according to your audio file
                LanguageCode = "en-US", // Set the language code as needed
            };

            RecognitionAudio recognitionAudio = new RecognitionAudio
            {
                Content = Google.Protobuf.ByteString.FromStream(audioStream),
            };

            RecognizeResponse response = speechClient.Recognize(config, recognitionAudio);
            StringBuilder resultBuilder = new StringBuilder();

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    resultBuilder.AppendLine(alternative.Transcript);
                }
            }

            return resultBuilder.ToString();
        }



    }
}
