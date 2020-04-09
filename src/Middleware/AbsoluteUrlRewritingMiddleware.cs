using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace B2C.PASSDemo.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class EnableAbsoluteUrlRewritingAttribute : Attribute { }

    public static class AbsoluteUrlRewritingMiddleware
    {
        public static IApplicationBuilder UseAbsoluteUrlRewriting(this IApplicationBuilder app)
        {
            app.UseWhen(context => {
                var metadata =  context.GetEndpoint()?.Metadata
                    .GetMetadata<EnableAbsoluteUrlRewritingAttribute>();
                return metadata != null;
            }, a => a.Use(ConvertToAbsoluteUrls));
            return app;
        }

        private static async Task ConvertToAbsoluteUrls(HttpContext context, Func<Task> next)
        {
            var urlAttributePattern = new Regex("(src|href)(\\s*=\\s*)('|\")(?!http)\\/?", RegexOptions.IgnoreCase);
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.PathBase}";

            var originalBodyStream = context.Response.Body;
            using (var newBodyStream = new MemoryStream())
            {
                context.Response.Body = newBodyStream;

                await next();

                newBodyStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(newBodyStream))
                {
                    var bodyContent = await reader.ReadToEndAsync();
                    bodyContent = urlAttributePattern.Replace(bodyContent, $"$1$2$3{baseUrl}");

                    context.Response.Body = originalBodyStream;
                    await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(bodyContent));
                }
            }
        }
    }
}