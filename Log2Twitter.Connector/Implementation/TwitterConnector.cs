using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Log2Twitter.Connector.Models;
using System.Configuration;

namespace Log2Twitter.Connector.Implementation
{
    public class TwitterConnector : ConnectorBase, ITwitterConnector
    {
        private readonly string _oauthConsumerKey;
        private readonly string _oauthConsumerSecret;
        private readonly string _oathVersion;
        private readonly string _oauthSignatureMethod;
        private string _callBackUrl;


        public TwitterConnector()
        {
            _oauthConsumerKey = ConfigurationManager.AppSettings["Log2Twitter.ConsumerKey"];
            _oauthConsumerSecret = ConfigurationManager.AppSettings["Log2Twitter.ConsumerSecret"];
            _oathVersion = "1.0";
            _oauthSignatureMethod = "HMAC-SHA1";
        }

        public ConnectorResponse<TwitterOAuthResponse> GetRequestToken(string callBackUrl)
        {
            _callBackUrl = callBackUrl;
            const string baseUrl = "https://api.twitter.com/oauth/request_token";
            return PostWebRequest(baseUrl, CreateAuthorizationHeader(baseUrl, null, null));
        }

        public ConnectorResponse<TwitterOAuthResponse> GetAccessToken(string callBackUrl, string oauthToken, string oauthVerifier)
        {
            _callBackUrl = callBackUrl;
            const string baseUrl = "https://api.twitter.com/oauth/access_token";
            return PostWebRequest(baseUrl, CreateAuthorizationHeader(baseUrl, oauthToken, oauthVerifier));
        }

        public object PostUpdate(string message, string clientToken, string clientSecret)
        {
            var url = "https://api.twitter.com/1.1/statuses/update.json";
            var requestUrl = string.Format("{0}?status={1}", url, Uri.EscapeDataString(message));

            return PostWebRequest(requestUrl, CreatePostHeader(url, clientToken, clientSecret, message));
        }

        private ConnectorResponse<TwitterOAuthResponse> PostWebRequest(string baseUrl, string authorizationHeader)
        {
            try
            {
                var headers = new Dictionary<string, string>
                {
                    {"Authorization", authorizationHeader}
                };

                var response = PostWebRequest(baseUrl, null, null, headers);
                if (!response.Success)
                {
                    return CreateErrorResponse<TwitterOAuthResponse>(response.ErrorMessage);
                }


                return CreateSuccessResponse(TryParseResponse(response.Data));
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<TwitterOAuthResponse>(ex.Message);
            }
        }



        private string CreateAuthorizationHeader(string baseUrl, string oauthToken, string oauthVerifier)
        {
            var oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var oathTimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);

            var headerBuilder = new StringBuilder();
            headerBuilder.Append("OAuth ");

            var parameters = new SortedDictionary<string, string>
                                 {
                                     { "oauth_nonce", oauthNonce },
                                     { "oauth_callback", _callBackUrl },
                                     { "oauth_signature_method", _oauthSignatureMethod },
                                     { "oauth_timestamp", oathTimestamp },
                                     { "oauth_consumer_key", _oauthConsumerKey },
                                     { "oauth_version", _oathVersion }
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

        private string CreatePostHeader(string baseUrl, string oauthToken, string oauthSecret, string message)
        {
            var oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var oathTimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);

            var headerBuilder = new StringBuilder();
            headerBuilder.Append("OAuth ");

            var parameters = new SortedDictionary<string, string>
                                   {
                                        { "oauth_consumer_key", _oauthConsumerKey },
                                        { "oauth_nonce", oauthNonce },
                                        { "oauth_signature_method", _oauthSignatureMethod },
                                        { "oauth_timestamp", oathTimestamp },
                                        { "oauth_token", oauthToken },
                                        { "oauth_version", _oathVersion },
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



        #region Header Signature
        private string CreateSignature(string baseUrl, string method, SortedDictionary<string, string> parameters, string secret)
        {
            var sortedDic = parameters.ToDictionary(p => Uri.EscapeDataString(p.Key), p => Uri.EscapeDataString(p.Value)).OrderBy(p => p.Key);
            var count = 0;
            var paraBuilder = new StringBuilder();
            foreach (var k in sortedDic)
            {
                count++;
                paraBuilder.Append($"{k.Key}={k.Value}");

                if (count < sortedDic.Count())
                {
                    paraBuilder.Append("&");
                }
            }
            var parameterString = paraBuilder.ToString();

            var signatureBaseString = CreateSignatureBaseString(baseUrl, method, parameterString);

            var signingKey = CreateSigningKey(secret);

            return EncodeSignature(signatureBaseString, signingKey);
        }

        private string CreateSigningKey(string token)
        {
            var signBuilder = new StringBuilder();
            //The signing key is simply the percent encoded consumer secret,
            signBuilder.Append(Uri.EscapeDataString(_oauthConsumerSecret));
            // followed by an ampersand character ‘&’, 
            signBuilder.Append("&");
            //followed by the percent encoded token secret:
            //Note that there are some flows, such as when obtaining a request token, 
            //where the token secret is not yet known. In this case, the signing key should consist of the percent 
            //encoded consumer secret followed by an ampersand character ‘&’.
            if (!string.IsNullOrEmpty(token))
            {
                signBuilder.Append(Uri.EscapeDataString(token));
            }
            return signBuilder.ToString();
        }

        private static string CreateSignatureBaseString(string url, string method, string parameterString)
        {
            var signBase = new StringBuilder();

            //Convert the HTTP Method to uppercase and set the output string equal to this value.
            signBase.Append(method.ToUpper());
            //Append the ‘&’ character to the output string.
            signBase.Append("&");
            //Percent encode the URL and append it to the output string.
            signBase.Append(Uri.EscapeDataString(url));
            //Append the ‘&’ character to the output string.
            signBase.Append("&");

            //Percent encode the parameter string and append it to the output string.
            signBase.Append(Uri.EscapeDataString(parameterString));

            return signBase.ToString();
        }

        private static string EncodeSignature(string signatureBase, string signingKey)
        {
            //Finally, the signature is calculated by passing the signature base string and signing key to the HMAC-SHA1 hashing algorithm. 
            var hmacsha1 = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));

            //The output of the HMAC signing function is a binary string. This needs to be base64 encoded to produce the signature string.
            return Convert.ToBase64String(hmacsha1.ComputeHash(new ASCIIEncoding().GetBytes(signatureBase)));
        }
        #endregion

        #region Parse results
        private static bool TryParseValues(string response, out Dictionary<string, string> values)
        {
            values = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(response))
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
        #endregion
    }
}
