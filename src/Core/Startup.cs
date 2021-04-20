// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ngsa.Middleware;
using Prometheus;

namespace Ngsa.Application
{
    /// <summary>
    /// WebHostBuilder Startup
    /// </summary>
    public class Startup
    {
        private const string SwaggerPath = "swagger.json";
        private const string SwaggerTitle = "Next Gen Symmetric Apps";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">the configuration for WebHost</param>
        public Startup(IConfiguration configuration)
        {
            // keep a local reference
            Configuration = configuration;
        }

        /// <summary>
        /// Gets IConfiguration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure the application builder
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <param name="env">IWebHostEnvironment</param>
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            // log http responses to the console
            // this should be first as it "wraps" all requests
            if (App.Config.LogLevel != LogLevel.None)
            {
                app.UseRequestLogger(new RequestLoggerOptions
                {
                    Log2xx = App.Config.LogLevel <= LogLevel.Information,
                    Log3xx = App.Config.LogLevel <= LogLevel.Information,
                    Log4xx = App.Config.LogLevel <= LogLevel.Warning,
                    Log5xx = true,
                });
            }

            // UseHsts in prod
            if (env.IsProduction())
            {
                app.UseHsts();
            }

            // fix links in swagger.json
            app.Use(async (context, next) =>
            {
                if (!string.IsNullOrEmpty(App.Config.UrlPrefix))
                {
                    var path = context.Request.Path.Value.ToLowerInvariant();

                    if (path.StartsWith(App.Config.UrlPrefix) &&
                        path.EndsWith("swagger.json"))
                    {
                        string json = File.ReadAllText("src/wwwroot/swagger.json");

                        json = json.Replace("/api/", $"{App.Config.UrlPrefix}/api/")
                        .Replace("/healthz", $"{App.Config.UrlPrefix}/healthz");

                        context.Response.ContentType = "application/json";
                        await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(json)).ConfigureAwait(false);

                        return;
                    }
                }

                await next().ConfigureAwait(false);
            });

            // handle url prefix
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value.ToLowerInvariant();

                if (!string.IsNullOrEmpty(App.Config.UrlPrefix))
                {
                    // reject anything that doesn't start with /urlPrefix
                    if (!path.StartsWith(App.Config.UrlPrefix.ToLowerInvariant()))
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }

                    // todo - use custom route mappings instead
                    if (path.StartsWith(App.Config.UrlPrefix + "/api") ||
                        path.StartsWith(App.Config.UrlPrefix + "/metrics") ||
                        path.StartsWith(App.Config.UrlPrefix + "/healthz"))
                    {
                        context.Request.Path = context.Request.Path.ToString()[App.Config.UrlPrefix.Length..];
                    }
                }

                await next();
            });

            // add middleware handlers
            app.UseRouting()
                .UseEndpoints(ep =>
                {
                    ep.MapControllers();

                    if (App.Config.Prometheus)
                    {
                        ep.MapMetrics();
                    }
                })
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(SwaggerPath, SwaggerTitle);
                    c.RoutePrefix = string.Empty;

                    if (!string.IsNullOrEmpty(App.Config.UrlPrefix))
                    {
                        c.RoutePrefix = App.Config.UrlPrefix[1..];
                    }
                })
                .UseVersion(App.Config.UrlPrefix)
                .UseRobots(App.Config.UrlPrefix)
                .UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "src/wwwroot")),
                });
        }

        /// <summary>
        /// Service configuration
        /// </summary>
        /// <param name="services">The services in the web host</param>
        public static void ConfigureServices(IServiceCollection services)
        {
            // set json serialization defaults and api behavior
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
        }
    }
}
