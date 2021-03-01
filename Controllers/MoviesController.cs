// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Imdb.Model;
using Microsoft.AspNetCore.Mvc;
using Ngsa.DataService.DataAccessLayer;
using Ngsa.Middleware;

namespace Ngsa.DataService.Controllers
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
            dal = App.CosmosDal;
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
                if (App.Config.Cache && res is JsonResult jres && jres.StatusCode == 429)
                {
                    Logger.Log429(nameof(GetMoviesAsync), HttpContext);

                    res = await ResultHandler.Handle(App.CacheDal.GetMoviesAsync(movieQueryParameters), Logger).ConfigureAwait(false);
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
                res = await ResultHandler.Handle(dal.GetMovieAsync(movieId), Logger).ConfigureAwait(false);

                // use cache dal on Cosmos 429 errors
                if (App.Config.Cache && res is JsonResult jres && jres.StatusCode == 429)
                {
                    Logger.Log429(nameof(GetMovieByIdAsync), HttpContext);

                    res = await ResultHandler.Handle(App.CacheDal.GetMovieAsync(movieId), Logger).ConfigureAwait(false);
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

                Movie mOrig = App.CacheDal.GetMovie(movieId.Replace("zz", "tt"));

                Movie m = mOrig.Clone() as Movie;

                m.MovieId = movieId;
                m.Id = movieId;
                m.Type = "Movie-Dupe";

                IActionResult res;

                if (App.Config.AppType == AppType.WebAPI)
                {
                    // todo - implement
                    res = await DataService.Read<Movie>(Request).ConfigureAwait(false);
                }
                else
                {
                    m = App.CacheDal.UpsertMovie(m, out HttpStatusCode status);

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
                res = await DataService.Read<Movie>(Request).ConfigureAwait(false);
            }
            else
            {
                // todo - implement in Cosmos DB
                App.CacheDal.DeleteMovie(movieId);
                res = NoContent();
            }

            return res;
        }
    }
}
