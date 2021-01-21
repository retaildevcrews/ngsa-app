// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Logging;

namespace Ngsa.Middleware
{
    public class NgsaLog
    {
        private static readonly Dictionary<int, NgsaLog> Loggers = new Dictionary<int, NgsaLog>();
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
        };

        private static int counter = 0;

        public string Name { get; set; } = string.Empty;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public string ErrorMessage { get; set; } = string.Empty;
        public string NotFoundError { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Exception Exception { get; set; } = null;
        public EventId EventId { get; set; } = new EventId(-1, string.Empty);
        public HttpContext Context { get; set; } = null;
        public Dictionary<string, string> Data { get; } = new Dictionary<string, string>();

        public NgsaLog GetLogger(string method, HttpContext context = null)
        {
            NgsaLog logger = new NgsaLog
            {
                Name = Name,
                ErrorMessage = ErrorMessage,
                NotFoundError = NotFoundError,
                LogLevel = LogLevel,

                Method = method,
                Context = context,
            };

            // get the next key
            while (Loggers.ContainsKey(counter))
            {
                if (counter == int.MaxValue)
                {
                    counter = 0;
                }

                counter++;
            }

            // todo - reuse loggers
            // use iDisposable?
            //Loggers.Add(counter, logger);

            return logger;
        }

        public void LogInformation(string message)
        {
            const LogLevel logLevel = LogLevel.Information;

            if (LogLevel <= logLevel)
            {
                Dictionary<string, object> d = GetDictionary(message, logLevel);

                WriteLog(d, ConsoleColor.Green);
            }
        }

        public void LogWarning(string message)
        {
            const LogLevel logLevel = LogLevel.Warning;

            if (LogLevel > logLevel)
            {
                return;
            }

            Dictionary<string, object> d = GetDictionary(message, logLevel);

            WriteLog(d, ConsoleColor.Yellow);
        }

        public void LogError(string message, Exception ex = null)
        {
            const LogLevel logLevel = LogLevel.Error;

            if (LogLevel > logLevel)
            {
                return;
            }

            Dictionary<string, object> d = GetDictionary(message, logLevel);

            if (ex != null)
            {
                d.Add("ExceptionType", ex.GetType());
                d.Add("ExceptionMessage", ex.Message);
            }
            else if (Exception != null)
            {
                d.Add("ExceptionType", Exception.GetType().FullName);
                d.Add("ExceptionMessage", Exception.Message);
            }

            // display the error
            WriteLog(d, ConsoleColor.Red);
        }

        private static void WriteLog(Dictionary<string, object> log, ConsoleColor color)
        {
            // todo - temporary workaround for XSS error

            StringBuilder sb = new StringBuilder("{");

            StringBuilder val;

            foreach (var key in log.Keys)
            {
                if (log[key] != null)
                {
                    val = new StringBuilder();

                    switch (log[key].GetType().Name)
                    {
                        case "DateTime":
                            val.Append('"');
                            val.Append(((DateTime)log[key]).ToString("o"));
                            val.Append('"');
                            break;

                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "Double":
                        case "Single":
                        case "Decimal":
                            val.Append(log[key]);
                            break;

                        default:
                            val.Append('"');
                            val.Append(log[key].ToString().Trim().Replace("\"", string.Empty));
                            val.Append('"');
                            break;
                    }

                    if (val != null && val.Length > 0)
                    {
                        if (sb.Length > 1)
                        {
                            sb.Append(',');
                        }

                        sb.Append(" \"");
                        sb.Append(key);
                        sb.Append("\": ");
                        sb.Append(val);
                    }
                }
            }

            sb.Append(" }");

            Console.ForegroundColor = color;

            if (color == ConsoleColor.Red)
            {
                // Console.Error.WriteLine(JsonSerializer.Serialize(log, Options));
                Console.Error.WriteLine(sb);
            }
            else
            {
                // Console.WriteLine(JsonSerializer.Serialize(log, Options));
                Console.WriteLine(sb);
            }

            Console.ResetColor();
        }

        private Dictionary<string, object> GetDictionary(string message, LogLevel logLevel)
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "Date", DateTime.UtcNow },
                { "LogName", Name },
                { "Method", Method },
                { "Message", message },
                { "LogLevel", logLevel.ToString() },
            };

            if (EventId.Id > 0)
            {
                data.Add("EventId", EventId.Id);
            }

            if (!string.IsNullOrWhiteSpace(EventId.Name))
            {
                data.Add("EventName", EventId.Name);
            }

            if (Context != null && Context.Items != null)
            {
                data.Add("Path", Context.Request.Path + (string.IsNullOrWhiteSpace(Context.Request.QueryString.Value) ? string.Empty : Context.Request.QueryString.Value));

                if (Context.Items != null)
                {
                    CorrelationVector cv = Middleware.CorrelationVectorExtensions.GetCorrelationVectorFromContext(Context);

                    if (cv != null)
                    {
                        data.Add("CVector", cv.Value);
                    }
                }
            }

            foreach (KeyValuePair<string, string> kvp in Data)
            {
                data.Add(kvp.Key, kvp.Value);
            }

            return data;
        }
    }
}
