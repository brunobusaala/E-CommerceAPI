using System.Text;

namespace CrudeApi.Middleware
{
    public class AuthDiagnosticsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthDiagnosticsMiddleware> _logger;

        public AuthDiagnosticsMiddleware(
            RequestDelegate next,
            ILogger<AuthDiagnosticsMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log request headers
            var headers = new StringBuilder();
            foreach (var header in context.Request.Headers)
            {
                // Mask sensitive data
                var value = header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                    ? (
                        header.Value.ToString().Length > 15
                            ? $"{header.Value.ToString().Substring(0, 15)}..."
                            : header.Value.ToString()
                    )
                    : header.Value.ToString();

                headers.AppendLine($"{header.Key}: {value}");
            }

            _logger.LogInformation($"Request path: {context.Request.Path}, Headers:\n{headers}");

            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                _logger.LogInformation("Authorization header found");
            }
            else
            {
                _logger.LogWarning("No Authorization header found");
            }

            await _next(context);

            _logger.LogInformation($"Response status code: {context.Response.StatusCode}");

            if (context.Response.StatusCode == 401)
            {
                _logger.LogWarning("401 Unauthorized response returned");
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogWarning(
                        "User is authenticated but still got 401 - possible authorization issue"
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "User is not authenticated - possible token validation issue"
                    );
                }
            }
        }
    }

    public static class AuthDiagnosticsMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthDiagnostics(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthDiagnosticsMiddleware>();
        }
    }
}
