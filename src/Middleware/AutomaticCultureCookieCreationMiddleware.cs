using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace B2C.PASSDemo.Middleware
{
    public static class AutomaticCultureCookieCreationMiddleware
    {
        public static IApplicationBuilder UseAutomaticCultureCookieCreation(this IApplicationBuilder app)
        {
            return app.UseWhen(IsCultureExplicitlySet, a => a.Use(WriteCultureCookie));
        }

        private static bool IsCultureExplicitlySet(HttpContext context) {
            var provider = context.Features.Get<IRequestCultureFeature>()?.Provider;
            return provider != null && !(provider is CookieRequestCultureProvider);
        }

        private static async Task WriteCultureCookie(HttpContext context, Func<Task> next)
        {
            var culture = context.Features.Get<IRequestCultureFeature>().RequestCulture;
            context.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(culture),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            await next();
        }
    }
}