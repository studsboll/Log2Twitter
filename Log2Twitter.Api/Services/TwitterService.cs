using System.Configuration;
using Log2Twitter.Api.Models;
using Log2Twitter.Connector;
using Log2Twitter.Connector.Implementation;
using ServiceStack;

namespace Log2Twitter.Api.Services
{
    public class TwitterService : Service
    {
        private readonly ITwitterOAuthClient _twitterOAuthClient;
        private readonly string _hostUrl;

        public TwitterService()
        {
            _twitterOAuthClient = new TwitterOAuthClient(
                ConfigurationManager.AppSettings["Log2Twitter.ConsumerKey"], 
                ConfigurationManager.AppSettings["Log2Twitter.ConsumerSecret"]);

            _hostUrl = ConfigurationManager.AppSettings["Log2Twitter.Host"] + "oauth";
        }

        public object Get(TwitterOauth request)
        {
            if (string.IsNullOrEmpty(request.oauth_token) || string.IsNullOrEmpty(request.oauth_verifier))
            {
                return _twitterOAuthClient.GetRequestToken(_hostUrl);
            }

            return _twitterOAuthClient.GetAccessToken(_hostUrl, request.oauth_token,
                request.oauth_verifier);
        }
    }
}