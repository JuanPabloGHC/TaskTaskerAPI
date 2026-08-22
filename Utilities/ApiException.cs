namespace TaskTaskerAPI.Utilities
{
    /// <summary>
    /// A controlled error meant to be returned to the client with a specific
    /// HTTP status code. Anything that is not an ApiException is treated as an
    /// unexpected failure (500) by the exception-handling middleware.
    /// </summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(int statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }
    }
}
