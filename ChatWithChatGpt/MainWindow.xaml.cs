using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        string Input = "";
        string Conversation = "";
        private WaveInEvent waveSource;
        private WaveFileWriter waveFile;
        private MemoryStream audioStream;


        public MainWindow()
        {
            InitializeComponent();


        }




        private Task<string> GetGPTResponse(string inputText)
        {
            throw new NotImplementedException();
        }



        private void RecordButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            waveSource = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 1)
            };

            audioStream = new MemoryStream();
            waveFile = new WaveFileWriter(audioStream, waveSource.WaveFormat);

            waveSource.DataAvailable += WaveSource_DataAvailable;
            waveSource.RecordingStopped += WaveSource_RecordingStopped;

            waveSource.StartRecording();
        }

        private void RecordButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            waveSource.StopRecording();

        }

        private void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        private async void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (audioStream != null)
            {
                string transcript = await SpeechToTextClient.ConvertSpeechToText(audioStream);

                // Do something with the transcript, e.g., display it or save it to a file
                // ...

                audioStream.Dispose();
                audioStream = null;
            }



        }
    }
}
