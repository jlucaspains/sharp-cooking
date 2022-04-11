using SharpCooking.Services;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace SharpCooking.iOS.Services
{
    public class iOSPrintService : IPrintService
    {
        public void Print(string documentName, WebView viewToPrint)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var appleViewToPrint = Platform.CreateRenderer(viewToPrint).NativeView;
#pragma warning restore CA2000 // Dispose objects before losing scope

            var printInfo = UIPrintInfo.PrintInfo;

            printInfo.OutputType = UIPrintInfoOutputType.General;
            printInfo.JobName = documentName;
            printInfo.Orientation = UIPrintInfoOrientation.Portrait;
            printInfo.Duplex = UIPrintInfoDuplex.None;

            var printController = UIPrintInteractionController.SharedPrintController;

            printController.PrintInfo = printInfo;
            printController.ShowsPageRange = true;
            printController.PrintFormatter = appleViewToPrint.ViewPrintFormatter;

            printController.Present(true, (printInteractionController, completed, error) => { });
        }
    }
}