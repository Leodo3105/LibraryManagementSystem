using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.DTOs;

namespace LibraryManagementSystem.Helpers
{
    /// <summary>
    /// Helper class for creating standardized API responses
    /// </summary>
    public static class ApiResponseHelper
    {
        /// <summary>
        /// Creates a standard error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <returns>BadRequestObjectResult with error message</returns>
        public static BadRequestObjectResult ErrorResponse(string message)
        {
            return new BadRequestObjectResult(new ErrorResponse { Message = message });
        }

        /// <summary>
        /// Creates a standard success response
        /// </summary>
        /// <param name="message">Success message</param>
        /// <returns>OkObjectResult with success message</returns>
        public static OkObjectResult SuccessResponse(string message)
        {
            return new OkObjectResult(new SuccessResponse { Message = message });
        }

        /// <summary>
        /// Creates a standard data response
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="data">Data to return</param>
        /// <returns>OkObjectResult with data</returns>
        public static OkObjectResult DataResponse<T>(T data)
        {
            return new OkObjectResult(data);
        }
    }

    /// <summary>
    /// Standard error response DTO
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; }
        public bool Success => false;
    }

    /// <summary>
    /// Standard success response DTO
    /// </summary>
    public class SuccessResponse
    {
        public string Message { get; set; }
        public bool Success => true;
    }
}