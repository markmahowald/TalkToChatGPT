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
using Microsoft.Win32;
using System.Windows.Forms;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using MessageBox = System.Windows.Forms.MessageBox;
using ChatWithChatGpt.GoogleClient;
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
        private WaveInEvent _waveIn;
        private WaveFileWriter _waveFileWriter;

        //todo: encase things in try/catch/log blocks
        //todo: implement logging
        //todo: implement saving a conversation for later. 


        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.StartNewConversation();
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

        private async void SendToGptButton_Click(object sender, RoutedEventArgs e)
        {
            string conversationText = this.InputTranscriptionTextBox.Text;
            string gptResponse = this._gptClient.ContinueConversation(conversationText);
            //string gptResponse = this._gptClient.SendToGpt(conversationText);
            // Display the GPT response in the ConversationTextBox
            ConversationTextBox.Text += "You Said: "+ this.InputTranscriptionTextBox.Text+"\n";
            this.InputTranscriptionTextBox.Clear();
            ConversationTextBox.Text += "GPT Said: " + gptResponse + Environment.NewLine;
            this._textToSpeechClient.PlayAudioStream( await this._textToSpeechClient.ConvertTextToSpeech(gptResponse));
        }

        private void NewConversationButton_Click(object sender, RoutedEventArgs e)
        {
            this.StartNewConversation();
        }

        private async void StartNewConversation()
        {
            var newChatDialog = new NewConversationMessageBox();
            newChatDialog.ShowDialog();
           
            var role = newChatDialog.InputTextBox.Text;
            this.ConversationTextBox.Clear();
            this.InputTranscriptionTextBox.Clear();
            this.InputTranscriptionTextBox.Text = "Type your transcription here...";
            var response = this._gptClient.StartNewConversation(role);
            ConversationTextBox.Text += "GPT's role: "+role+"\nGPT: " + response + Environment.NewLine;
            this._textToSpeechClient.PlayAudioStream(await this._textToSpeechClient.ConvertTextToSpeech(response));

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.StartNewConversation();
        }

        private void SaveConversationButton_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                this._gptClient.SaveConversation( saveFileDialog.FileName);
            }
        }
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var oldConvo = this._gptClient.LoadConversation( openFileDialog.FileName );
                if (oldConvo is not null)
                {
                    this.ConversationTextBox.Clear();
                    this.InputTranscriptionTextBox.Clear();
                    string history = "";
                    foreach (var message in oldConvo)
                    {
                        string speaker = (message.Item1 == "Assistant" ? "GPT Said" : "You Said: ");
                        history += (speaker + message.Item2+"\n");
                    }
                    this.ConversationTextBox.Text = history; 
                }
                else
                {
                    MessageBox.Show("Error loading conversation");
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }


}
