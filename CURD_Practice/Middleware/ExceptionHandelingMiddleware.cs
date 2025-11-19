using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;

namespace CURD_Practice.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionHandelingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandelingMiddleware> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public ExceptionHandelingMiddleware(RequestDelegate next, ILogger<ExceptionHandelingMiddleware> logger, IDiagnosticContext diagnosticContext)
        {
            _next = next;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                if(ex.InnerException != null)
                {
                    _logger.LogError("{ExpectionType} {ExceptionMessage}",
                        ex.InnerException.GetType().ToString(), 
                        ex.InnerException.Message);
                }
                else
                {
                    _logger.LogError("{ExpectionType} {ExceptionMessage}",
                       ex.GetType().ToString(),
                       ex.Message);
                }

                //httpContext.Response.StatusCode = 500;
                //await httpContext.Response.WriteAsJsonAsync("Error has occoured");

                throw;
              
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandelingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandelingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandelingMiddleware>();
        }
    }
}
