using System.Net;
using System.Text;
using System.Text.Json;

namespace BskyCli.bsky.client
{
    internal partial class Client
    {
        internal class Session
        {
            internal string accessJwt { get; private set; }
            internal string refreshJwt { get; private set; }
            internal string handle { get; private set; }
            internal string did { get; private set; }
            internal string email { get; private set; }


            private Session(string accessJwt, string refreshJwt, string handle, string did, string email)
            {
                this.accessJwt  = accessJwt;
                this.refreshJwt = refreshJwt;
                this.handle     = handle;
                this.did        = did;
                this.email      = email;
            }

            internal static async Task<Session> CreateSession(string identifier, string password)
            {
                using var httpClient = new HttpClient();

                var payload = new
                {
                    identifier,
                    password
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = httpClient.PostAsync("https://bsky.social/xrpc/com.atproto.server.createSession", content).Result;

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Login failed: HTTP Error {response.StatusCode}");
                }

                var jsonDoc    = JsonDocument.Parse(responseContent);
                var accessJwt  = jsonDoc.RootElement.GetProperty("accessJwt").GetString() ?? string.Empty;
                var refreshJwt = jsonDoc.RootElement.GetProperty("refreshJwt").GetString() ?? string.Empty;
                var handle     = jsonDoc.RootElement.GetProperty("handle").GetString() ?? string.Empty;
                var did        = jsonDoc.RootElement.GetProperty("did").GetString() ?? string.Empty;
                var email      = jsonDoc.RootElement.GetProperty("email").GetString() ?? string.Empty;
                return new Session(accessJwt, refreshJwt, handle, did, email);
            }

            internal async Task<HttpStatusCode> DeleteSession()
            {
                using var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.refreshJwt);

                var response = httpClient.PostAsync("https://bsky.social/xrpc/com.atproto.server.deleteSession", null).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Logout failed: HTTP Error {response.StatusCode}");
                }

                return response.StatusCode;
            }
        }
    }
}
