using ServiceStack;

namespace Log2Twitter.Api.Models
{
    [Route("/oauth")]
    public class TwitterOauth
    {
        public string oauth_token { get; set; }
        public string oauth_verifier { get; set; }
    }
}