// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using Ngsa.Application.DataAccessLayer;

namespace Ngsa.Application
{
    public enum AppType
    {
        App,
        WebAPI,
    }

    public class Config
    {
        public AppType AppType { get; set; } = AppType.App;
        public string DataService { get; set; } = string.Empty;
        public string SecretsVolume { get; set; } = "secrets";
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public string CosmosName { get; set; } = string.Empty;
        public bool IsLogLevelSet { get; set; }
        public Secrets Secrets { get; set; }
        public bool DryRun { get; set; }
        public bool InMemory { get; set; }
        public bool Cache => !NoCache;
        public bool NoCache { get; set; }
        public int PerfCache { get; set; }
        public int CacheDuration { get; set; } = 300;
        public string Zone { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int Port { get; set; } = 8080;
        public int Retries { get; set; }
        public int Timeout { get; set; }
        public bool Prometheus { get; set; }
        public LogLevel RequestLogLevel { get; set; } = LogLevel.Information;
        public InMemoryDal CacheDal { get; set; }
        public IDAL CosmosDal { get; set; }
    }
}
