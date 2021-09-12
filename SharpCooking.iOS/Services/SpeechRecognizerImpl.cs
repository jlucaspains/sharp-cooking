using System;
using System.Globalization;
using System.Threading.Tasks;
using SharpCooking.Services;
using Xamarin.Essentials;

namespace SharpCooking.iOS.Services
{
    class SpeechRecognizerImpl : ISpeechRecognizer
    {
        public Action ContinuousDictation(Action<bool, string> callback, CultureInfo culture = null)
        {
            return () => { };
        }

        public Action ListenUntilPause(Action<bool, string> callback, CultureInfo culture = null)
        {
            return () => { };
        }

        public Task<PermissionStatus> RequestAccess()
        {
            return Task.FromResult(PermissionStatus.Granted);
        }
    }
}