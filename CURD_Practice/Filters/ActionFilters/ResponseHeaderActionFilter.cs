using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Client;

namespace CURD_Practice.Filters.ActionFilters
{
    public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<ResponseHeaderActionFilter> _logger;

        private readonly string _key;
        private readonly string _value;

        public int Order { get; set; }

        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value , int order)
        {
            _logger = logger;
            _key = key;
            _value = value;

            Order = order;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogInformation("{FilteredName}.{MethodName} Before"
               , nameof(ResponseHeaderActionFilter)
               , nameof(OnActionExecutionAsync));

            await next();//Must have this 

            _logger.LogInformation("{FilteredName}.{MethodName} After"
                , nameof(ResponseHeaderActionFilter)
                , nameof(OnActionExecutionAsync));

            context.HttpContext.Response.Headers[_key] = _value;
        }
    }
}
