// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;

namespace Ngsa.Application
{
    public class PerfCounters
    {
        private static readonly PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
        private static DateTime lastRefresh = DateTime.UtcNow;
        private static float cpu = CpuCounter.NextValue();

        public static double GetCpu()
        {
            if (DateTime.UtcNow.Subtract(lastRefresh).TotalMilliseconds >= 900)
            {
                cpu = CpuCounter.NextValue();
            }

            return Math.Round(cpu, 2);
        }
    }
}
