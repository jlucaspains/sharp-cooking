using System.Threading.Tasks;

namespace SharpCooking.Services
{
    public interface IHttpService
    {
        Task<string> GetStringAsync(string url);
        Task<T> GetAsync<T>(string url) where T : class;
    }
}
