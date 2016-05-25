using System.Configuration;
using System.Reflection;
using log4net;
using Log2Twitter.Connector.Implementation;

namespace Log2Twitter.Console
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            var client = new TwitterClient(
                ConfigurationManager.AppSettings["Log2Twitter.ConsumerKey"], 
                ConfigurationManager.AppSettings["Log2Twitter.ConsumerSecret"],
                ConfigurationManager.AppSettings["Log2Twitter.ClientToken"],
                ConfigurationManager.AppSettings["Log2Twitter.ClientSecret"]);

            var message = System.Console.ReadLine();

            var r = client.PostUpdate(message);

            System.Console.WriteLine("Response="+r);
        }
    }
}
