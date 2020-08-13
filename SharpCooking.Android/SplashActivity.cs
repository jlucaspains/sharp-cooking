using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.Forms.Platform.Android;

namespace SharpCooking.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/SplashTheme", Icon = "@mipmap/icon", 
        MainLauncher = true, NoHistory = true)]
    public class SplashActivity : FormsAppCompatActivity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed() { }
    }
}