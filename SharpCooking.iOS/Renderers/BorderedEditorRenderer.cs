using SharpCooking.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Editor), typeof(BorderedEditorRenderer))]
namespace SharpCooking.iOS.Renderers
{
#pragma warning disable CA1010 // Generic interface should also be implemented
    public class BorderedEditorRenderer : EditorRenderer
#pragma warning restore CA1010 // Generic interface should also be implemented
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Layer.CornerRadius = 5;
                Control.Layer.BorderColor = UIColor.Clear.CGColor;
                Control.Layer.BorderWidth = 1;
            }
        }
    }
}