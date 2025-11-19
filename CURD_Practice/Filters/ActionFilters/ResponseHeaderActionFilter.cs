using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Client;

namespace CURD_Practice.Filters.ActionFilters
{
    public class ResponseHeaderFilterFactoryAttribute : Attribute, IFilterFactory
    {
        public string? Key { set; get; }
        public string? Value { set; get; }
        public int Order { set; get; }

        public ResponseHeaderFilterFactoryAttribute(string key, string value, int order)
        {
            Key = key;
            Value = value;
            Order = order;
        }

        #region IFileterFactory
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<ResponseHeaderActionFilter>();
            filter.Key = Key;
            filter.Value = Value;
            filter.Order = Order;
            
            return filter;
        }
        #endregion
    }


    public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<ResponseHeaderActionFilter> _logger;

        public string Key { set; get; }
        public string Value { set; get; }
        public int Order { get; set; }

        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger)
        {  
            _logger = logger;
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

            context.HttpContext.Response.Headers[Key] = Value;
        }
    }
}
