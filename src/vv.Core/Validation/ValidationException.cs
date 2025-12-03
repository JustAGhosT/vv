using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using vv.Core.Validation;

namespace vv.Domain.Validation
{
    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Gets the collection of validation errors that caused this exception
        /// </summary>
        public IReadOnlyList<ValidationError> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a collection of validation errors
        /// </summary>
        /// <param name="errors">The validation errors, or null for an empty collection</param>
        public ValidationException(IEnumerable<ValidationError>? errors)
            : base(CreateMessage(errors))
        {
            // Ensure we always have a non-null collection of errors
            Errors = errors?.ToList().AsReadOnly() ?? new ReadOnlyCollection<ValidationError>(Array.Empty<ValidationError>());
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a single validation error
        /// </summary>
        /// <param name="propertyName">The name of the property that failed validation</param>
        /// <param name="errorMessage">The validation error message</param>
        /// <param name="errorCode">Optional error code</param>
        /// <param name="source">Optional source of the validation error</param>
        public ValidationException(string propertyName, string errorMessage, string? errorCode = null, string? source = null)
            : this(new[] { new ValidationError
                {
                    PropertyName = propertyName,
                    ErrorMessage = errorMessage,
                    ErrorCode = errorCode,
                    Source = source
                }
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a specified error message
        /// </summary>
        /// <param name="message">The error message</param>
        public ValidationException(string message)
            : base(message)
        {
            Errors = Array.Empty<ValidationError>();
        }

        /// <summary>
        /// Creates a validation exception message from a collection of validation errors
        /// </summary>
        /// <param name="errors">The validation errors</param>
        /// <returns>A formatted error message</returns>
        private static string CreateMessage(IEnumerable<ValidationError>? errors)
        {
            if (errors == null || !errors.Any())
                return "Validation failed.";

            var messages = errors.Select(e =>
                string.IsNullOrWhiteSpace(e.PropertyName)
                    ? e.ErrorMessage
                    : $"{e.PropertyName}: {e.ErrorMessage}");

            return "Validation failed: " + string.Join("; ", messages);
        }
    }
}