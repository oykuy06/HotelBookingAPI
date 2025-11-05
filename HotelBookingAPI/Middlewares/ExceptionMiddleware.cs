using System.Net;
using System.Text.Json;

namespace HotelBookingAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public readonly ILogger<ExceptionMiddleware> _logger; //error log

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) //main function of middleware
        {
            try
            {
                await _next(context); //Forwards the request to the next middleware or controller
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex) //json
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error. Please try again later",
                Detail = ex.Message
            };

            var json = JsonSerializer.Serialize(response); //object is converted to a JSON string.
            return context.Response.WriteAsync(json); //Sends the JSON as an HTTP response.
        }
    }
}
