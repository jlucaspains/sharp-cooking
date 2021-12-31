using System;
using Android.Content;
using Android.Print;
using SharpCooking.Services;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AndroidApp = Android.App.Application;

namespace SharpCooking.Droid.Services
{
    public class AndroidPrintService : IPrintService
    {
        public void Print(string documentName, WebView viewToPrint)
        {
            if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.Kitkat)
                throw new NotSupportedException();

#pragma warning disable CA2000 // Dispose objects before losing scope
            var platformWebView = (WebViewRenderer)Platform.CreateRendererWithContext(viewToPrint, AndroidApp.Context).View;
            if (platformWebView.Control == null)
                return;
#pragma warning restore CA2000 // Dispose objects before losing scope

            var printMgr = (PrintManager)Forms.Context.GetSystemService(Context.PrintService);
            //var printMgr = (PrintManager)AndroidApp.Context.GetSystemService(Context.PrintService);

            printMgr.Print(documentName, platformWebView.Control.CreatePrintDocumentAdapter(documentName), null);
        }
    }
}