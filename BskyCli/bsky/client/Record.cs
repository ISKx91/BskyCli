using System.Net.Http.Headers;

namespace BskyCli.bsky.client
{
    internal partial class Client
    {
        internal class Record
        {
            private const string ENDPOINT_CREATE_RECORD = "https://bsky.social/xrpc/com.atproto.repo.createRecord";

            internal Record()
            {

            }


            internal async Task<HttpResponseMessage> CreatePost(Session session, string text, DateTime? createdAt)
            {
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

                var response = await Http.HttpPost(ENDPOINT_CREATE_RECORD, new AuthenticationHeaderValue("Bearer", session.accessJwt), payload);

                return response;
            }
        }
    }
}
