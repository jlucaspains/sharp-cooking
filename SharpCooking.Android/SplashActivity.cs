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
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
#pragma warning disable CA2000 // Dispose objects before losing scope
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed() { }
    }
}