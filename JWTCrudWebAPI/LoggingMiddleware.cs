namespace JWTCrudWebAPI
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // Log request details
            Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
            Console.WriteLine($"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"))}");

            // Call the next middleware in the pipeline
            await _next(context);

            // Log response status code
            Console.WriteLine($"Response: {context.Response.StatusCode}");
        }
    }
}
