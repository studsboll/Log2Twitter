using System;
using System.Collections.Generic;
using System.Text;
using Log2Twitter.Connector.Models;

namespace Log2Twitter.Connector.Implementation
{
    public class TwitterOAuthClient : TwitterConnectorBase, ITwitterOAuthClient
    {
        private const string OAuthRequestTokenUrl = "https://api.twitter.com/oauth/request_token";
        private const string OAuthAccessTokenUrl = "https://api.twitter.com/oauth/access_token";

        public TwitterOAuthClient(string consumerKey, string consumerSecret) : 
            base(consumerKey, consumerSecret)
        {
        }

        public ConnectorResponse<TwitterOAuthResponse> GetRequestToken(string callbackUrl)
        {
            var header = CreateAuthorizationHeader(OAuthRequestTokenUrl, null, null, callbackUrl);
            var request = PostWebRequest(OAuthRequestTokenUrl, header);
            if (request.Success)
            {
                return new ConnectorResponse<TwitterOAuthResponse>(true, TryParseResponse(request.Data));
            }
            return new ConnectorResponse<TwitterOAuthResponse>(false, null, request.ErrorMessage);
        }

        public ConnectorResponse<TwitterOAuthResponse> GetAccessToken(string callbackUrl, string requestToken,
            string requestVerifier)
        {
            var header = CreateAuthorizationHeader(OAuthAccessTokenUrl, requestToken, requestVerifier, callbackUrl);
            var request = PostWebRequest(OAuthAccessTokenUrl, header);
            if (request.Success)
            {
                return new ConnectorResponse<TwitterOAuthResponse>(true, TryParseResponse(request.Data));
            }
            return new ConnectorResponse<TwitterOAuthResponse>(false, null, request.ErrorMessage);
        } 


        private string CreateAuthorizationHeader(string baseUrl, string oauthToken, string oauthVerifier, string callbackUrl)
        {
            var headerBuilder = new StringBuilder();
            headerBuilder.Append("OAuth ");

            var parameters = new SortedDictionary<string, string>
                                 {
                                     { "oauth_nonce", CreateOAuthNonce() },
                                     { "oauth_callback", callbackUrl },
                                     { "oauth_signature_method", OAuthSignatureMethod },
                                     { "oauth_timestamp", CreateTimestamp() },
                                     { "oauth_consumer_key", ConsumerKey },
                                     { "oauth_version", OAuthVersion }
                                 };

            if (!string.IsNullOrEmpty(oauthToken) && !string.IsNullOrEmpty(oauthVerifier))
            {
                parameters.Add("oauth_token", oauthToken);
                parameters.Add("oauth_verifier", oauthVerifier);
            }
            
            var signature = CreateSignature(baseUrl, "POST", parameters, oauthToken);
            foreach (var parameter in parameters)
            {
                headerBuilder.Append($"{parameter.Key}=\"{Uri.EscapeDataString(parameter.Value)}\", ");
            }
            headerBuilder.Append($"oauth_signature=\"{Uri.EscapeDataString(signature)}\"");

            return headerBuilder.ToString();
        }

        private static bool TryParseValues(string response, out Dictionary<string, string> values)
        {
            values = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(response) || !response.Contains("&"))
            {
                return false;
            }

            foreach (var s in response.Split('&'))
            {
                var parts = s.Split('=');
                values.Add(parts[0], parts[1]);
            }
            return true;
        }

        private TwitterOAuthResponse TryParseResponse(string response)
        {
            Dictionary<string, string> values;
            if (TryParseValues(response, out values))
            {
                var result = new TwitterOAuthResponse();
                if (values.ContainsKey("oauth_token") && values.ContainsKey("oauth_token_secret"))
                {
                    result.OAuthToken = values["oauth_token"];
                    result.OAuthTokenSecret = values["oauth_token_secret"];
                    result.Success = true;

                    // Final handshake
                    if (values.ContainsKey("screen_name") && values.ContainsKey("user_id"))
                    {
                        result.ScreenName = values["screen_name"];
                        result.UserId = values["user_id"];
                    }
                    else
                    {
                        result.RedirectUrl =
                            $"https://api.twitter.com/oauth/authenticate?oauth_token={result.OAuthToken}";
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Could not find required values";
                }
                return result;
            }
            return new TwitterOAuthResponse { Success = false, ErrorMessage = "Could not parse values." };
        }
    }
}
