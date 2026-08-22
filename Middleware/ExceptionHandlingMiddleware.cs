using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Middleware
{
    /// <summary>
    /// Central error handling: turns exceptions into the standard ApiResponse
    /// shape. Preferred path is <see cref="ApiException"/>; the legacy
    /// "code;message" convention thrown by repositories is still understood.
    /// Anything else is logged and returned as a generic 500 so internal
    /// details never reach the client.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this._next = next;
            this._logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this._next(context);
            }
            catch (Exception ex)
            {
                (int statusCode, string message) = Translate(ex);

                if (statusCode >= 500)
                    this._logger.LogError(ex, "Unhandled exception");

                context.Response.StatusCode = statusCode;

                await context.Response.WriteAsJsonAsync(new ApiResponse<string>
                {
                    StatusCode = statusCode,
                    Message = message,
                    Data = string.Empty
                });
            }
        }

        private static (int, string) Translate(Exception ex)
        {
            if (ex is ApiException api)
                return (api.StatusCode, api.Message);

            // Legacy convention: "409;Name already in use"
            string[] parts = ex.Message.Split(';');
            if (parts.Length >= 2 && int.TryParse(parts[0], out int code))
                return (code, string.Join(';', parts[1..]));

            return (500, "Internal server error");
        }
    }
}
