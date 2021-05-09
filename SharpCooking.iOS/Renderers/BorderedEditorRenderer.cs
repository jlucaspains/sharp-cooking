using SharpCooking.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Editor), typeof(BorderedEditorRenderer))]
namespace SharpCooking.iOS.Renderers
{
    public class BorderedEditorRenderer : EditorRenderer
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