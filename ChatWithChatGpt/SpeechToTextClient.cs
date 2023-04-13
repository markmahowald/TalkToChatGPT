using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Google.Protobuf;
using NAudio.CoreAudioApi;
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
    internal class SpeechToTextClient
    {
        internal async Task<string> ConvertSpeechToText(MemoryStream audioStream)
        {
            // Set the path to your Google Cloud JSON credentials file
            // You can also set the "GOOGLE_APPLICATION_CREDENTIALS" environment variable to the path of the JSON file
            string credentialsFilePath = "path/to/your/google-cloud-credentials.json";
            GoogleCredential googleCredential;

            using (var stream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                googleCredential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SpeechClient.DefaultScopes);
            }

            var channel = new Grpc.Core.Channel(SpeechClient.DefaultEndpoint.ToString(), googleCredential.ToChannelCredentials());
            var speechClient = SpeechClient.Create(channel);

            // Rewind the audio stream to the beginning before sending it to the API
            audioStream.Seek(0, SeekOrigin.Begin);

            // Create a RecognitionConfig for the API
            var recognitionConfig = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 44100,
                LanguageCode = "en-US",
            };

            // Create a RecognitionAudio object from the MemoryStream
            var recognitionAudio = RecognitionAudio.FromStream(audioStream);

            // Perform the speech-to-text request
            var response = await speechClient.RecognizeAsync(recognitionConfig, recognitionAudio);

            // Process the results
            StringBuilder transcription = new StringBuilder();

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    transcription.AppendLine(alternative.Transcript);
                }
            }

            return transcription.ToString();
        }

        internal byte[] CaptureAudio()
        {
            int recordDurationMilliseconds = 5000; // Set the desired recording duration in milliseconds
            int sampleRate = 16000; // Set the sample rate to match the RecognitionConfig
            WaveFormat waveFormat = new WaveFormat(sampleRate, 16, 1); // 16-bit, 1 channel (mono)

            using (var waveIn = new WaveInEvent { WaveFormat = waveFormat })
            using (var memoryStream = new MemoryStream())
            using (var waveFileWriter = new WaveFileWriter(memoryStream, waveFormat))
            {
                AutoResetEvent stopRecordingEvent = new AutoResetEvent(false);

                waveIn.DataAvailable += (sender, e) =>
                {
                    waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                };

                waveIn.RecordingStopped += (sender, e) =>
                {
                    waveIn.Dispose();
                    waveFileWriter.Dispose();
                    stopRecordingEvent.Set();
                };

                waveIn.StartRecording();
                Thread.Sleep(recordDurationMilliseconds);
                waveIn.StopRecording();

                stopRecordingEvent.WaitOne();

                // Convert the WAV data to raw PCM data
                byte[] rawData = ConvertWavToPcm(memoryStream.ToArray());

                return rawData;
            }
        }
        private byte[] ConvertWavToPcm(byte[] wavData)
        {
            using (var inputStream = new MemoryStream(wavData))
            using (var wavReader = new WaveFileReader(inputStream))
            {
                int bytesPerSample = wavReader.WaveFormat.BitsPerSample / 8;
                int headerSize = (int)wavReader.Position;
                int dataSize = (int)(wavReader.Length - headerSize);
                int samples = dataSize / (wavReader.WaveFormat.Channels * bytesPerSample);

                byte[] pcmData = new byte[dataSize];

                for (int i = 0; i < samples; i++)
                {
                    for (int j = 0; j < wavReader.WaveFormat.Channels; j++)
                    {
                        for (int k = 0; k < bytesPerSample; k++)
                        {
                            int byteIndex = headerSize + (i * wavReader.WaveFormat.Channels * bytesPerSample) + (j * bytesPerSample) + k;
                            pcmData[(i * wavReader.WaveFormat.Channels * bytesPerSample) + (j * bytesPerSample) + k] = wavData[byteIndex];
                        }
                    }
                }

                return pcmData;
            }
        }

    }
}
