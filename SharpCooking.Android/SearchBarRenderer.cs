using Android.Text;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using G = Android.Graphics;

[assembly: ExportRenderer(typeof(SearchBar), typeof(SharpCooking.Droid.MySearchBarRenderer))]

namespace SharpCooking.Droid
{

    public class MySearchBarRenderer : SearchBarRenderer
    {
        public MySearchBarRenderer(Android.Content.Context context) : base(context)
        { }

        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> args)
        {
            base.OnElementChanged(args);

            if (Application.Current.RequestedTheme != OSAppTheme.Dark)
                return;

            // Get native control (background set in shared code, but can use SetBackgroundColor here)
            SearchView searchView = (base.Control as SearchView);
            //searchView.SetInputType(InputTypes.ClassText | InputTypes.TextVariationNormal);

            // Access search textview within control
            int textViewId = searchView.Context.Resources.GetIdentifier("android:id/search_src_text", null, null);
            EditText textView = (searchView.FindViewById(textViewId) as EditText);

            // Set custom colors
            textView.SetTextColor(G.Color.Rgb(255, 255, 255));
            textView.SetHintTextColor(G.Color.Rgb(128, 128, 128));

            int searchIconId = searchView.Context.Resources.GetIdentifier("android:id/search_mag_icon", null, null);
            var searchIconView = (searchView.FindViewById(searchIconId) as ImageView);
            searchIconView.SetColorFilter(G.Color.Rgb(96, 96, 96));
        }
    }
}