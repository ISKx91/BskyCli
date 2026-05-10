using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BskyCli.bsky.client
{
    internal partial class Client
    {
        /// <summary>
        /// HTTPリクエストを送信するためのユーティリティクラスです。
        /// </summary>
        internal class Http
        {
            internal static async Task<HttpResponseMessage> HttpPost(string requestUri, AuthenticationHeaderValue? authenticationHeaderValue, object? payload)
            {
                using var httpClient = new HttpClient();

                if (authenticationHeaderValue != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
                }

                StringContent? content = null;
                if (payload != null)
                {
                    var json = JsonSerializer.Serialize(payload);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await httpClient.PostAsync(requestUri, content);

                return response;
            }
        }
    }
}
