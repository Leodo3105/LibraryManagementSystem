using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Helpers
{
    /// <summary>
    /// Helper class for handling pagination-related operations
    /// </summary>
    public static class PaginationHelper
    {
        /// <summary>
        /// Adds pagination headers to HTTP response
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <param name="totalItems">Total count of items</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="currentPage">Current page number</param>
        public static void AddPaginationHeaders(HttpResponse response, int totalItems, int pageSize, int currentPage)
        {
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            response.Headers.Add("X-Pagination-TotalItems", totalItems.ToString());
            response.Headers.Add("X-Pagination-TotalPages", totalPages.ToString());
            response.Headers.Add("X-Pagination-CurrentPage", currentPage.ToString());
        }

        /// <summary>
        /// Calculates pagination details based on input parameters
        /// </summary>
        /// <param name="totalItems">Total count of items</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="currentPage">Current page number</param>
        /// <returns>Tuple with skip value and total pages</returns>
        public static (int skip, int totalPages) CalculatePagination(int totalItems, int pageSize, int currentPage)
        {
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var skip = (currentPage - 1) * pageSize;

            return (skip, totalPages);
        }
    }
}