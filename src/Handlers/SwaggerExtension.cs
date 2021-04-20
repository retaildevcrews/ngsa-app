// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Ngsa.Application;

namespace Ngsa.Middleware
{
    /// <summary>
    /// Register swagger middleware
    /// </summary>
    public static class SwaggerExtension
    {
        private const string MatchEndsWith = "swagger.json";

        // cached response
        private static byte[] responseBytes;

        /// <summary>
        /// aspnet middleware extension method to update swagger.json
        ///
        /// This has to be before UseSwaggerUI() in startup
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <param name="jsonPath">swagger.json path</param>
        /// <returns>ApplicationBuilder</returns>
        public static IApplicationBuilder UseSwaggerReplaceJson(this IApplicationBuilder builder, string jsonPath)
        {
            // implement the middleware
            builder.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value.ToLowerInvariant();

                if (path.EndsWith(MatchEndsWith))
                {
                    App.Config.UrlPrefix = string.IsNullOrWhiteSpace(App.Config.UrlPrefix) ? string.Empty : App.Config.UrlPrefix;

                    // cache the response
                    if (responseBytes == null)
                    {
                        string json = File.ReadAllText(jsonPath).Replace("{urlPrefix}", App.Config.UrlPrefix);
                        responseBytes = Encoding.UTF8.GetBytes(json);
                    }

                    context.Response.ContentType = "application/json";
                    await context.Response.Body.WriteAsync(responseBytes).ConfigureAwait(false);

                    return;
                }

                await next().ConfigureAwait(false);
            });

            return builder;
        }
    }
}
