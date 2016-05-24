using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log2Twitter.Connector.Models
{
    public class TwitterOAuthResponse
    {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string RedirectUrl { get; set; }
            public string OAuthToken { get; set; }
            public string OAuthTokenSecret { get; set; }
            public string UserId { get; set; }
            public string ScreenName { get; set; }
    }
}
