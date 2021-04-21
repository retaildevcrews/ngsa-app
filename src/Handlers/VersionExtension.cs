// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;

namespace Ngsa.Middleware
{
    /// <summary>
    /// Registers aspnet middleware handler that handles /version
    /// </summary>
    public static class VersionExtension
    {
        // cached response
        private static byte[] responseBytes;

        // cache version info as it doesn't change
        private static string version = string.Empty;

        /// <summary>
        /// Gets the app version
        /// </summary>
        public static string Version
        {
            get
            {
                // use reflection to get the version
                if (string.IsNullOrWhiteSpace(version))
                {
                    if (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute v)
                    {
                        version = v.InformationalVersion;
                    }
                }

                return version;
            }
        }

        /// <summary>
        /// Middleware extension method to handle /version request
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <param name="urlPrefix">URL prefix</param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder UseVersion(this IApplicationBuilder builder, string urlPrefix)
        {
            // implement the middleware
            builder.Use(async (context, next) =>
            {
                const string swaggerFile = "swagger.json";

                string path = "/version";

                if (!string.IsNullOrWhiteSpace(urlPrefix))
                {
                    path = urlPrefix + path;
                }

                // matches /version
                if (context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase))
                {
                    // cache the version info for performance
                    if (responseBytes == null)
                    {
                        // default to 1.0
                        string swaggerVersion = "1.0";

                        try
                        {
                            if (File.Exists(swaggerFile))
                            {
                                // read swagger version from swagger.json
                                using JsonDocument sw = JsonDocument.Parse(File.ReadAllText(swaggerFile));
                                swaggerVersion = sw.RootElement.GetProperty("info").GetProperty("version").ToString();
                            }
                        }
                        catch
                        {
                            // ignore
                        }

                        // build and cache the json string
                        Dictionary<string, string> dict = new Dictionary<string, string>
                        {
                            { "apiVersion", swaggerVersion },
                            { "appVersion", Version },
                            { "language", "C#" },
                        };

                        responseBytes = JsonSerializer.SerializeToUtf8Bytes(dict);
                    }

                    // return the version info
                    context.Response.ContentType = "application/json";
                    await context.Response.Body.WriteAsync(responseBytes).ConfigureAwait(false);
                }
                else
                {
                    // not a match, so call next middleware handler
                    await next().ConfigureAwait(false);
                }
            });

            return builder;
        }
    }
}
