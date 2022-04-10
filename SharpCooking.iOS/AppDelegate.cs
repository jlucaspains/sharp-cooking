using System.IO;
using Foundation;
using SharpCooking.iOS.Services;
using SharpCooking.Services;
using TinyIoC;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

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
            Xamarin.Forms.Forms.Init();
            XamEffects.iOS.Effects.Init();

            UNUserNotificationCenter.Current.Delegate = new iOSNotificationReceiver();

            LoadApplication(new App());
            RegisterContainer();

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (url == null) return false;

            var docsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(docsPath, "import.zip");

            File.Copy(url.Path, filePath, true);

            Shell.Current.GoToAsync("import");

            return true;
        }

        public override void DidEnterBackground(UIApplication uiApplication)
        {
            MessagingCenter.Send(Shell.Current, "Backgrounded");

            base.DidEnterBackground(uiApplication);
        }

        public override void WillEnterForeground(UIApplication uiApplication)
        {
            MessagingCenter.Send(Shell.Current, "Foregrounded");

            base.WillEnterForeground(uiApplication);
        }

        private void RegisterContainer()
        {
            var container = TinyIoCContainer.Current;

            container.Register<ISpeechRecognizer, SpeechRecognizerImpl>();
            container.Register<INotificationService, iOSNotificationService>();
            container.Register<IPrintService, iOSPrintService>();
        }
    }
}
