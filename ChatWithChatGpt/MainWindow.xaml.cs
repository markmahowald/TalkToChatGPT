using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Google.Cloud.TextToSpeech.V1;
using Google.Protobuf;
using NAudio.Wave;



namespace ChatWithChatGpt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SpeechToTextClient _speechClient = new SpeechToTextClient();
        private readonly TextToSpeechClient _textToSpeechClient = new TextToSpeechClient();
        private readonly GptClient _gptClient = new GptClient();
        private MemoryStream _recordedAudioStream;

        string Input = "";
        string Conversation = "";

        private WaveInEvent _waveIn;
        private WaveFileWriter _waveFileWriter;



        public MainWindow()
        {
            InitializeComponent();


        }

        private void StartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_waveIn == null)
            {
                _waveIn = new WaveInEvent();
                _waveIn.DeviceNumber = 0; // Default microphone
                _waveIn.WaveFormat = new WaveFormat(16000, 1); // 44.1 kHz sample rate, 1 channel (mono)
                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.RecordingStopped += OnRecordingStopped;

                _recordedAudioStream = new MemoryStream();

                _waveIn.StartRecording();
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (_recordedAudioStream != null)
            {
                _recordedAudioStream.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (_waveIn != null)
            {
                _waveIn.Dispose();
                _waveIn = null;
            }

            if (_recordedAudioStream != null)
            {
                // Reset the position of the stream to the beginning before passing it to ConvertSpeechToText
                _recordedAudioStream.Position = 0;

                // Assuming _speechClient.ConvertSpeechToText() is an async method
                string result =  _speechClient.ConvertSpeechToText(_recordedAudioStream);

                // Do something with the result (e.g., display it in the ConversationTextBox)
                this.InputTranscriptionTextBox.Clear();
                this.InputTranscriptionTextBox.AppendText(result);

                // Dispose of the MemoryStream and set it to null
                _recordedAudioStream.Dispose();
                _recordedAudioStream = null;
            }
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
            }
        }

        private void SendToGptButton_Click(object sender, RoutedEventArgs e)
        {
            string conversationText = this.InputTranscriptionTextBox.Text;
            string gptResponse = this._gptClient.ContinueConversation(conversationText);
            //string gptResponse = this._gptClient.SendToGpt(conversationText);
            // Display the GPT response in the ConversationTextBox
            ConversationTextBox.Text += "You Said: "+ this.InputTranscriptionTextBox.Text;
            this.InputTranscriptionTextBox.Clear();
            ConversationTextBox.Text += "GPT: " + gptResponse + Environment.NewLine;
        }
    }


}
