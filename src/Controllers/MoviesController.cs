// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Imdb.Model;
using Microsoft.AspNetCore.Mvc;
using Ngsa.Application.DataAccessLayer;
using Ngsa.Middleware;

namespace Ngsa.Application.Controllers
{
    /// <summary>
    /// Handle all of the /api/movies requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : Controller
    {
        private static readonly NgsaLog Logger = new NgsaLog
        {
            Name = typeof(MoviesController).FullName,
            ErrorMessage = "MovieControllerException",
            NotFoundError = "Movie Not Found",
        };

        private readonly IDAL dal;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoviesController"/> class.
        /// </summary>
        public MoviesController()
        {
            dal = App.Config.CosmosDal;
        }

        /// <summary>
        /// Returns a JSON array of Movie objects
        /// </summary>
        /// <param name="movieQueryParameters">query parameters</param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetMoviesAsync([FromQuery] MovieQueryParameters movieQueryParameters)
        {
            if (movieQueryParameters == null)
            {
                throw new ArgumentNullException(nameof(movieQueryParameters));
            }

            List<Middleware.Validation.ValidationError> list = movieQueryParameters.Validate();

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(GetMoviesAsync), NgsaLog.MessageInvalidQueryString, NgsaLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<List<Movie>>(Request).ConfigureAwait(false);
            }
            else
            {
                // get the result
                res = await ResultHandler.Handle(dal.GetMoviesAsync(movieQueryParameters), Logger).ConfigureAwait(false);

                // use cache dal on Cosmos 429 errors
                // todo - this will never get called
                //        if App.Config.Cache, the cache will be used in ResultHandler
                //        we would need an additional flag and update the cache creation
                if (App.Config.Cache && res is JsonResult jres && jres.StatusCode == 429)
                {
                    Logger.Log429(nameof(GetMoviesAsync), HttpContext);

                    res = await ResultHandler.Handle(App.Config.CacheDal.GetMoviesAsync(movieQueryParameters), Logger).ConfigureAwait(false);
                }
            }

            return res;
        }

        /// <summary>
        /// Returns a single JSON Movie by movieIdParameter
        /// </summary>
        /// <param name="movieId">Movie ID</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{movieId}")]
        public async Task<IActionResult> GetMovieByIdAsync([FromRoute] string movieId)
        {
            if (string.IsNullOrWhiteSpace(movieId))
            {
                throw new ArgumentNullException(nameof(movieId));
            }

            List<Middleware.Validation.ValidationError> list = MovieQueryParameters.ValidateMovieId(movieId);

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(GetMoviesAsync), "Invalid Movie Id", NgsaLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<Movie>(Request).ConfigureAwait(false);
            }
            else
            {
                // todo - remove this once upsert / delete work
                if (movieId.StartsWith("zz"))
                {
                    res = await ResultHandler.Handle(App.Config.CacheDal.GetMovieAsync(movieId), Logger).ConfigureAwait(false);
                }
                else
                {
                    res = await ResultHandler.Handle(dal.GetMovieAsync(movieId), Logger).ConfigureAwait(false);
                }

                // use cache dal on Cosmos 429 errors
                if (App.Config.Cache && res is JsonResult jres && jres.StatusCode == 429)
                {
                    Logger.Log429(nameof(GetMovieByIdAsync), HttpContext);

                    res = await ResultHandler.Handle(App.Config.CacheDal.GetMovieAsync(movieId), Logger).ConfigureAwait(false);
                }
            }

            return res;
        }

        [HttpPut("{movieId}")]
        public async Task<IActionResult> UpsertMovieAsync([FromRoute] string movieId)
        {
            try
            {
                List<Middleware.Validation.ValidationError> list = MovieQueryParameters.ValidateMovieId(movieId);

                if (list.Count > 0 || !movieId.StartsWith("zz"))
                {
                    Logger.LogWarning(nameof(UpsertMovieAsync), "Invalid Movie Id", NgsaLog.LogEvent400, HttpContext);

                    return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
                }

                // duplicate the movie for upsert
                Movie mOrig = App.Config.CacheDal.GetMovie(movieId.Replace("zz", "tt"));
                Movie m = mOrig.DuplicateForUpsert();

                IActionResult res;

                if (App.Config.AppType == AppType.WebAPI)
                {
                    // todo - implement
                    res = await DataService.Post(Request, m).ConfigureAwait(false);
                }
                else
                {
                    // todo - implement for Cosmos
                    m = App.Config.CacheDal.UpsertMovie(m, out HttpStatusCode status);

                    if (status == HttpStatusCode.Created)
                    {
                        res = Created($"/api/movies/{m.MovieId}", m);
                    }
                    else
                    {
                        res = Ok(m);
                    }
                }

                return res;
            }
            catch
            {
                return NotFound($"Movie ID Not Found: {movieId}");
            }
        }

        /// <summary>
        /// Delete a movie by movieId
        /// </summary>
        /// <param name="movieId">ID to delete</param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{movieId}")]
        public async Task<IActionResult> DeleteMovieAsync([FromRoute] string movieId)
        {
            List<Middleware.Validation.ValidationError> list = MovieQueryParameters.ValidateMovieId(movieId);

            if (list.Count > 0 || !movieId.StartsWith("zz"))
            {
                Logger.LogWarning(nameof(UpsertMovieAsync), "Invalid Movie Id", NgsaLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                // todo - implement
                res = await DataService.Delete(Request).ConfigureAwait(false);
            }
            else
            {
                // todo - implement in Cosmos DB
                App.Config.CacheDal.DeleteMovie(movieId);
                res = NoContent();
            }

            return res;
        }
    }
}
