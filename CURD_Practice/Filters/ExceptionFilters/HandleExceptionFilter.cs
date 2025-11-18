using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CURD_Practice.Filters.ExceptionFilters
{
    public class HandleExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<HandleExceptionFilter> _logger;
        private readonly IHostEnvironment _hostEnviroment;

        public HandleExceptionFilter(ILogger<HandleExceptionFilter> logger, IHostEnvironment hostEnviroment)
        {
            _logger = logger;
            _hostEnviroment = hostEnviroment;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            _logger.LogError("Excpetion filter {FilterName}.{MethodeName} \n" +
                "{ExcpetionType} \n" +
                "{ExceptionMessage}"
                , nameof(HandleExceptionFilter), nameof(OnExceptionAsync), 
                context.Exception.GetType().ToString(), 
                context.Exception.Message);

            if(_hostEnviroment.IsDevelopment())
            {
                context.Result = new ContentResult()
                {
                    Content = context.Exception.Message,
                    StatusCode = 500
                };
            }

        }
    }
}
