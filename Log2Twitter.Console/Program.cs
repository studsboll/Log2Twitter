using System.Reflection;
using log4net;
using Log2Twitter.Connector;
using Log2Twitter.Connector.Implementation;

namespace Log2Twitter.Console
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            var twitterConnector = new TwitterConnector();


            var message = System.Console.ReadLine();

            var r = twitterConnector.PostUpdate(message, "417978678-H0e6oJzXyXzUDCkTgTWohMUQLcWz9oVJG258Zfmu", "Tg9hoqN66rb5IJB0LVVDNFA3cwkopS1eHjXSIOFX0eQWf");
            System.Console.WriteLine("Response="+r);
        }
    }
}
