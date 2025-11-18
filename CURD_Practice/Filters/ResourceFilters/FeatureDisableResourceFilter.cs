using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Runtime.InteropServices;

namespace CURD_Practice.Filters.ResourceFilters
{
    public class FeatureDisableResourceFilter : IAsyncResourceFilter
    {

        private readonly ILogger<FeatureDisableResourceFilter> _logger;
        private readonly bool _isDisable;

        public FeatureDisableResourceFilter(ILogger<FeatureDisableResourceFilter> logger, bool isDisable = true)
        {
            _logger = logger;
            _isDisable = isDisable;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            //Before Logic
            _logger.LogInformation("{ClassName}.{MethodName} Before", nameof(FeatureDisableResourceFilter), nameof(OnResourceExecutionAsync));

            if(_isDisable)
                 context.Result = new StatusCodeResult(501); //501 Not Implementred
            else
                await next();

            //After Logic 
            _logger.LogInformation("{ClassName}.{MethodName} After", nameof(FeatureDisableResourceFilter), nameof(OnResourceExecutionAsync));
        }
    }
}
