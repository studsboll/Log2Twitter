namespace Log2Twitter.Connector.Models
{
    public class ConnectorResponse<T>
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }

        public ConnectorResponse() { }

        public ConnectorResponse(bool success, T data) : this()
        {
            Data = data;
            Success = success;
        }

        public ConnectorResponse(bool success, T data, string errorMessage) : this(success, data)
        {
            ErrorMessage = errorMessage;
        }
    }
}
