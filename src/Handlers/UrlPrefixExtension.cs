// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;

namespace Ngsa.Middleware
{
    /// <summary>
    /// Register URL prefix middleware
    /// </summary>
    public static class UrlPrefixExtension
    {
        private static string prefix = null;

        /// <summary>
        /// aspnet middleware extension method to update swagger.json
        ///
        /// This has to be before UseSwaggerUI() in startup
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <param name="urlPrefix">URL prefix</param>
        /// <returns>ApplicationBuilder</returns>
        public static IApplicationBuilder UseUrlPrefix(this IApplicationBuilder builder, string urlPrefix)
        {
            // cache the prefix
            prefix = string.IsNullOrEmpty(urlPrefix) ? string.Empty : urlPrefix.Trim();

            // only add the handler if url prefix is set
            if (!string.IsNullOrEmpty(prefix))
            {
                // implement the middleware
                builder.Use(async (context, next) =>
                {
                    // reject anything that doesn't start with /urlPrefix
                    if (!context.Request.Path.Value.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }

                    await next().ConfigureAwait(false);
                });
            }

            return builder;
        }
    }
}
