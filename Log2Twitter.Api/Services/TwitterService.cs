using System.Configuration;
using Log2Twitter.Api.Models;
using Log2Twitter.Connector;
using Log2Twitter.Connector.Implementation;
using ServiceStack;

namespace Log2Twitter.Api.Services
{
    public class TwitterService : Service
    {
        private readonly ITwitterConnector _twitterConnector;
        private readonly string _hostUrl;

        public TwitterService()
        {
            _twitterConnector = new TwitterConnector();
            _hostUrl = ConfigurationManager.AppSettings["Log2Twitter.Host"] + "oauth";
        }

        public object Get(TwitterOauth request)
        {
            if (string.IsNullOrEmpty(request.oauth_token) || string.IsNullOrEmpty(request.oauth_verifier))
            {
                return _twitterConnector.GetRequestToken(_hostUrl);
            }

            return _twitterConnector.GetAccessToken(_hostUrl, request.oauth_token,
                request.oauth_verifier);
        }
    }
}