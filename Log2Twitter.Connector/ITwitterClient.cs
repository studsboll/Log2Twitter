using System.Threading.Tasks;
using Log2Twitter.Connector.Models;

namespace Log2Twitter.Connector
{
    public interface ITwitterClient
    {
        Task<ConnectorResponse<object>> PostUpdateAsync(string message);
        ConnectorResponse<object> PostUpdate(string message);
    }
}
