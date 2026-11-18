using System.Security.Claims;
using Dot.Net.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace P7.Tests
{
    public class RequestLoggingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WithAuthenticatedUser_LogsUserRouteAndStatus()
        {
            var logger = new ListLogger();
            var context = CreateContext();
            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "alice") }, "Test"));

            await CreateMiddleware(logger, StatusCodes.Status204NoContent).InvokeAsync(context);

            Assert.Equal("alice GET /bidlists -> 204", Assert.Single(logger.Messages));
        }

        [Fact]
        public async Task InvokeAsync_WithoutUser_LogsAnonymous()
        {
            var logger = new ListLogger();
            var context = CreateContext();

            await CreateMiddleware(logger, StatusCodes.Status401Unauthorized).InvokeAsync(context);

            Assert.Equal("anonymous GET /bidlists -> 401", Assert.Single(logger.Messages));
        }

        private static DefaultHttpContext CreateContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/bidlists";
            return context;
        }

        private static RequestLoggingMiddleware CreateMiddleware(ListLogger logger, int statusCode)
        {
            return new RequestLoggingMiddleware(
                context =>
                {
                    context.Response.StatusCode = statusCode;
                    return Task.CompletedTask;
                },
                logger);
        }

        private sealed class ListLogger : ILogger<RequestLoggingMiddleware>, IDisposable
        {
            public List<string> Messages { get; } = new List<string>();

            public IDisposable BeginScope<TState>(TState state) => this;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                Messages.Add(formatter(state, exception));
            }

            public void Dispose()
            {
            }
        }
    }
}
