// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Timers;

namespace Ngsa.Application
{
    /// <summary>
    /// Encapsulates dotnet PerformanceCounters
    /// </summary>
    public class PerfCounters
    {
        public const string CapacityHeader = "X-Capacity-Metric";

        private static readonly Process Proc = Process.GetCurrentProcess();
        private static DateTime lastRefresh = DateTime.UtcNow;
        private static TimeSpan lastCpu = Proc.TotalProcessorTime;
        private static double cpu = 0;
        private static Timer timer = null;

        /// <summary>
        /// Gets current CPU usage
        /// </summary>
        /// <returns>double</returns>
        public static double CpuPercent
        {
            get
            {
                return cpu;
            }
        }

        public static void Start()
        {
            if (timer != null)
            {
                Stop();
            }

            lastRefresh = DateTime.UtcNow;
            lastCpu = Proc.TotalProcessorTime;

            timer = new Timer(1000);
            timer.Elapsed += TimerEvent;
            timer.Start();
        }

        public static void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
                cpu = 0;
            }
        }

        private static void TimerEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan nowCpu = Proc.TotalProcessorTime;

            cpu = Math.Round(nowCpu.Subtract(lastCpu).TotalMilliseconds / (Environment.ProcessorCount * now.Subtract(lastRefresh).TotalMilliseconds) * 100, 4);

            lastCpu = nowCpu;
            lastRefresh = now;
        }
    }
}
