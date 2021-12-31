using Xamarin.Forms;

namespace SharpCooking.Services
{
    public interface IPrintService
    {
        void Print(string documentName, WebView viewToPrint);
    }
}
