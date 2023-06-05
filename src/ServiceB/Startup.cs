using System;
using System.Text.Json;
using Dapr.Workflow;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceB.Services;
using ServiceB.Workflows.Common;
using ServiceB.Workflows.Test;

namespace ServiceB
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.configuration = configuration;
            this.environment = env;

            Console.WriteLine($"================== ServiceB @ {DateTime.UtcNow} ==================");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            services.AddScoped<IWorkflowSchedulerService, WorkflowSchedulerService>();

            services.AddCors();

            services.AddHealthChecks();

            // Add Controllers along with Dapr
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = jsonOptions.PropertyNameCaseInsensitive;
                    foreach (var converter in jsonOptions.Converters)
                    {
                        options.JsonSerializerOptions.Converters.Add(converter);              
                    }
                })
                .AddDapr(builder =>
                {
                    builder.UseJsonSerializationOptions(jsonOptions);
                });

            services.AddDaprWorkflow(options =>
            {
                // Common
                options.RegisterActivity<DelayActivity>();
                options.RegisterActivity<LoggerActivity>();

                // Test Workflow
                options.RegisterWorkflow<TestWorkflow>();                
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (this.environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(o => o
               .AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed((host) => true)
               .AllowCredentials());

            app.UseCloudEvents();

            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapSubscribeHandler();

                endpoints.MapHealthChecks("/healthz");

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync($"Hello from Services.Core.Tags! [{DateTime.UtcNow}]").ConfigureAwait(false);
                });
            });
        }
    }
}
