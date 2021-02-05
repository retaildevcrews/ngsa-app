// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Ngsa.Middleware;

namespace Ngsa.DataService
{
    public enum AppType
    {
        DataService,
        WebAPI,
    }

    public class Config
    {
        public AppType AppType { get; set; } = AppType.DataService;
        public string SecretsVolume { get; set; } = "secrets";
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public bool DryRun { get; set; }
        public bool InMemory { get; set; }
        public bool NoCache { get; set; }
        public int PerfCache { get; set; }
        public int CacheDuration { get; set; } = 300;
        public string Zone { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
