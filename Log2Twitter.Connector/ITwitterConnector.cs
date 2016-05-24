using Log2Twitter.Connector.Models;

namespace Log2Twitter.Connector
{
    public interface ITwitterConnector
    {
        ConnectorResponse<TwitterOAuthResponse> GetRequestToken(string callBackUrl);

        ConnectorResponse<TwitterOAuthResponse> GetAccessToken(string callBackUrl, string oauthToken,
            string oauthVerifier);

        object PostUpdate(string message, string clientToken, string clientSecret);
    }
}
