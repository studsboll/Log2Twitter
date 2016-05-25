using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Log2Twitter.Connector.Models;

namespace Log2Twitter.Connector.Implementation
{
    public abstract class TwitterConnectorBase
    {
        protected const string OAuthVersion = "1.0";
        protected const string OAuthSignatureMethod = "HMAC-SHA1";

        protected readonly string ConsumerKey;
        protected readonly string ConsumerSecret;

        protected TwitterConnectorBase(string consumerKey, string consumerSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }

        /// <summary>
        /// A value that makes our signed requests unique.
        /// </summary>
        /// <returns>Unique identifier</returns>
        protected string CreateOAuthNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
        }


        /// <summary>
        /// Creates a timestamp to make sure the header isn't tempered with. This is only valid
        /// for a few minutes and will be embeded in the rest of the signingkeys.
        /// </summary>
        /// <returns>Timestamp for use in header.</returns>
        protected string CreateTimestamp()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// It is very important that the parameters appear sorted by their encoded key.
        /// </summary>
        /// <param name="baseUrl">Base Url for request, no extra parameters.</param>
        /// <param name="method">POST/GET etc.</param>
        /// <param name="parameters">Collection of parameters to include</param>
        /// <param name="tokenSecret">Client token secret. If OAuth request, then empty.</param>
        /// <returns></returns>
        protected string CreateSignature(string baseUrl, string method, SortedDictionary<string, string> parameters, string tokenSecret)
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
            var signatureBaseString = CreateSignatureBase(baseUrl, method, parameterString);
            var signingKey = CreateSigningKey(tokenSecret);
            return EncodeSignature(signatureBaseString, signingKey);
        }

        /// <summary>
        /// The signing key is simply the percent encoded consumer secret,
        /// followed by an ampersand character ‘&’,
        /// followed by the percent encoded token secret:
        /// Note that there are some flows, such as when obtaining a request token, 
        /// where the token secret is not yet known. In this case, the signing key should consist of the percent 
        /// encoded consumer secret followed by an ampersand character ‘&’.
        /// </summary>
        /// <param name="tokenSecret">Client Token Secret. If OAuth request, then empty.</param>
        /// <returns>Signature</returns>
        protected string CreateSigningKey(string tokenSecret)
        {
            var signBuilder = new StringBuilder();
            signBuilder.Append(Uri.EscapeDataString(ConsumerSecret));
            signBuilder.Append("&");
            if (!string.IsNullOrEmpty(tokenSecret))
            {
                signBuilder.Append(Uri.EscapeDataString(tokenSecret));
            }
            return signBuilder.ToString();
        }

        /// <summary>
        /// Convert the HTTP Method to uppercase and set the output string equal to this value.
        /// Append the ‘&’ character to the output string.
        /// Percent encode the URL and append it to the output string.
        /// Append the ‘&’ character to the output string.
        /// Percent encode the parameter string and append it to the output string.
        /// </summary>
        /// <param name="url">Base request url. No parameters.</param>
        /// <param name="method">POST/GET etc</param>
        /// <param name="parameterString">UrlEncoded string with all parameters</param>
        /// <returns></returns>
        protected static string CreateSignatureBase(string url, string method, string parameterString)
        {
            var signBase = new StringBuilder();
            signBase.Append(method.ToUpper());
            signBase.Append("&");
            signBase.Append(Uri.EscapeDataString(url));
            signBase.Append("&");
            signBase.Append(Uri.EscapeDataString(parameterString));
            return signBase.ToString();
        }

        /// <summary>
        /// Finally, the signature is calculated by passing the signature base string and signing key to the HMAC-SHA1 hashing algorithm.
        /// The output of the HMAC signing function is a binary string. This needs to be base64 encoded to produce the signature string.
        /// </summary>
        /// <param name="signatureBase">Obtained through CreateSignatureBase()</param>
        /// <param name="signingKey">Obtained through CreateSigningKey()</param>
        /// <returns>Encoded Signature.</returns>
        protected static string EncodeSignature(string signatureBase, string signingKey)
        {
            var hmacsha1 = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));
            return Convert.ToBase64String(hmacsha1.ComputeHash(new ASCIIEncoding().GetBytes(signatureBase)));
        }


        /// <summary>
        /// Post a request.
        /// </summary>
        /// <param name="url">Base Url</param>
        /// <param name="authorizationHeader">Authorization header</param>
        /// <returns></returns>
        protected ConnectorResponse<string> PostWebRequest(string url, string authorizationHeader)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent("")
                };
                requestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                var response = client.SendAsync(requestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content.ReadAsStringAsync().Result;
                    return new ConnectorResponse<string>(true, data);
                }
                return new ConnectorResponse<string>(false, null, response.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                return new ConnectorResponse<string>(false, null, ex.Message);
            }
        }


        protected async Task<ConnectorResponse<string>> PostWebRequestAsync(string url, string authorizationHeader)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent("")
                };
                requestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return new ConnectorResponse<string>(true, data);
                }
                return new ConnectorResponse<string>(false, null, response.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                return new ConnectorResponse<string>(false, null, ex.Message);
            }
        }
    }
}
