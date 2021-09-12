using System.IO;
using Foundation;
using SharpCooking.iOS.Services;
using SharpCooking.Services;
using TinyIoC;
using UIKit;

namespace SharpCooking.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            global::Xamarin.Forms.Forms.SetFlags("IndicatorView_Experimental");
            global::Xamarin.Forms.Forms.Init();
            XamEffects.iOS.Effects.Init();

            LoadApplication(new App());
            RegisterContainer();

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (url == null) return false;

            var docsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var filePath = System.IO.Path.Combine(docsPath, "import.zip");

            File.Copy(url.Path, filePath, true);

            Xamarin.Forms.Shell.Current.GoToAsync("import");

            return true;
        }

        private void RegisterContainer()
        {
            var container = TinyIoCContainer.Current;

            container.Register<ISpeechRecognizer, SpeechRecognizerImpl>();
        }

    }
}
