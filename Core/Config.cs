// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

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
        public string DataService { get; set; } = string.Empty;
        public string SecretsVolume { get; set; } = "secrets";
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public bool DryRun { get; set; }
        public bool InMemory { get; set; }
        public bool Cache => !NoCache;
        public bool NoCache { get; set; }
        public int PerfCache { get; set; }
        public int CacheDuration { get; set; } = 300;
        public string Zone { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int Port { get; set; } = 8080;
    }
}
