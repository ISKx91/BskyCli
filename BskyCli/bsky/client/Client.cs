using System.Net;

namespace BskyCli.bsky.client
{
    /// <summary>
    /// Clientクラスは、BlueskyのAPIと対話するための主要なクラスであり、ユーザーのセッション管理や投稿作成などの機能を提供します。
    /// </summary>
    internal partial class Client
    {
        internal Session session { get; private set; }

        private Client(Session session)
        { 
            this.session = session;
        }

        /// <summary>
        /// Blueskyへログインを行い、セッションを確立するためのメソッドです。
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static async Task<Client> Login(string identifier, string password)
        {
            var session = await Session.CreateSession(identifier, password);
            return new Client(session);   
        }

        /// <summary>
        /// Blueskyからログアウトし、セッションを削除するためのメソッドです。
        /// </summary>
        /// <returns></returns>
        internal async Task<HttpResponseMessage> Logout()
        {
            return await this.session.DeleteSession();
        }

        internal async Task<HttpResponseMessage> Post(string text, DateTime? createdAt)
        {
            if (createdAt == null)
            {
                createdAt = DateTime.Now;
            }

            var record = new Record();
            var response = await record.CreatePost(this.session, text, createdAt);
            return response;
        }

    }
}
