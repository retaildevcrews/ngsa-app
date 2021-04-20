// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Routing;

namespace Ngsa.Application
{
    /// <summary>
    /// Add the --url-prefix to the routes if specified
    /// </summary>
    public class UrlPrefixTransformer : IOutboundParameterTransformer
    {
        private readonly string prefix = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlPrefixTransformer"/> class.
        /// </summary>
        /// <param name="urlPrefix">urlPrefix to add</param>
        public UrlPrefixTransformer(string urlPrefix)
        {
            if (!string.IsNullOrWhiteSpace(urlPrefix))
            {
                prefix = urlPrefix.Trim()[1..] + "/";
            }
        }

        /// <summary>
        /// Transform the controller
        /// </summary>
        /// <param name="value">controller</param>
        /// <returns>string</returns>
        public string TransformOutbound(object value)
        {
            if (value == null)
            {
                return null;
            }

            string path = value.ToString();

            // don't add api/ for healthz
            if (path.ToLowerInvariant() != "healthz")
            {
                path = "api/" + path;
            }

            // add the prefix
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                path = prefix + path;
            }

            return path;
        }
    }
}
