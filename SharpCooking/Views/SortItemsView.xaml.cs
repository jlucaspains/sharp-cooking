using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SharpCooking.Models;
using SharpCooking.ViewModels;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace SharpCooking.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class SortItemsView : ContentPage
    {
        public SortItemsView()
        {
            InitializeComponent();

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
        }
    }
}