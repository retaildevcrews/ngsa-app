// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Ngsa.Application.Metrics
{
    /// <summary>
    /// Burst Metrics Service Interface
    /// </summary>
    public interface IBurstMetricsService
    {
        string GetBurstMetrics();
        void InjectBurstMetricsHeader(HttpContext context);
    }
}
