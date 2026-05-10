using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace BskyCli.bsky.client
{
    internal partial class Client
    {
        /// <summary>
        /// Sessionクラスは、Blueskyへのログインセッションを管理するためのクラスです。
        /// ユーザーの認証情報やセッションに関連するデータを保持し、セッションの作成や削除などの操作を提供します。
        /// </summary>
        internal class Session
        {
            private const string ENDPOINT_CREATE_SESSION = "https://bsky.social/xrpc/com.atproto.server.createSession";
            private const string ENDPOINT_DELETE_SESSION = "https://bsky.social/xrpc/com.atproto.server.deleteSession";

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
                var response = await Http.HttpPost(Session.ENDPOINT_CREATE_SESSION, null, new { identifier, password });
                var responseContent = await response.Content.ReadAsStringAsync();

                var jsonDoc    = JsonDocument.Parse(responseContent);
                var accessJwt  = jsonDoc.RootElement.GetProperty("accessJwt").GetString() ?? string.Empty;
                var refreshJwt = jsonDoc.RootElement.GetProperty("refreshJwt").GetString() ?? string.Empty;
                var handle     = jsonDoc.RootElement.GetProperty("handle").GetString() ?? string.Empty;
                var did        = jsonDoc.RootElement.GetProperty("did").GetString() ?? string.Empty;
                var email      = jsonDoc.RootElement.GetProperty("email").GetString() ?? string.Empty;
                return new Session(accessJwt, refreshJwt, handle, did, email);
            }

            internal async Task<HttpResponseMessage> DeleteSession()
            {
                var response = await Http.HttpPost(Session.ENDPOINT_DELETE_SESSION, new AuthenticationHeaderValue("Bearer", this.refreshJwt), null);

                return response;
            }
        }
    }
}
