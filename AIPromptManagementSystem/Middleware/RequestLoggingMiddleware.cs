using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace AIPromptManagementSystem.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the next middleware for the specified HttpContext, measures execution time, and logs request
        /// details.
        /// </summary>
        /// <remarks>Logs a UTC timestamp, HTTP method, request path, response status code, and elapsed
        /// time in milliseconds to the console.</remarks>
        /// <param name="context">The HttpContext for the current request.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            var log = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | " +
                      $"Method: {context.Request.Method} | " +
                      $"Path: {context.Request.Path} | " +
                      $"StatusCode: {context.Response.StatusCode} | " +
                      $"ExecutionTime: {stopwatch.ElapsedMilliseconds} ms";

            Console.WriteLine(log);
        }
    }

    /// <summary>
    /// Provides an extension method for IApplicationBuilder to add request logging middleware to the application's
    /// request pipeline.
    /// </summary>
    /// <remarks>Call from the application's Configure method to register RequestLoggingMiddleware via
    /// UseMiddleware<RequestLoggingMiddleware>. The middleware relies on the application's logging configuration and
    /// pipeline ordering to determine which requests and details are logged.</remarks>
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
