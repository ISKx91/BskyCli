using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BskyCli.bsky.client
{
    internal partial class Client
    {
        internal class Record
        {
            internal Record()
            {

            }


            internal async Task<HttpStatusCode> CreatePost(Session session, string text, DateTime? createdAt)
            {
                using var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", session.accessJwt);

                var payload = new
                {
                    repo = session.handle,
                    collection = "app.bsky.feed.post",
                    record = new
                    {
                        text,
                        createdAt = createdAt?.ToString("o") // ISO 8601 format
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://bsky.social/xrpc/com.atproto.repo.createRecord", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Post failed: HTTP Error {response.StatusCode}");
                }

                return response.StatusCode;
            }
        }
    }
}
