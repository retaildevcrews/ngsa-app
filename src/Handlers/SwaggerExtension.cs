// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Ngsa.Middleware
{
    /// <summary>
    /// Register swagger middleware
    /// </summary>
    public static class SwaggerExtension
    {
        // cached response
        private static byte[] responseBytes;

        private static string match;

        /// <summary>
        /// aspnet middleware extension method to update swagger.json
        ///
        /// Can throw exceptions at startup
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <param name="jsonPath">swagger.json path</param>
        /// <param name="urlPrefix">URL prefix</param>
        /// <returns>ApplicationBuilder</returns>
        public static IApplicationBuilder UseSwaggerReplaceJson(this IApplicationBuilder builder, string jsonPath, string urlPrefix)
        {
            FileInfo fi = new FileInfo(jsonPath);

            if (!fi.Exists)
            {
                throw new FileNotFoundException(jsonPath);
            }

            // cache the file
            responseBytes = Encoding.UTF8.GetBytes(File.ReadAllText(jsonPath).Replace("{urlPrefix}", urlPrefix));

            // clean up urlPrefix
            if (string.IsNullOrWhiteSpace(urlPrefix))
            {
                urlPrefix = string.Empty;
            }
            else
            {
                urlPrefix = urlPrefix.Trim();

                if (!urlPrefix.StartsWith('/'))
                {
                    urlPrefix = "/" + urlPrefix;
                }

                if (!urlPrefix.EndsWith('/'))
                {
                    urlPrefix += "/";
                }
            }

            if (urlPrefix.Length < 3)
            {
                throw new ArgumentException("Invalid urlPrefix");
            }

            match = urlPrefix + fi.Name;

            // implement the middleware
            builder.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.Equals(match, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.Body.WriteAsync(responseBytes).ConfigureAwait(false);

                    return;
                }

                // call next handler
                await next().ConfigureAwait(false);
            });

            return builder;
        }
    }
}
