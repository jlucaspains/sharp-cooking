using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SharpCooking.Data
{
    public interface IDataStore
    {
        Task<IEnumerable<TEntity>> QueryAsync<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class, new();
        Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class, new();
        Task<IEnumerable<TEntity>> AllAsync<TEntity>() where TEntity : class, new();
        Task InsertAsync<TEntity>(TEntity entity) where TEntity : class, new();
        Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class, new();
        Task UpsertAsync<TEntity>(TEntity entity) where TEntity : class, new();
        Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class, new();
        Task DeleteAllAsync<TEntity>() where TEntity : class, new();
    }

    public class DataStore : IDataStore
    {
        private readonly IConnectionFactory _connectionFactory;

        public DataStore(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class, new()
        {
            var connection = _connectionFactory.GetConnection();
            await connection.DeleteAsync(entity);
        }

        public async Task DeleteAllAsync<TEntity>() where TEntity : class, new()
        {
            var connection = _connectionFactory.GetConnection();
            await connection.DeleteAllAsync<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(Expression<Func<TEntity, bool>> where)
            where TEntity : class, new()
        {
            var connection = _connectionFactory.GetConnection();
            return await connection.Table<TEntity>().Where(where).ToListAsync();
        }

        public async Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> where)
            where TEntity : class, new()
        {
            var connection = _connectionFactory.GetConnection();
            return await connection.Table<TEntity>().FirstOrDefaultAsync(where);
        }

        public async Task<IEnumerable<TEntity>> AllAsync<TEntity>() where TEntity : class, new()
        {
            var connection = _connectionFactory.GetConnection();
            return await connection.Table<TEntity>().ToListAsync();
        }

        public async Task InsertAsync<TEntity>(TEntity entity) where TEntity : class, new()
        {
            var connection = _connectionFactory.GetConnection();

            await connection.InsertAsync(entity);
        }

        public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class, new()
        {
            var connection = _connectionFactory.GetConnection();

            await connection.UpdateAsync(entity);
        }

        public async Task UpsertAsync<TEntity>(TEntity entity) where TEntity : class, new()
        {
            var connection = _connectionFactory.GetConnection();

            await connection.InsertOrReplaceAsync(entity);
        }
    }
}
