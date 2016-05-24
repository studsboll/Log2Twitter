using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using log4net;
using Log2Twitter.Connector.Models;
using Newtonsoft.Json;

namespace Log2Twitter.Connector.Implementation
{
    public abstract class ConnectorBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected ConnectorResponse<string> GetWebRequest(string requestUrl, string accept = "application/json", IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            var client = new HttpClient();
            if (string.IsNullOrEmpty(accept))
            {
                accept = "application/json";
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            var response = client.GetAsync(requestUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                return CreateSuccessResponse(response.Content.ReadAsStringAsync().Result);
            }
            return CreateErrorResponse<string>(response.StatusCode.ToString());
        }

        protected ConnectorResponse<string> PostWebRequest(string requestUrl, object json, string accept = "application/json", IEnumerable<KeyValuePair<string, string>> headers = null, bool serializedata = true)
        {
            string data;
            if (serializedata)
            {
                data = JsonConvert.SerializeObject(json);
            }
            else
            {
                data = (string)json;
            }

            var client = new HttpClient();
            if (string.IsNullOrEmpty(accept))
            {
                accept = "application/json";
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(data)
            };
            requestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            var response = client.SendAsync(requestMessage).Result;
            if (response.IsSuccessStatusCode)
            {
                return CreateSuccessResponse(response.Content.ReadAsStringAsync().Result);
            }
            return CreateErrorResponse<string>(response.StatusCode.ToString());

        }


        protected ConnectorResponse<T> CreateSuccessResponse<T>(T data)
        {
            return new ConnectorResponse<T>(true, data);
        }

        protected ConnectorResponse<T> CreateErrorResponse<T>(string errorMessage)
        {
            Logger.Error(errorMessage);
            return new ConnectorResponse<T>(false, default(T), errorMessage);
        }
    }
}
