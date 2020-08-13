using Microsoft.AppCenter.Crashes;
using SharpCooking.Localization;
using SharpCooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public class BaseViewModel : BindableModel
    {
        public bool IsBusy { get; set; }

        public string Title { get; set; }

        public Shell Shell { get; set; } = Shell.Current;

        internal string ViewName { get; set; }

        public virtual Task InitializeAsync()
        {
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent("PageView", new Dictionary<string, string> {
                { "Name", ViewName }
            });

            return Task.CompletedTask;
        }

        public virtual Task TerminateAsync()
        {
            return Task.FromResult(0);
        }

        protected async Task GoToAsync(string route, Dictionary<string, object> parameters = null)
        {
            if (parameters != null)
            {
                var parsedParameters = parameters.Select(item => $"{item.Key}={item.Value}");
                route = $"{route}?{string.Join("&", parsedParameters)}";
            }

            await Shell.GoToAsync(route, true);
        }

        protected async Task GoBackAsync()
        {
            await Shell.Navigation.PopAsync(true);
        }

        protected async Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction = null, params string[] buttons)
        {
            return await Shell.DisplayActionSheet(title, cancel, destruction, buttons);
        }

        protected async Task<string> DisplayPromptAsync(string title, string message, string accept, string cancel, string placeholder = null, Keyboard keyboard = null)
        {
            return await Shell.DisplayPromptAsync(title, message, accept, cancel, placeholder, keyboard: keyboard);
        }

        protected async Task DisplayAlertAsync(string title, string message, string cancel)
        {
            await Shell.DisplayAlert(title, message, cancel);
        }

        protected async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
        {
            return await Shell.DisplayAlert(title, message, accept, cancel);
        }

        protected Task DisplayToastAsync(string message, int durationInSeconds = 3)
        {
            Acr.UserDialogs.UserDialogs.Instance.Toast(new Acr.UserDialogs.ToastConfig(message)
                .SetDuration(TimeSpan.FromSeconds(durationInSeconds))
                .SetPosition(Acr.UserDialogs.ToastPosition.Bottom));

            return Task.CompletedTask;
        }

        protected async Task<TimeSpan?> DisplayTimePromptAsync(string title, string accept, string cancel)
        {
            var result = await Acr.UserDialogs.UserDialogs.Instance.TimePromptAsync(new Acr.UserDialogs.TimePromptConfig
            {
                Title = title,
                OkText = accept,
                CancelText = cancel,
                IsCancellable = true
            });

            return result.Ok
                ? (TimeSpan?)result.SelectedTime
                : null;
        }

        protected IDisposable DisplayLoading(string title)
        {
            var config = new Acr.UserDialogs.ProgressDialogConfig().SetTitle(title).SetIsDeterministic(false).SetAutoShow(true);
            return Acr.UserDialogs.UserDialogs.Instance.Progress(config);
        }

        protected async Task ReportError(string message)
        {
            await Shell.DisplayAlert(Resources.ErrorTitle, message, Resources.ErrorOk);
        }

        protected Task TrackEvent(string name, Dictionary<string, string> properties = null)
        {
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name, properties);

            return Task.CompletedTask;
        }

        protected Task TrackEvent(string name, params (string Name, string Value)[] properties)
        {
            if(properties == null)
            {
                Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name);
            }
            else
            {
                var props = properties.ToDictionary(item => item.Name, item => item.Value);

                Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name, props);
            }

            return Task.CompletedTask;
        }

        protected Task TrackException(Exception ex)
        {
            Crashes.TrackError(ex);

            return Task.CompletedTask;
        }
    }
}
