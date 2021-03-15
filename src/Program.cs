// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngsa.Application.DataAccessLayer;
using Ngsa.Middleware;

namespace Ngsa.Application
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        // ILogger instance
        private static readonly NgsaLog Logger = new NgsaLog { Name = typeof(App).FullName };

        // web host
        private static IWebHost host;

        // Key Vault configuration
        private static IConfigurationRoot config;

        private static CancellationTokenSource ctCancel;

        public static InMemoryDal CacheDal { get; set; }
        public static InMemoryDal SearchService => CacheDal;

        public static IDAL CosmosDal { get; set; }

        public static string CosmosName { get; set; } = string.Empty;

        public static Config Config { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether LogLevel is set in command line or env var
        /// </summary>
        public static bool IsLogLevelSet { get; set; }

        /// <summary>
        /// Gets or sets the secrets from k8s volume
        /// </summary>
        public static Secrets Secrets { get; set; }

        /// <summary>
        /// Main entry point
        ///
        /// Configure and run the web server
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>IActionResult</returns>
        public static async Task<int> Main(string[] args)
        {
            if (args != null)
            {
                DisplayAsciiArt(new List<string>(args));
            }

            // build the System.CommandLine.RootCommand
            RootCommand root = BuildRootCommand();
            root.Handler = CommandHandler.Create<Config>(RunApp);

            // run the app
            return await root.InvokeAsync(args).ConfigureAwait(false);
        }

        private static void DisplayAsciiArt(List<string> cmd)
        {
            if (!cmd.Contains("--version") &&
                (cmd.Contains("-h") ||
                cmd.Contains("--help") ||
                cmd.Contains("-d") ||
                cmd.Contains("--dry-run")))
            {
                const string file = "src/Core/ascii-art.txt";

                try
                {
                    if (File.Exists(file))
                    {
                        string txt = File.ReadAllText(file);

                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine(txt);
                        Console.ResetColor();
                    }
                }
                catch
                {
                    // ignore any errors
                }
            }
        }

        /// <summary>
        /// Creates a CancellationTokenSource that cancels on ctl-c or sigterm
        /// </summary>
        /// <returns>CancellationTokenSource</returns>
        private static CancellationTokenSource SetupCtlCHandler()
        {
            CancellationTokenSource ctCancel = new CancellationTokenSource();

            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                ctCancel.Cancel();

                Logger.LogInformation("CtlCHandler", "Ctl-C Pressed");

                // trigger graceful shutdown for the webhost
                // force shutdown after timeout, defined in UseShutdownTimeout within BuildHost() method
                await host.StopAsync().ConfigureAwait(false);

                // end the app
                Environment.Exit(0);
            };

            return ctCancel;
        }

        /// <summary>
        /// Log startup messages
        /// </summary>
        private static void LogStartup()
        {
            if (Logger != null)
            {
                Logger.LogInformation("Data Service Started", VersionExtension.Version);
            }
        }

        /// <summary>
        /// Builds the config for the web server
        /// </summary>
        /// <returns>Root Configuration</returns>
        private static IConfigurationRoot BuildConfig()
        {
            try
            {
                // standard config builder
                IConfigurationBuilder cfgBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true);

                // build the config
                return cfgBuilder.Build();
            }
            catch (Exception ex)
            {
                // log and fail
                Logger.LogError(nameof(BuildConfig), "Exception", ex: ex);

                Environment.Exit(-1);
            }

            return null;
        }

        /// <summary>
        /// Build the web host
        /// </summary>
        /// <param name="useInMemory">Use in memory DB flag</param>
        /// <returns>Web Host ready to run</returns>
        private static IWebHost BuildHost()
        {
            // build the config
            config = BuildConfig();

            // configure the web host builder
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .UseConfiguration(config)
                .UseUrls(string.Format(System.Globalization.CultureInfo.InvariantCulture, $"http://*:{Config.Port}/"))
                .UseStartup<Startup>()
                .UseShutdownTimeout(TimeSpan.FromSeconds(Constants.GracefulShutdownTimeout))
                .ConfigureServices(services =>
                {
                    // add IConfigurationRoot
                    services.AddSingleton<IConfigurationRoot>(config);
                });

            // configure logger based on command line
            builder.ConfigureLogging(logger =>
            {
                logger.ClearProviders();
                logger.AddNgsaLogger(config => { config.LogLevel = Config.LogLevel; });

                // if you specify the --log-level option, it will override the appsettings.json options
                // remove any or all of the code below that you don't want to override
                if (App.IsLogLevelSet)
                {
                    logger.AddFilter("Microsoft", Config.LogLevel)
                    .AddFilter("System", Config.LogLevel)
                    .AddFilter("Default", Config.LogLevel)
                    .AddFilter("Ngsa.Application", Config.LogLevel);
                }
            });

            // build the host
            return builder.Build();
        }
    }
}
