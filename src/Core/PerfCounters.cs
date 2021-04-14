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
        private static readonly PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
        private static readonly object Lock = new object();
        private static DateTime lastRefresh = DateTime.UtcNow;
        private static float cpu = CpuCounter.NextValue();

        /// <summary>
        /// Get current CPU usage
        /// </summary>
        /// <returns>double</returns>
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
                        cpu = CpuCounter.NextValue();
                        lastRefresh = DateTime.UtcNow;
                    }
                }
            }

            return Math.Round(cpu, 2);
        }
    }
}
