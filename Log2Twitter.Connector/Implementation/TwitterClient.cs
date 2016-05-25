using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Log2Twitter.Connector.Models;

namespace Log2Twitter.Connector.Implementation
{
    public class TwitterClient : TwitterConnectorBase, ITwitterClient
    {
        private readonly string _clientToken;
        private readonly string _clientSecret;

        private const string StatusUpdateUrl = "https://api.twitter.com/1.1/statuses/update.json";

        public TwitterClient(string consumerKey, string consumerSecret, string clientToken, string clientSecret) 
            : base(consumerKey, consumerSecret)
        {
            _clientToken = clientToken;
            _clientSecret = clientSecret;
        }

        public async Task<ConnectorResponse<object>> PostUpdateAsync(string message)
        {
            var requestUrl = $"{StatusUpdateUrl}?status={Uri.EscapeDataString(message)}";
            var header = CreatePostHeader(StatusUpdateUrl, _clientToken, _clientSecret, message);
            var request = await PostWebRequestAsync(requestUrl, header);
            if (request.Success)
            {
                return new ConnectorResponse<object>(true, request.Data);
            }
            return new ConnectorResponse<object>(false, null, request.ErrorMessage);
        }

        public ConnectorResponse<object> PostUpdate(string message)
        {
            var requestUrl = $"{StatusUpdateUrl}?status={Uri.EscapeDataString(message)}";
            var header = CreatePostHeader(StatusUpdateUrl, _clientToken, _clientSecret, message);
            var request = PostWebRequest(requestUrl, header);
            if (request.Success)
            {
                return new ConnectorResponse<object>(true, request.Data);
            }
            return new ConnectorResponse<object>(false, null, request.ErrorMessage);
        }


        private string CreatePostHeader(string baseUrl, string oauthToken, string oauthSecret, string message)
        {
            var headerBuilder = new StringBuilder();
            headerBuilder.Append("OAuth ");

            var parameters = new SortedDictionary<string, string>
                                   {
                                        { "oauth_consumer_key", ConsumerKey },
                                        { "oauth_nonce", CreateOAuthNonce() },
                                        { "oauth_signature_method", OAuthSignatureMethod },
                                        { "oauth_timestamp", CreateTimestamp() },
                                        { "oauth_token", oauthToken },
                                        { "oauth_version", OAuthVersion },
                                        { "status", message}
                                   };

            var signature = CreateSignature(baseUrl, "POST", parameters, oauthSecret);
            foreach (var parameter in parameters)
            {
                headerBuilder.Append($"{parameter.Key}=\"{Uri.EscapeDataString(parameter.Value)}\", ");
            }
            headerBuilder.Append($"oauth_signature=\"{Uri.EscapeDataString(signature)}\"");

            var header = headerBuilder.ToString();
            return header;
        }
    }
}
