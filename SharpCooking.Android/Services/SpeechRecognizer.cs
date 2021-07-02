using System;
using System.Globalization;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Speech;
using Plugin.CurrentActivity;
using SharpCooking.Services;
using Xamarin.Essentials;

namespace SharpCooking.Droid.Services
{
    public class SpeechRecognizerImpl : ISpeechRecognizer
    {
        readonly Context context;
        readonly object syncLock = new object();

        public SpeechRecognizerImpl() => this.context = CrossCurrentActivity.Current.AppContext;

        public async Task<PermissionStatus> RequestAccess()
        {
            if (!SpeechRecognizer.IsRecognitionAvailable(context))
                return PermissionStatus.Disabled;

            //if (!context.IsInManifest(Manifest.Permission.RecordAudio))
            //    return AccessState.NotSetup;

            var result = await Permissions.CheckStatusAsync<Permissions.Speech>();

            if (result != Xamarin.Essentials.PermissionStatus.Granted)
                result = await Permissions.RequestAsync<Permissions.Speech>();

            return result;
        }

        public Action ListenUntilPause(Action<bool, string> callback, CultureInfo culture)
        {
            var final = "";
            var listener = new SpeechRecognitionListener
            {
                //ReadyForSpeech = () => this.ListenSubject.OnNext(true),
                Error = ex => callback(false, $"Failure in speech engine - {ex.ToString()}"),
                PartialResults = sentence =>
                {
                    lock (syncLock)
                        final = sentence;
                },
                FinalResults = sentence =>
                {
                    lock (syncLock)
                        final = sentence;
                },
                EndOfSpeech = () =>
                {
                    lock (syncLock)
                    {
                        callback(true, final);
                    }
                }
            };
            var speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(context);
            speechRecognizer.SetRecognitionListener(listener);
            speechRecognizer.StartListening(CreateSpeechIntent(true, culture));

            return () =>
            {
                listener.Dispose();
                speechRecognizer.StopListening();
                speechRecognizer.Destroy();
                speechRecognizer.Dispose();
            };
        }

        public Action ContinuousDictation(Action<bool, string> callback, CultureInfo culture = null)
        {
            var stop = false;
            var currentIndex = 0;
            var speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Application.Context);
            var listener = new SpeechRecognitionListener();

            //listener.ReadyForSpeech = () => this.ListenSubject.OnNext(true);
            listener.PartialResults = sentence =>
            {
                lock (syncLock)
                {
                    sentence = sentence.Trim();
                    if (currentIndex > sentence.Length)
                        currentIndex = 0;

                    var newPart = sentence.Substring(currentIndex);
                    currentIndex = sentence.Length;
                    callback(true, newPart);
                }
            };

            listener.EndOfSpeech = () =>
            {
                lock (syncLock)
                {
                    currentIndex = 0;
                    speechRecognizer.Destroy();

                    speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(context);
                    speechRecognizer.SetRecognitionListener(listener);
                    speechRecognizer.StartListening(CreateSpeechIntent(true, culture));
                }
            };
            listener.Error = ex =>
            {
                switch (ex)
                {
                    case SpeechRecognizerError.Client:
                    case SpeechRecognizerError.RecognizerBusy:
                    case SpeechRecognizerError.SpeechTimeout:
                        lock (syncLock)
                        {
                            if (stop)
                                return;

                            speechRecognizer.Destroy();

                            speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(context);
                            speechRecognizer.SetRecognitionListener(listener);
                            speechRecognizer.StartListening(CreateSpeechIntent(true, culture));
                        }
                        break;

                    default:
                        callback(false, $"Could not start speech recognizer - ERROR: {ex}");
                        break;
                }
            };
            speechRecognizer.SetRecognitionListener(listener);
            speechRecognizer.StartListening(CreateSpeechIntent(true, culture));


            return () =>
            {
                stop = true;
                listener.Dispose();
                speechRecognizer?.StopListening();
                speechRecognizer?.Destroy();
                speechRecognizer?.Dispose();
            };
        }


        protected virtual Intent CreateSpeechIntent(bool partialResults, CultureInfo culture)
        {
            var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, Java.Util.Locale.Default);

            if (culture == null)
            {
                intent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            }
            else
            {
                var javaLocale = Java.Util.Locale.ForLanguageTag(culture.Name);
                intent.PutExtra(RecognizerIntent.ExtraLanguage, javaLocale);
            }
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            intent.PutExtra(RecognizerIntent.ExtraCallingPackage, "com.lpains.sharpcooking");
            //intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            //intent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
            //intent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
            //intent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
            intent.PutExtra(RecognizerIntent.ExtraPartialResults, partialResults);

            return intent;
        }
    }
}