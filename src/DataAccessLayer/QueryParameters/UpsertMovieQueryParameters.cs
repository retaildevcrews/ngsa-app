// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Ngsa.Middleware.Validation;

namespace Ngsa.Middleware
{
    /// <summary>
    /// Query string parameters for PUT Movies controller method
    /// </summary>
    public sealed class UpsertMovieQueryParameters
    {
        public int PayloadSize { get; set; }

        /// <summary>
        /// Validate this object
        /// </summary>
        /// <returns>list of validation errors or empty list</returns>
        public List<ValidationError> Validate()
        {
            List<ValidationError> errors = new ();

            if (PayloadSize > 1024 * 1024)
            {
                errors.Add(new ValidationError { Target = "payload size", Message = $"payload size must be <= 1 MB ({1024 * 1024})" });
            }
            else if (PayloadSize < 0)
            {
                errors.Add(new ValidationError { Target = "payload size", Message = $"payload size must be >= 0" });
            }


            return errors;
        }
    }
}
