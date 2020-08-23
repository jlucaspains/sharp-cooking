using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharpCooking.Services
{
    public class HttpService : IHttpService, IDisposable
    {
        private bool disposedValue;
        private HttpClient _httpClient;

        public HttpService()
        {
            _httpClient = new HttpClient();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }

                _httpClient = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async Task<string> GetStringAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            return response.IsSuccessStatusCode ? await response.Content?.ReadAsStringAsync() : string.Empty;
        }

        public async Task<T> GetAsync<T>(string url) where T: class
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var stringResult = await response.Content?.ReadAsStringAsync();
            
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(stringResult);
        }
    }
}
