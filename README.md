# C# WPF Voice Assistant Application

This is a C# WPF application that leverages Google Cloud Speech-to-Text API and OpenAI GPT to create a voice assistant. Users can record their voice, convert the speech to text, and send the text to GPT for generating relevant responses.

## Features

- Record audio using the default microphone
- Convert the recorded audio to text using Google Cloud Speech-to-Text API
- Interact with OpenAI GPT to generate relevant responses
- Display the conversation history in a WPF application

## Prerequisites

- .NET Framework (version X.X or later)
- Google Cloud SDK
- OpenAI API access

## Installation

1. Clone the repository:
git clone https://github.com/yourusername/yourrepository.git

2. Open the solution file in Visual Studio.

3. Install the required NuGet packages:

- Google.Cloud.Speech.V1
- Google.Apis.Auth
- Grpc.Net.Client
- Newtonsoft.Json

4. Set up the environment variables:

- `GoogleTextToSpeechAndBackAgainServiceAccountCredientalPath`: The file path to your Google Cloud credentials JSON file.
- `OPENAI_API_KEY`: Your OpenAI API key.

5. Build and run the application in Visual Studio.

## Usage

1. Click the "Start Recording" button to start recording audio.
2. Click the "Stop Recording" button to stop recording.
3. The recorded audio will be converted to text and displayed in the `ConversationTextBox`.
4. Click the "Send to GPT" button to send the text to the OpenAI GPT.
5. GPT-generated response will be displayed in the `ConversationTextBox`.

## License

This project is licensed under the [MIT License](LICENSE).


