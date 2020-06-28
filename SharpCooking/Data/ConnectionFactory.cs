using SharpCooking.Models;
using SQLite;
using System.Threading.Tasks;

namespace SharpCooking.Data
{
    public interface IConnectionFactory
    {
        SQLiteAsyncConnection GetConnection();
        Task MigrateDbToLatestAsync();
    }

    public class ConnectionFactory : IConnectionFactory
    {
        private readonly string _databasePath;

        public ConnectionFactory(string databasePath)
        {
            _databasePath = databasePath;
        }

        public SQLiteAsyncConnection GetConnection()
        {
            return new SQLiteAsyncConnection(_databasePath,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache,
                false);
        }

        public async Task MigrateDbToLatestAsync()
        {
            var connection = GetConnection();
            await connection.CreateTableAsync<Recipe>();
            await connection.CreateTableAsync<Uom>();
        }
    }
}
