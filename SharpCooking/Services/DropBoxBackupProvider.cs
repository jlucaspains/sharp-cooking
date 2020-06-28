using Dropbox.Api;
using Dropbox.Api.Files;
using SharpCooking.Localization;
using SharpCooking.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharpCooking.Services
{
    public class DropBoxBackupProvider : IBackupProvider
    {
        private const string ClientId = "kdedpnnfp1al1mw";

        private const string RedirectUri = "https://sharpcooking.net/dropboxauth";

        private readonly IEssentials _essentials;

        private string _authState;

        private string _accessToken;

        public bool IsAuthorized { get { return !string.IsNullOrEmpty(_accessToken); } }

        public string ProviderName { get { return Resources.DropBoxProviderName; } }

        public string ProviderLogo { get; } = "dropbox.png";

        public DropBoxBackupProvider(IEssentials essentials)
        {
            _essentials = essentials;
        }

        public async Task<bool> BackupFile(string localFilePath)
        {
            if (!IsAuthorized)
                return false;

            using (var client = new DropboxClient(_accessToken))
            {
                var fileToUpload = File.ReadAllBytes(localFilePath);
                var fileName = Path.GetFileName(localFilePath);

                using (var mem = new MemoryStream(fileToUpload))
                {
                    var updated = await client.Files.UploadAsync(
                        $"/{fileName}",
                        WriteMode.Overwrite.Instance,
                        body: mem);
                }
            }

            return true;
        }

        public async Task Authorize()
        {
            _accessToken = _accessToken ?? GetAccessTokenFromSettings();
            if (!string.IsNullOrEmpty(_accessToken) && await CheckToken(_accessToken))
            {
                return;
            }

            // Run Dropbox authentication
            _authState = Guid.NewGuid().ToString("N");
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, ClientId, new Uri(RedirectUri), _authState);
            var webView = new WebView { Source = new UrlWebViewSource { Url = authorizeUri.AbsoluteUri } };
            webView.Navigating += WebViewOnNavigating;
            var contentPage = new ContentPage { Content = webView };
            await Application.Current.MainPage.Navigation.PushModalAsync(contentPage);
        }

        private async Task<bool> CheckToken(string accessToken)
        {
            using (var client = new DropboxClient(accessToken))
            {
                try
                {
                    await client.Files.ListFolderAsync(new ListFolderArg("/"));
                    
                    return true;
                }
                catch (AuthException)
                {
                    SaveDropboxToken("");
                    return false;
                }
            }
        }

        private async void WebViewOnNavigating(object sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.
                return;
            }

            try
            {
                var result = DropboxOAuth2Helper.ParseTokenFragment(new Uri(e.Url));

                if (result.State != _authState)
                {
                    return;
                }

                _accessToken = result.AccessToken;

                SaveDropboxToken(_accessToken);
            }
            catch (ArgumentException)
            {
                // There was an error in the URI passed to ParseTokenFragment
            }
            finally
            {
                if (sender is WebView view)
                    view.Navigating -= WebViewOnNavigating;

                e.Cancel = true;
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }

        private string GetAccessTokenFromSettings()
        {
            return _essentials.GetStringSetting(AppConstants.DropBoxAccessToken);
        }

        private void SaveDropboxToken(string accessToken)
        {
            _essentials.SetStringSetting(AppConstants.DropBoxAccessToken, accessToken);
        }
    }
}
