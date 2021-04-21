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
        // cached response
        private static byte[] responseBytes;

        private static string matchEndsWith;

        /// <summary>
        /// aspnet middleware extension method to update swagger.json
        ///
        /// This has to be before UseSwaggerUI() in startup
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <param name="jsonPath">swagger.json path</param>
        /// <param name="urlPrefix">URL prefix</param>
        /// <returns>ApplicationBuilder</returns>
        public static IApplicationBuilder UseSwaggerReplaceJson(this IApplicationBuilder builder, string jsonPath, string urlPrefix)
        {
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException(jsonPath);
            }

            string json = File.ReadAllText(jsonPath).Replace("{urlPrefix}", urlPrefix);
            responseBytes = Encoding.UTF8.GetBytes(json);

            FileInfo fi = new FileInfo(jsonPath);
            matchEndsWith = fi.Name.ToLowerInvariant();

            // implement the middleware
            builder.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value.ToLowerInvariant();

                if (path.EndsWith(matchEndsWith))
                {
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
