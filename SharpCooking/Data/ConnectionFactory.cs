using SQLite;

namespace SharpCooking.Data
{
    public interface IConnectionFactory
    {
        SQLiteAsyncConnection GetConnection();
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
    }
}
