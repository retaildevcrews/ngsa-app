// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Ngsa.Application;

namespace Ngsa.Middleware
{
    /// <summary>
    /// Register swagger root middleware
    /// </summary>
    public static class SwaggerExtension
    {
        /// <summary>
        /// aspnet middleware extension method to handle / request
        /// rewrite / to /index.html (the swagger UI)
        ///
        /// This has to be before UseSwaggerUI() in startup
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <returns>ApplicationBuilder</returns>
        public static IApplicationBuilder UseSwaggerRoot(this IApplicationBuilder builder)
        {
            // implement the middleware
            builder.Use(async (context, next) =>
            {
                // rewrite / path
                if (context.Request.Path.Value == "/")
                {
                    string path = "/index.html";

                    if (!string.IsNullOrEmpty(App.Config.UrlPrefix))
                    {
                        path = App.Config.UrlPrefix + path;
                    }

                    context.Request.Path = new Microsoft.AspNetCore.Http.PathString(path);
                }

                // call next middleware handler
                await next().ConfigureAwait(false);
            });

            return builder;
        }
    }
}
