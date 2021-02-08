// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ngsa.DataService;
using Ngsa.Middleware.Validation;
using Prometheus;

namespace Ngsa.Middleware
{
    /// <summary>
    /// Simple aspnet core middleware that logs requests to the console
    /// </summary>
    public class RequestLogger
    {
        private const string IpHeader = "X-Client-IP";

        private static readonly List<int> RPS = new List<int>();
        private static Histogram requestDuration = null;
        private static int counter;

        // next action to Invoke
        private readonly RequestDelegate next;
        private readonly RequestLoggerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLogger"/> class.
        /// </summary>
        /// <param name="next">RequestDelegate</param>
        /// <param name="options">LoggerOptions</param>
        public RequestLogger(RequestDelegate next, IOptions<RequestLoggerOptions> options)
        {
            // save for later
            this.next = next;
            this.options = options?.Value;

            if (this.options == null)
            {
                // use default
                this.options = new RequestLoggerOptions();
            }

            if (App.Config.Prometheus)
            {
                requestDuration = Metrics.CreateHistogram(
                            "NgsaAppDuration",
                            "Histogram of NGSA App request duration",
                            new HistogramConfiguration
                            {
                                Buckets = Histogram.ExponentialBuckets(1, 2, 10),
                                LabelNames = new string[] { "code", "category", "subcategory", "mode", "zone", "region" },
                            });
            }
        }

        public static string DataService { get; set; } = string.Empty;
        public static string CosmosName { get; set; } = string.Empty;
        public static string CosmosQueryId { get; set; } = string.Empty;
        public static double CosmosRUs { get; set; } = 0;
        public static string Zone { get; set; } = string.Empty;
        public static string Region { get; set; } = string.Empty;

        public static int RequestsPerSecond => RPS.Count > 0 ? RPS[0] : counter;

        /// <summary>
        /// Return the path and query string if it exists
        /// todo move to utility class
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>string</returns>
        public static string GetPathAndQuerystring(HttpRequest request)
        {
            if (request == null || !request.Path.HasValue)
            {
                return string.Empty;
            }

            return request.Path.Value + (request.QueryString.HasValue ? request.QueryString.Value : string.Empty);
        }

        /// <summary>
        /// Start a timer that summarizes the requests every period
        /// </summary>
        /// <param name="delay">initial delay (ms - min 5000)</param>
        /// <param name="period">delay per run (ms - min 1000)</param>
        public static void StartCounterTime(int delay, int period)
        {
            delay = delay < 5000 ? 5000 : delay;
            period = period < 1000 ? 1000 : period;

            _ = new Timer(RollCounter, null, delay - DateTime.UtcNow.Millisecond, period);
        }

        /// <summary>
        /// Called by aspnet pipeline
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>Task (void)</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                return;
            }

            CorrelationVector cv;
            DateTime dtStart = DateTime.Now;
            double duration = 0;
            double ttfb = 0;

            cv = CorrelationVectorExtensions.Extend(context);

            // Invoke next handler
            if (next != null)
            {
                await next.Invoke(context).ConfigureAwait(false);
            }

            // compute request duration
            duration = Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds, 2);
            ttfb = ttfb == 0 ? duration : ttfb;

            // don't log favicon.ico 404s
            if (context.Request.Path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            LogRequest(context, cv, ttfb, duration);
        }

        // roll request counter into list
        private static void RollCounter(object state)
        {
            RPS.Insert(0, Interlocked.Exchange(ref counter, 0));

            while (RPS.Count > 600)
            {
                RPS.RemoveAt(RPS.Count - 1);
            }
        }

        // log the request
        private static void LogRequest(HttpContext context, CorrelationVector cv, double ttfb, double duration)
        {
            DateTime dt = DateTime.UtcNow;

            string category = ValidationError.GetCategory(context, out string subCategory, out string mode);

            if (App.Config.RequestLogLevel != LogLevel.None &&
                (App.Config.RequestLogLevel <= LogLevel.Information ||
                (App.Config.RequestLogLevel == LogLevel.Warning && context.Response.StatusCode >= 400) ||
                context.Response.StatusCode >= 500))
            {
                Dictionary<string, object> log = new Dictionary<string, object>
                {
                    { "Date", dt },
                    { "LogName", "Ngsa.RequestLog" },
                    { "StatusCode", context.Response.StatusCode },
                    { "TTFB", ttfb },
                    { "Duration", duration },
                    { "Verb", context.Request.Method },
                    { "Path", GetPathAndQuerystring(context.Request) },
                    { "Host", context.Request.Headers["Host"].ToString() },
                    { "ClientIP", GetClientIp(context) },
                    { "UserAgent", context.Request.Headers["User-Agent"].ToString() },
                    { "CVector", cv.Value },
                    { "CVectorBase", cv.GetBase() },
                    { "Category", category },
                    { "Subcategory", subCategory },
                    { "Mode", mode },
                };

                if (!string.IsNullOrWhiteSpace(Zone))
                {
                    log.Add("Zone", Zone);
                }

                if (!string.IsNullOrWhiteSpace(Region))
                {
                    log.Add("Region", Region);
                }

                if (!string.IsNullOrWhiteSpace(CosmosName))
                {
                    log.Add("CosmosName", CosmosName);
                }

                if (!string.IsNullOrWhiteSpace(CosmosQueryId))
                {
                    log.Add("CosmosQueryId", CosmosQueryId);
                }

                if (CosmosRUs > 0)
                {
                    log.Add("CosmosRUs", CosmosRUs);
                }

                if (!string.IsNullOrWhiteSpace(DataService))
                {
                    log.Add("DataService", DataService);
                }

                // write the results to the console
                Console.WriteLine(JsonSerializer.Serialize(log));
            }

            Interlocked.Increment(ref counter);

            if (App.Config.Prometheus && requestDuration != null)
            {
                requestDuration.WithLabels(context.Response.StatusCode.ToString(), category, subCategory, mode, App.Config.Zone, App.Config.Region).Observe(duration);
            }
        }

        // get the client IP address from the request / headers
        // todo move to utility class
        private static string GetClientIp(HttpContext context)
        {
            string clientIp = context.Connection.RemoteIpAddress.ToString();

            // check for the forwarded header
            if (context.Request.Headers.ContainsKey(IpHeader))
            {
                clientIp = context.Request.Headers[IpHeader].ToString();
            }

            // remove IP6 local address
            return clientIp.Replace("::ffff:", string.Empty);
        }
    }
}
