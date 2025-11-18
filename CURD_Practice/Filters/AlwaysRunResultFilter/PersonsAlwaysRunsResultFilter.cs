using Microsoft.AspNetCore.Mvc.Filters;

namespace CURD_Practice.Filters.AlwaysRunResultFilter
{
    public class PersonsAlwaysRunsResultFilter : IAsyncAlwaysRunResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if(context.Filters.OfType<SkipFilter>().Any())
                return;

            await next();
        }
    }
}
