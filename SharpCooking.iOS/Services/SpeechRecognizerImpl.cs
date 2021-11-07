using System;
using System.Globalization;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using SharpCooking.Services;
using Speech;
using Xamarin.Essentials;

namespace SharpCooking.iOS.Services
{
    sealed class SpeechRecognizerImpl : ISpeechRecognizer, IDisposable
    {
        private AVAudioEngine _audioEngine = new AVAudioEngine();
        private SFSpeechRecognizer _speechRecognizer = new SFSpeechRecognizer();
        private SFSpeechAudioBufferRecognitionRequest _liveSpeechRequest = new SFSpeechAudioBufferRecognitionRequest();
        private SFSpeechRecognitionTask _recognitionTask;
        private bool _disposedValue;

        public Action ContinuousDictation(Action<bool, string> callback, CultureInfo culture = null)
        {
            // Setup audio session
            var node = _audioEngine.InputNode;
            var recordingFormat = node.GetBusOutputFormat(0);
            node.InstallTapOnBus(0, 1024, recordingFormat, (AVAudioPcmBuffer buffer, AVAudioTime when) =>
            {
                // Append buffer to recognition request
                _liveSpeechRequest.Append(buffer);
            });

            // Start recording
            _audioEngine.Prepare();
            _audioEngine.StartAndReturnError(out NSError error);

            // Did recording start?
            if (error != null)
            {
                callback(false, error.LocalizedDescription);
            }
            else
            {
                // Start recognition
                _recognitionTask = _speechRecognizer.GetRecognitionTask(_liveSpeechRequest, (SFSpeechRecognitionResult result, NSError err) =>
                {
                    // Was there an error?
                    if (err != null)
                    {
                        // Handle error
                    }
                    else
                    {
                        // Is this the final translation?
                        if (result.Final)
                        {
                            callback(true, result.BestTranscription.FormattedString);
                        }
                    }
                });
            }

            return () =>
            {
                _audioEngine.Stop();
                _liveSpeechRequest.EndAudio();
            };
        }

        public Action ListenUntilPause(Action<bool, string> callback, CultureInfo culture = null)
        {
            return () => { };
        }

        public Task<PermissionStatus> RequestAccess()
        {
            return Task.FromResult(PermissionStatus.Granted);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _audioEngine.Dispose();
                    _liveSpeechRequest.Dispose();
                    _recognitionTask.Dispose();
                    _speechRecognizer.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}