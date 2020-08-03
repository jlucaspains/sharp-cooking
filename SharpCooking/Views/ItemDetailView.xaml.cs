using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SharpCooking.Models;
using SharpCooking.ViewModels;

namespace SharpCooking.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemDetailView : ContentPage
    {
        public ItemDetailView()
        {
            InitializeComponent();

            //var item = new Item
            //{
            //    Text = "Item 1",
            //    Description = "This is an item description."
            //};

            //viewModel = new ItemDetailViewModel(item);
            //BindingContext = viewModel;
        }
    }
}