namespace Dot.Net.WebApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        public const string AnonymousUser = "anonymous";

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            string user = context.User.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name ?? AnonymousUser
                : AnonymousUser;

            _logger.LogInformation(
                "{User} {Method} {Path} -> {StatusCode}",
                user,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);
        }
    }
}
