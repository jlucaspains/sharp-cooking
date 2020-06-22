using SharpCooking.Localization;
using SharpCooking.Models;
using System.Collections.Generic;
using System.ComponentModel;
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

        public virtual Task InitializeAsync()
        {
            return Task.FromResult(0);
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

        protected async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
        {
            return await Shell.DisplayAlert(title, message, accept, cancel);
        }

        protected async Task ReportError(string message)
        {
            await Shell.DisplayAlert(Resources.ErrorTitle, message, Resources.ErrorOk);
        }
    }
}
