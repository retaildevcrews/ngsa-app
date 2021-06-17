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
        public bool Prometheus { get; set; }
        public int CacheDuration { get; set; } = 300;
        public string Zone { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int Port { get; set; } = 8080;
        public int Retries { get; set; } = 10;
        public int Timeout { get; set; } = 10;
        public LogLevel RequestLogLevel { get; set; } = LogLevel.Information;
        public InMemoryDal CacheDal { get; set; }
        public IDAL CosmosDal { get; set; }
        public bool BurstHeader { get; set; }
        public string BurstService { get; set; }
        public int BurstTarget { get; set; }
        public int BurstMax { get; set; }
        public string UrlPrefix { get; set; }

        public void SetConfig(Config config)
        {
            AppType = config.AppType;
            IsLogLevelSet = config.IsLogLevelSet;
            DryRun = config.DryRun;
            InMemory = config.InMemory;
            NoCache = config.NoCache;
            CacheDuration = config.CacheDuration;
            Secrets = config.Secrets;
            Port = config.Port;
            Retries = config.Retries;
            Timeout = config.Timeout;
            Prometheus = config.Prometheus;
            CacheDal = config.CacheDal;
            CosmosDal = config.CosmosDal;
            BurstHeader = config.BurstHeader;
            BurstService = config.BurstService;
            BurstMax = config.BurstMax;
            BurstTarget = config.BurstTarget;
            UrlPrefix = string.IsNullOrWhiteSpace(config.UrlPrefix) ? string.Empty : config.UrlPrefix;

            // remove trailing / if present
            if (UrlPrefix.EndsWith('/'))
            {
                UrlPrefix = UrlPrefix[0..^1];
            }

            // LogLevel.Information is the min
            LogLevel = config.LogLevel <= LogLevel.Information ? LogLevel.Information : config.LogLevel;
            RequestLogLevel = config.RequestLogLevel <= LogLevel.Information ? LogLevel.Information : config.RequestLogLevel;

            // clean up string values
            DataService = config.DataService == "n/a" ? string.Empty : config.DataService;
            SecretsVolume = string.IsNullOrWhiteSpace(config.SecretsVolume) ? string.Empty : config.SecretsVolume.Trim();
            CosmosName = string.IsNullOrWhiteSpace(config.CosmosName) ? string.Empty : config.CosmosName.Trim();
            Zone = string.IsNullOrWhiteSpace(config.Zone) ? string.Empty : config.Zone.Trim();
            Region = string.IsNullOrWhiteSpace(config.Region) ? string.Empty : config.Region.Trim();
        }
    }
}
