using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Plugin.CurrentActivity;
using Acr.UserDialogs;
using Android.Content;
using SharpCooking.Services;
using TinyIoC;
using SharpCooking.Droid.Services;

namespace SharpCooking.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait,
        LaunchMode =LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionSend },
        Categories = new[] { Intent.CategoryDefault },
        DataHost = "*",
        DataPathPattern = ".*\\.zip",
        DataMimeType = @"application/zip")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.SetFlags("IndicatorView_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            UserDialogs.Init(this);
            Xamarin.DateTimePopups.Platform.Init(this, savedInstanceState);
            XamEffects.Droid.Effects.Init();

            LoadApplication(new App());
            RegisterContainer();
            SaveImportFile();
            CreateNotificationFromIntent(Intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            CreateNotificationFromIntent(intent);
        }

        private void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(AndroidNotificationService.TitleKey);
                string message = intent.GetStringExtra(AndroidNotificationService.MessageKey);

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                    return;

                TinyIoCContainer.Current.Resolve<INotificationService>().ReceiveNotification(title, message);
            }
        }

        private void RegisterContainer()
        {
            var container = TinyIoCContainer.Current;

            container.Register<ISpeechRecognizer, SpeechRecognizerImpl>();
            container.Register<INotificationService, AndroidNotificationService>();
        }

        private void SaveImportFile()
        {
            if (Intent.Action != Intent.ActionSend)
                return;

            var docsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var filePath = System.IO.Path.Combine(docsPath, "import.zip");

            var zip = Intent.ClipData.GetItemAt(0);
            var zipStream = ContentResolver.OpenInputStream(zip.Uri);

            using var memStream = new System.IO.MemoryStream();
            zipStream.CopyTo(memStream);

            System.IO.File.WriteAllBytes(filePath, memStream.ToArray());

            Xamarin.Forms.Shell.Current.GoToAsync("import");
        }
    }
}