using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SharpCooking.Data;
using SharpCooking.Services;
using SharpCooking.ViewModels;
using System.IO;
using TinyIoC;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace SharpCooking
{
    public partial class App : Xamarin.Forms.Application
    {

        public App()
        {
            InitializeComponent();

            RegisterContainer();

            Current.On<Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=aec1d7ce-6dbc-4207-aede-119fe2233238;" +
                  "ios=cc405e62-53be-4e4f-b9f6-0107416cbad5",
                  typeof(Analytics), typeof(Crashes));
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void RegisterContainer()
        {
            var container = TinyIoCContainer.Current;
            
            // View models - by default, TinyIoC will register concrete classes as multi-instance.
            container.Register<AboutViewModel>();
            container.Register<ItemsViewModel>();
            container.Register<ItemDetailViewModel>();
            container.Register<SettingsViewModel>();
            container.Register<EditItemViewModel>();

            // Services - by default, TinyIoC will register interface registrations as singletons.
            container.Register<IDataStore, DataStore>();
            container.Register<IEssentials, Essentials>();
            container.Register(GetConnectionFactory());
        }

        private IConnectionFactory GetConnectionFactory()
        {
            var dbName = "SharpCooking.db3";
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                dbName);
            var result = new ConnectionFactory(path);

            // call and forget.
            _ = result.MigrateDbToLatestAsync();
            
            return result;
        }

    }
}
