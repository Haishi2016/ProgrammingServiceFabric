using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using WordCounter.Interfaces;
using System.Data.SqlClient;
using System.Text;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.KeyVault;

namespace WordCounter
{
    [StatePersistence(StatePersistence.None)]
    internal class WordCounter : Actor, IWordCounter
    {
        private string mConnectionString;
        public WordCounter(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
       
        }

        private async Task ensureConnetionstring()
        {
            if (string.IsNullOrEmpty(mConnectionString))
            {
                var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(getToken));
                var config = this.ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                var secretUri = config.Settings.Sections["ActorSettings"].Parameters["SecretUri"].Value;
                mConnectionString = (await kv.GetSecretAsync(secretUri)).Value;
            }
        }
        private async Task<string> getToken(string authority, string resource, string cope)
        {
            var config = this.ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var clientId = config.Settings.Sections["ActorSettings"].Parameters["ClientId"].Value;
            var clientSecret = config.Settings.Sections["ActorSettings"].Parameters["ClientSecret"].Value;
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);
            return result.AccessToken;
        }
        public async Task CountWordsAsync(string sentence, CancellationToken cancellationToken)
        {
            await ensureConnetionstring();
            using (SqlConnection connection = new SqlConnection(mConnectionString))
            {
                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("BEGIN TRAN ");
                sb.Append("IF EXISTS (SELECT * FROM WordFrequency WHERE [Word]=@word) ");
                sb.Append("BEGIN ");
                sb.Append("    UPDATE WordFrequency SET [Count]=[Count]+1 ");
                sb.Append("    WHERE [Word]=@word ");
                sb.Append("END ELSE BEGIN ");
                sb.Append("    INSERT INTO WordFrequency ([Word], [Count]) ");
                sb.Append("    VALUES (@word, 1) ");
                sb.Append("END ");
                sb.Append("COMMIT TRAN ");
                String sql = sb.ToString();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("word", System.Data.SqlDbType.NVarChar, 80));
                    string[] words = sentence.Split(new char[] { ' ', ',', '.', '?', '!' });
                    foreach (string word in words)
                    {
                        string cleanWord = word.Trim().ToUpper();
                        if (!string.IsNullOrEmpty(cleanWord))
                        {
                            command.Parameters["word"].Value = cleanWord;
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
    }
}
