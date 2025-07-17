using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace TenentManagement.Services.Utilities
{
    public class BlockDirectAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Get the Referer header
            context.HttpContext.Request.Headers.TryGetValue("Referer", out StringValues referer);

            var request = context.HttpContext.Request;
            var host = request.Host.ToString();

            // If no Referer OR it's from an external site → block
            if (StringValues.IsNullOrEmpty(referer) || !referer.ToString().Contains(host))
            {
                context.Result = new ContentResult
                {
                    Content = "🚫 Direct access to this page is not allowed.",
                    StatusCode = 403
                };
            }

            base.OnActionExecuting(context);
        }
    }
}
