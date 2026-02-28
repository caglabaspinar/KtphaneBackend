using System.Net;
using System.Text.Json;

namespace LMS.Backend.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;

                _logger.LogError(ex,
                    "Unhandled exception. TraceId: {TraceId}, Method: {Method}, Path: {Path}, User: {User}",
                    traceId,
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.User?.Identity?.Name ?? "Anonymous");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json; charset=utf-8";

                var payload = new
                {
                    message = "Beklenmeyen bir hata oluştu.",
                    traceId
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}

//Bu dosya, uygulamada yakalanmamış hataları tek bir yerde loglayıp standart JSON hata cevabı
//döndürür ve `Program.cs` içinde `UseGlobalExceptionMiddleware()` ile middleware pipeline’ına
//eklenerek tüm controller ve servis katmanıyla bağlantılı çalışır.

