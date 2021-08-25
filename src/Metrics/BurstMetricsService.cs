// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Timers;
using Microsoft.AspNetCore.Http;
using Ngsa.Middleware;

/// <summary>
/// This service consumes burst metrics from an external service and will also
/// inject these metrics into a response header.
/// </summary>
namespace Ngsa.Application.Metrics
{
    public class BurstMetricsService : IDisposable, IBurstMetricsService
    {
        private const int ClientTimeout = 5;
        private const int MetricsRefreshFrequency = 5;
        private const string CapacityHeader = "X-Load-Feedback";
        private const string EnvBurstEndpoint = "BURST_SERVICE_ENDPOINT";
        private const string EnvNamespace = "BURST_SERVICE_NS";
        private const string EnvHPA = "BURST_SERVICE_HPA";
        private readonly NgsaLog logger;
        private HttpClient client;
        private System.Timers.Timer timer;
        private string burstMetricsPath;
        private string burstMetricsResult;
        public CancellationToken Token { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BurstMetricsService"/> class.
        /// </summary>
        public BurstMetricsService()
        {
            logger = new NgsaLog { Name = typeof(BurstMetricsService).FullName };
            burstMetricsResult = string.Empty;

            // read burst service endpoint from env vars and setup http client
            SetupHTTPClient();

            // run timed burst metrics service
            StartTimer();
        }

        /// <summary>
        /// Return burst metrics
        /// </summary>
        public string GetBurstMetrics()
        {
            return burstMetricsResult;
        }

        /// <summary>
        /// Inject burst metrics, if present, into the response header
        /// </summary>
        /// <param name="context">response context</param>
        public void InjectBurstMetricsHeader(HttpContext context)
        {
            if (!string.IsNullOrEmpty(burstMetricsResult))
            {
                context.Response.Headers.Add(CapacityHeader, burstMetricsResult);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    client.Dispose();
                }
            }
        }

        private void SetupHTTPClient()
        {
            // read burst service endpoint env variables
            string baseAddress = Environment.GetEnvironmentVariable(EnvBurstEndpoint);
            string ns = Environment.GetEnvironmentVariable(EnvNamespace);
            string hpa = Environment.GetEnvironmentVariable(EnvHPA);
            burstMetricsPath = $"{ns}/{hpa}";

            // verify burst service endpoint
            if (!Uri.IsWellFormedUriString($"{baseAddress}/{burstMetricsPath}", UriKind.Absolute))
            {
                throw new Exception("Burst metrics service endpoint is not a valid URI. Ensure you have set burst env vars.");
            }

            try
            {
                // Create a http client
                client = new ()
                {
                    Timeout = new TimeSpan(0, 0, ClientTimeout),
                    BaseAddress = new Uri(baseAddress),
                };
                client.DefaultRequestHeaders.Add("User-Agent", $"ngsa/{VersionExtension.ShortVersion}");
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to setup client to burst service endpoint.", ex);
            }
        }

        private void StartTimer()
        {
            timer = new ()
            {
                Enabled = true,
                Interval = MetricsRefreshFrequency * 1000,
            };
            timer.Elapsed += TimerWork;

            // Run once before the timer
            TimerWork(this, null);

            // Start the timer, it will be called after Interval
            timer.Start();
        }

        private async void TimerWork(object state, ElapsedEventArgs e)
        {
            // exit if cancelled
            if (Token.IsCancellationRequested)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
                client.Dispose();
                client = null;

                return;
            }

            // verify http client
            if (client == null)
            {
                Console.WriteLine("Error: Burst Metrics Service http client is null");
                return;
            }

            try
            {
                // process the response
                using HttpResponseMessage resp = await client.GetAsync(burstMetricsPath).ConfigureAwait(false);
                if (resp.IsSuccessStatusCode)
                {
                    burstMetricsResult = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    burstMetricsResult = string.Empty;
                    logger.LogWarning("BurstMetricsServiceTimer", "Received error status code from burst service.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError("BurstMetricsServiceTimer", "Failed to get response from burst service.", ex: ex);
            }
        }
    }
}
