using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using SKA.Controllers;

namespace SKA
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            services.AddControllersWithViews();
            services.AddHttpClient<IGeoIpService, GeoIpService>()
                .AddTransientHttpErrorPolicy(GetRetryPolicy);
            services.AddTransient<IGeoIpService, GeoIpService>();
            services.AddTransient<LogFilter>();
            services.AddSwaggerGen(c => { 
                c.SwaggerDoc("v3", new OpenApiInfo
                {
                    Title = "GTrackAPI",
                    Version = "v3"
                });
            });
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(PolicyBuilder<HttpResponseMessage> arg)
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .OrResult(msg =>
                    msg.StatusCode == HttpStatusCode.GatewayTimeout || msg.StatusCode == HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromMilliseconds(100));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseResponseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v3/swagger.json", "My Api V1");
            });

        }
    }
}
