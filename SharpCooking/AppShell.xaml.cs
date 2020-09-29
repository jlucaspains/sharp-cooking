﻿using SharpCooking.Views;
using Xamarin.Forms;

namespace SharpCooking
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute("items", typeof(Views.ItemsView));
            Routing.RegisterRoute("items/detail", typeof(ItemDetailView));
            Routing.RegisterRoute("items/edit", typeof(EditItemView));
            Routing.RegisterRoute("items/new", typeof(EditItemView));
            Routing.RegisterRoute("about", typeof(AboutView));
            Routing.RegisterRoute("settings", typeof(SettingsView));
            Routing.RegisterRoute("credits", typeof(CreditsView));
        }
    }
}
