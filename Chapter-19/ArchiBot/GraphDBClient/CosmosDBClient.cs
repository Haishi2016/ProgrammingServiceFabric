using ArchiBot.ArchiGraph;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;

namespace ArchiBot.GraphDBClient
{
    public class CosmosDBClient
    {
        private string mEndpoint;
        private string mAuthKey;        

        public CosmosDBClient(string endpoint, string authKey)
        {
            mEndpoint = endpoint;
            mAuthKey = authKey;
        }
        public DocumentClient Connect()
        {
            DocumentClient client = new DocumentClient(
                new Uri(mEndpoint),
                mAuthKey,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp });

            return client;
        }
    }
    public static class DocumentClientExtensions
    {
        public static async void WriteGraphs(this DocumentClient client, string database, string collection, List<AppGraph> appGraphs)
        {
            DocumentCollection graph = await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(database),
                new DocumentCollection { Id = collection },
                new RequestOptions { OfferThroughput = 1000 });

        }
    }
}
