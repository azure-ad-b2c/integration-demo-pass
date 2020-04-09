﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using B2C.PASSDemo.Authentication;
using B2C.PASSDemo.Middleware;

namespace B2C.PASSDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            if (Environment.IsDevelopment())
            {
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                    .AddRazorRuntimeCompilation();
            }
            else
            {
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            }

            services.AddCors(options =>
            {
                options.AddPolicy(AuthConfig.AuthTemplateCorsPolicyName, builder =>
                {
                    builder
                        .WithOrigins(Configuration.GetValue<string>(AuthConfig.AuthCorsOriginSettingName))
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var auth = services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie();

            foreach (var authPolicy in Configuration.GetAuthenticationConfigurationSections())
            {
                auth.AddOpenIdConnect(authPolicy.Key, options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    authPolicy.Bind(options);
                    options.SaveTokens = true; // Save the ID token for the sign out request (it is added automatically)
                    options.Events = new OpenIdConnectEvents
                    {
                        OnAccessDenied = context =>
                        {
                            if (context.Request.ContainsOidcErrorDescriptionContent(AuthConfig.ErrorCodes.CancelledByUser) ||
                                context.Request.ContainsOidcErrorDescriptionContent(AuthConfig.ErrorCodes.ForgotPassword))
                            {
                                context.Response.Redirect("/");
                                context.HandleResponse();
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)//, IWebHostEnvironment env)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors(AuthConfig.AuthTemplateCorsPolicyName);
            app.UseAbsoluteUrlRewriting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

        }
    }
}
