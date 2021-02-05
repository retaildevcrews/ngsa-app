﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ngsa.Middleware;

namespace Ngsa.DataService.Controllers
{
    /// <summary>
    /// Handle the single /api/genres requests
    /// </summary>
    [Route("api/[controller]")]
    public class GenresController : Controller
    {
        private static readonly NgsaLog Logger = new NgsaLog
        {
            Name = typeof(GenresController).FullName,
            ErrorMessage = "GenreControllerException",
        };

        /// <summary>
        /// Returns a JSON string array of Genre
        /// </summary>
        /// <response code="200">JSON array of strings or empty array if not found</response>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetGenresAsync()
        {
            NgsaLog nLogger = Logger.GetLogger(nameof(GetGenresAsync), HttpContext);

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<List<string>>(Request).ConfigureAwait(false);
            }
            else
            {
                res = await ResultHandler.Handle(App.CosmosDal.GetGenresAsync(), nLogger).ConfigureAwait(false);

                // use cache dal on Cosmos 429 errors
                if (App.Config.Cache && res is JsonResult jres && jres.StatusCode == 429)
                {
                    nLogger.EventId = new EventId(429, "Cosmos 429 Result");
                    nLogger.LogWarning("Served from cache");

                    res = await ResultHandler.Handle(App.CacheDal.GetGenresAsync(), nLogger).ConfigureAwait(false);
                }
            }

            return res;
        }
    }
}
