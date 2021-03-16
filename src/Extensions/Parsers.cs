// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ngsa.Application;

namespace Ngsa.Middleware.CommandLine
{
    public static class Parsers
    {
        // parse string command line arg
        public static string ParseString(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return null;
            }

            string val;

            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                val = string.IsNullOrWhiteSpace(env) ? string.Empty : env.Trim();
            }
            else
            {
                val = result.Tokens[0].Value.Trim();
            }

            if (string.IsNullOrWhiteSpace(val))
            {
                return name switch
                {
                    "DATA_SERVICE" => "http://localhost:8080",
                    "SECRETS_VOLUME" => "secrets",
                    "ZONE" => "dev",
                    "REGION" => "dev",
                    _ => null,
                };
            }
            else if (val.Length < 3)
            {
                result.ErrorMessage = $"--{result.Parent.Symbol.Name} must be at least 3 characters";
                return null;
            }
            else if (val.Length > 100)
            {
                result.ErrorMessage = $"--{result.Parent.Symbol.Name} must be 100 characters or less";
            }

            return val;
        }

        // parse List<string> command line arg (--files)
        public static List<string> ParseStringList(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');
            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return null;
            }

            List<string> val = new List<string>();

            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    result.ErrorMessage = $"--{result.Argument.Name} is a required parameter";
                    return null;
                }

                string[] files = env.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (string f in files)
                {
                    val.Add(f.Trim());
                }
            }
            else
            {
                for (int i = 0; i < result.Tokens.Count; i++)
                {
                    val.Add(result.Tokens[i].Value.Trim());
                }
            }

            return val;
        }

        // parse boolean command line arg
        public static bool ParseBool(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return false;
            }

            string errorMessage = $"--{result.Parent.Symbol.Name} must be true or false";
            bool val;

            // bool options default to true if value not specified (ie -r and -r true)
            if (result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == result.Parent.Symbol.Name) is OptionResult res &&
                !res.IsImplicit &&
                result.Tokens.Count == 0)
            {
                return true;
            }

            // nothing to validate
            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (!string.IsNullOrWhiteSpace(env))
                {
                    if (bool.TryParse(env, out val))
                    {
                        return val;
                    }
                    else
                    {
                        result.ErrorMessage = errorMessage;
                        return false;
                    }
                }

                // default to true
                if (result.Parent.Symbol.Name == "verbose-errors")
                {
                    return true;
                }

                if (result.Parent.Symbol.Name == "verbose" &&
                    result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") is OptionResult resRunLoop &&
                    !resRunLoop.GetValueOrDefault<bool>())
                {
                    return true;
                }

                return false;
            }

            if (!bool.TryParse(result.Tokens[0].Value, out val))
            {
                result.ErrorMessage = errorMessage;
                return false;
            }

            return val;
        }

        // parser for integer >= 0
        public static int ParseIntGEZero(ArgumentResult result)
        {
            return ParseInt(result, 0);
        }

        // parser for integer > 0
        public static int ParseIntGTZero(ArgumentResult result)
        {
            return ParseInt(result, 1);
        }

        // parser for integer
        public static int ParseInt(ArgumentResult result, int minValue)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return -1;
            }

            string errorMessage = $"--{result.Parent.Symbol.Name} must be an integer >= {minValue}";
            int val;

            // nothing to validate
            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    return GetCommandDefaultValues(result);
                }
                else
                {
                    if (!int.TryParse(env, out val) || val < minValue)
                    {
                        result.ErrorMessage = errorMessage;
                        return -1;
                    }

                    return val;
                }
            }

            if (!int.TryParse(result.Tokens[0].Value, out val) || val < minValue)
            {
                result.ErrorMessage = errorMessage;
                return -1;
            }

            return val;
        }

        // parser for enums
        public static AppType ParseAppType(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return AppType.App;
            }

            string env = result.Tokens.Count == 0 ? Environment.GetEnvironmentVariable(name) : result.Tokens[0].Value;

            if (!string.IsNullOrWhiteSpace(env))
            {
                if (Enum.TryParse<AppType>(env, out AppType at))
                {
                    return at;
                }
                else
                {
                    result.ErrorMessage = "--app-type must be of type AppType";
                }
            }

            // default value
            return AppType.App;
        }

        // parser for LogLevel
        public static LogLevel ParseLogLevel(ArgumentResult result)
        {
            App.IsLogLevelSet = false;

            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return LogLevel.Error;
            }

            string env = result.Tokens.Count == 0 ? Environment.GetEnvironmentVariable(name) : result.Tokens[0].Value;

            if (!string.IsNullOrWhiteSpace(env))
            {
                if (Enum.TryParse<LogLevel>(env, out LogLevel ll))
                {
                    App.IsLogLevelSet = true;
                    return ll;
                }
                else
                {
                    result.ErrorMessage = "--log-level must be of type LogLevel";
                }
            }

            // default value
            return LogLevel.Warning;
        }

        // get default values for command line args
        public static int GetCommandDefaultValues(ArgumentResult result)
        {
            return result.Parent.Symbol.Name switch
            {
                "perf-cache" => -1,
                "cache-duration" => 300,
                "retries" => 5,
                "timeout" => 30,
                _ => 0,
            };
        }
    }
}
