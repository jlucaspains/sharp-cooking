using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SharpCooking.Data;
using SharpCooking.Services;
using SharpCooking.ViewModels;
using System;
using System.IO;
using TinyIoC;
using Xamarin.Essentials;
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

            VersionTracking.Track();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=73def550-8bd6-46cc-8cde-7af4dda4b48e;" +
                  "ios=5cce9d18-398e-44a8-b793-d6b34fd06d24",
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
            container.Register<CreditsViewModel>();
            container.Register<ImportViewModel>();
            container.Register<SortItemsViewModel>();
            container.Register<CookModeViewModel>();
            container.Register<PreviewFeaturesViewModel>();

            // Services - by default, TinyIoC will register interface registrations as singletons.
            container.Register<IDataStore, DataStore>();
            container.Register<IEssentials, Essentials>();
            container.Register<IFileHelper, FileHelper>();
            container.Register<IHttpService, HttpService>();
            container.Register<IRecipePackager, RecipePackager>();
            container.Register(GetConnectionFactory());
        }

        private IConnectionFactory GetConnectionFactory()
        {
            var dbName = "SharpCooking.db3";
            string folder;

            if (DeviceInfo.Platform == DevicePlatform.iOS)
                folder = Environment.GetFolderPath(Environment.SpecialFolder.Resources);
            else
                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var path = Path.Combine(folder, dbName);

            var result = new ConnectionFactory(path);

            // call and forget.
            _ = result.MigrateDbToLatestAsync();

            return result;
        }
    }
}
