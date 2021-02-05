﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Imdb.Model;
using Microsoft.AspNetCore.Mvc;
using Ngsa.DataService.DataAccessLayer;
using Ngsa.Middleware;
using Ngsa.Middleware.Validation;

namespace Ngsa.DataService.Controllers
{
    /// <summary>
    /// Handle all of the /api/actors requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ActorsController : Controller
    {
        private static readonly NgsaLog Logger = new NgsaLog
        {
            Name = typeof(ActorsController).FullName,
            ErrorMessage = "ActorControllerException",
            NotFoundError = "Actor Not Found",
        };

        private readonly IDAL dal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorsController"/> class.
        /// </summary>
        public ActorsController()
        {
            // save to local for use in handlers
            dal = App.CosmosDal;
        }

        /// <summary>
        /// Returns a JSON array of Actor objects based on query parameters
        /// </summary>
        /// <param name="actorQueryParameters">query parameters</param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetActorsAsync([FromQuery] ActorQueryParameters actorQueryParameters)
        {
            if (actorQueryParameters == null)
            {
                throw new ArgumentNullException(nameof(actorQueryParameters));
            }

            List<ValidationError> list = actorQueryParameters.Validate();

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(GetActorsAsync), "Invalid query string", new LogEventId((int)HttpStatusCode.BadRequest, HttpStatusCode.BadRequest.ToString()), HttpContext);

                return ResultHandler.CreateResult(list, Request.Path.ToString() + (Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty));
            }

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<List<Actor>>(Request).ConfigureAwait(false);
            }
            else
            {
                res = await ResultHandler.Handle(dal.GetActorsAsync(actorQueryParameters), Logger).ConfigureAwait(false);

                // use cache dal on Cosmos 429 errors
                if (App.Config.Cache && res is JsonResult jres && jres.StatusCode == 429)
                {
                    Logger.LogWarning(nameof(GetActorsAsync), "Served from cache", new LogEventId(429, "Cosmos 429 Result"), HttpContext);

                    res = await ResultHandler.Handle(App.CacheDal.GetActorsAsync(actorQueryParameters), Logger).ConfigureAwait(false);
                }
            }

            return res;
        }

        /// <summary>
        /// Returns a single JSON Actor by actorId
        /// </summary>
        /// <param name="actorId">The actorId</param>
        /// <response code="404">actorId not found</response>
        /// <returns>IActionResult</returns>
        [HttpGet("{actorId}")]
        public async Task<IActionResult> GetActorByIdAsync([FromRoute] string actorId)
        {
            if (actorId == null)
            {
                throw new ArgumentNullException(nameof(actorId));
            }

            List<Middleware.Validation.ValidationError> list = ActorQueryParameters.ValidateActorId(actorId);

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(GetActorByIdAsync), "Invalid Actor Id", new LogEventId((int)HttpStatusCode.BadRequest, HttpStatusCode.BadRequest.ToString()), HttpContext);

                return ResultHandler.CreateResult(list, Request.Path.ToString() + (Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty));
            }

            IActionResult res;

            // return result
            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<Actor>(Request).ConfigureAwait(false);
            }
            else
            {
                res = await ResultHandler.Handle(dal.GetActorAsync(actorId), Logger).ConfigureAwait(false);

                // use cache dal on Cosmos 429 errors
                if (App.Config.Cache && res is JsonResult jres && jres.StatusCode == 429)
                {
                    Logger.LogWarning(nameof(GetActorByIdAsync), "Served from cache", new LogEventId(429, "Cosmos 429 Result"), HttpContext);

                    res = await ResultHandler.Handle(App.CacheDal.GetActorAsync(actorId), Logger).ConfigureAwait(false);
                }
            }

            return res;
        }
    }
}
