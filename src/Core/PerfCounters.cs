// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;

namespace Ngsa.Application
{
    /// <summary>
    /// Encapsulates dotnet PerformanceCounters
    /// </summary>
    public class PerfCounters
    {
        public const string CapacityHeader = "X-Capacity-Metric";

        private static readonly Process Proc = Process.GetCurrentProcess();
        private static readonly object Lock = new object();
        private static DateTime lastRefresh = DateTime.UtcNow;
        private static TimeSpan lastCpu = Proc.TotalProcessorTime;
        private static double cpu = 0;

        /// <summary>
        /// Get current CPU usage
        /// </summary>
        /// <returns>int</returns>
        public static double GetCpu()
        {
            // wait at least 1 second before updating per docs
            if (DateTime.UtcNow.Subtract(lastRefresh).TotalMilliseconds >= 1000)
            {
                lock (Lock)
                {
                    // check if updated before lock
                    if (DateTime.UtcNow.Subtract(lastRefresh).TotalMilliseconds >= 1000)
                    {
                        DateTime now = DateTime.UtcNow;
                        TimeSpan nowCpu = Proc.TotalProcessorTime;

                        cpu = Math.Round(nowCpu.Subtract(lastCpu).TotalMilliseconds / (Environment.ProcessorCount * now.Subtract(lastRefresh).TotalMilliseconds) * 100, 4);

                        lastCpu = nowCpu;
                        lastRefresh = now;
                    }
                }
            }

            return cpu;
        }
    }
}
