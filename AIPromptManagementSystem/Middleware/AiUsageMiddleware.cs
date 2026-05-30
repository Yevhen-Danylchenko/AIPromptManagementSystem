namespace AIPromptManagementSystem.Middleware
{
    public class AiUsageMiddleware
    {
        private readonly RequestDelegate _next;

        public AiUsageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the next middleware in the pipeline and logs usage when the request path starts with
        /// '/Prompts/UsePrompt'.
        /// </summary>
        /// <remarks>When the request path starts with '/Prompts/UsePrompt', writes a usage entry with a
        /// UTC timestamp to the console before invoking the next middleware.</remarks>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Prompts/UsePrompt"))
            {
                Console.WriteLine($"AI Prompt used at {DateTime.UtcNow}");
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Provides extension methods for IApplicationBuilder to add AI usage logging middleware to the application's
    /// request pipeline.
    /// </summary>
    /// <remarks>Call UseAiUsageLogging in Configure(IApplicationBuilder) to register AiUsageMiddleware. Place
    /// it appropriately in the middleware order so requests are logged as intended.</remarks>
    public static class AiUsageMiddlewareExtensions
    {
        public static IApplicationBuilder UseAiUsageLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AiUsageMiddleware>();
        }
    }
}
