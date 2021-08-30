using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Navi_Server_Test.Helper
{
    public static class ClientHelper
    {
        public static async Task<ResponseEntity<T>> GetForEntity<T>(this HttpClient httpClient, string url,
            string token)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-API-AUTH", token);
            var response = await httpClient.GetAsync(url);
            var jsonString = await response.Content.ReadAsStringAsync();

            return new ResponseEntity<T>
            {
                Body = JsonConvert.DeserializeObject<T>(jsonString),
                StatusCode = (int) response.StatusCode,
                RawMessage = response
            };
        }

        public static async Task<ResponseEntity<T>> PostForEntity<T, TB>(this HttpClient httpClient, string url,
            TB toPost, string token)
        {
            // Add Token to header if exists.
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-API-AUTH", token);

            // Create Request Body
            var requestContent = new StringContent(
                JsonSerializer.Serialize(toPost),
                Encoding.UTF8,
                "application/json"
            );

            // Request && Deserialize
            var response = await httpClient.PostAsync(url, requestContent);
            var jsonString = await response.Content.ReadAsStringAsync();

            return new ResponseEntity<T>
            {
                Body = JsonConvert.DeserializeObject<T>(jsonString),
                StatusCode = (int) response.StatusCode,
                RawMessage = response
            };
        }
    }
}