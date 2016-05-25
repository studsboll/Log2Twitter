using Log2Twitter.Connector.Models;

namespace Log2Twitter.Connector
{
    public interface ITwitterOAuthClient
    {
        ConnectorResponse<TwitterOAuthResponse> GetRequestToken(string callbackUrl);

        ConnectorResponse<TwitterOAuthResponse> GetAccessToken(string callbackUrl, string requestToken,
            string requestVerifier);
    }
}
