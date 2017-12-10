using Microsoft.Bing.Speech;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Transcriber
{
    public sealed class CognitiveServicesAuthorizationProvider : IAuthorizationProvider
    {
        /// <summary>
        /// The fetch token URI
        /// </summary>
        private const string FetchTokenUri = "https://api.cognitive.microsoft.com/sts/v1.0";
        //private const string FetchTokenUri = "https://westus.api.cognitive.microsoft.com/sts/v1.0";

        /// <summary>
        /// The subscription key
        /// </summary>
        private readonly string subscriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="CognitiveServicesAuthorizationProvider" /> class.
        /// </summary>
        /// <param name="subscriptionKey">The subscription identifier.</param>
        public CognitiveServicesAuthorizationProvider(string subscriptionKey)
        {
            if (subscriptionKey == null)
            {
                throw new ArgumentNullException(nameof(subscriptionKey));
            }

            if (string.IsNullOrWhiteSpace(subscriptionKey))
            {
                throw new ArgumentException(nameof(subscriptionKey));
            }

            this.subscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Gets the authorization token asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the string parameter contains the next the authorization token.
        /// </returns>
        /// <remarks>
        /// This method should always return a valid authorization token at the time it is called.
        /// </remarks>
        public Task<string> GetAuthorizationTokenAsync()
        {
            return FetchToken(FetchTokenUri, this.subscriptionKey);
        }

        /// <summary>
        /// Fetches the token.
        /// </summary>
        /// <param name="fetchUri">The fetch URI.</param>
        /// <param name="subscriptionKey">The subscription key.</param>
        /// <returns>An access token.</returns>
        private static async Task<string> FetchToken(string fetchUri, string subscriptionKey)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                var uriBuilder = new UriBuilder(fetchUri);
                uriBuilder.Path += "/issueToken";

                using (var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null).ConfigureAwait(false))
                {
                    return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
