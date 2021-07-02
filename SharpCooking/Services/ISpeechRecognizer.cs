using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SharpCooking.Services
{
    public interface ISpeechRecognizer
    {
        /// <summary>
        /// Requests/ensures appropriate platform permissions where necessary
        /// </summary>
        /// <returns></returns>
        Task<PermissionStatus> RequestAccess();


        /// <summary>
        /// Optimal command for listening to a sentence.  Completes when user pauses
        /// </summary>
        /// <returns>Action to dispose of inner handlers</returns>
        Action ListenUntilPause(Action<bool, string> callback, CultureInfo culture = null);


        /// <summary>
        /// Continuous dictation.  Returns text as made available.  Dispose to stop dictation.
        /// </summary>
        /// <returns>Action to dispose of inner handlers</returns>
        Action ContinuousDictation(Action<bool, string> callback, CultureInfo culture = null);
    }
}
