using AiForms.Renderers;
using SharpCooking.Data;
using SharpCooking.Models;
using SharpCooking.Services;
using SharpCooking.ViewModels;
using System.IO;
using TinyIoC;
using Xamarin.Forms;

namespace SharpCooking
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            RegisterContainer();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
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
            container.Register<SettingsView>();
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

            var connection = result.GetConnection();
            connection.CreateTableAsync<Recipe>();
            //connection.CreateTableAsync<RecipePictures>();
            connection.CreateTableAsync<Uom>();

            return result;
        }

    }
}
