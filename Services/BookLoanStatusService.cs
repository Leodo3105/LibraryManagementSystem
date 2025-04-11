using System;
using System.Collections.Generic;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    /// <summary>
    /// Service for handling book loan status-related operations
    /// </summary>
    public class BookLoanStatusService
    {
        /// <summary>
        /// All possible book loan statuses
        /// </summary>
        public static class LoanStatus
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Returned = "Returned";
            public const string Overdue = "Overdue";
        }

        /// <summary>
        /// Checks if the status transition is valid
        /// </summary>
        /// <param name="currentStatus">Current loan status</param>
        /// <param name="newStatus">New loan status</param>
        /// <returns>True if the transition is valid, false otherwise</returns>
        public bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            if (currentStatus == newStatus)
                return true;

            return currentStatus switch
            {
                LoanStatus.Pending => newStatus == LoanStatus.Approved || newStatus == LoanStatus.Rejected,
                LoanStatus.Approved => newStatus == LoanStatus.Returned || newStatus == LoanStatus.Overdue,
                LoanStatus.Overdue => newStatus == LoanStatus.Returned,
                _ => false
            };
        }

        /// <summary>
        /// Processes a book loan status change
        /// </summary>
        /// <param name="bookLoan">Book loan to update</param>
        /// <param name="newStatus">New status</param>
        /// <returns>Updated book loan</returns>
        public BookLoan ProcessStatusChange(BookLoan bookLoan, string newStatus)
        {
            if (!IsValidStatusTransition(bookLoan.Status, newStatus))
            {
                throw new InvalidOperationException($"Invalid status transition from {bookLoan.Status} to {newStatus}");
            }

            // Process the update based on the new status
            if (newStatus == LoanStatus.Returned && bookLoan.ReturnDate == null)
            {
                // Mark as returned and update book's available copies
                bookLoan.ReturnDate = DateTime.UtcNow;
                bookLoan.Book.AvailableCopies++;
            }
            else if (newStatus == LoanStatus.Approved && bookLoan.Status == LoanStatus.Pending)
            {
                // Book is now officially checked out
                // No need to update available copies again as we already did that when creating the loan
            }
            else if (newStatus == LoanStatus.Rejected && bookLoan.Status == LoanStatus.Pending)
            {
                // Rejected loan - return the book to inventory
                bookLoan.Book.AvailableCopies++;
            }

            bookLoan.Status = newStatus;
            bookLoan.UpdatedAt = DateTime.UtcNow;

            return bookLoan;
        }

        /// <summary>
        /// Creates a default loan with standard settings
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="bookId">Book ID</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>New BookLoan object</returns>
        public BookLoan CreateDefaultLoan(int userId, int bookId, string notes = null)
        {
            return new BookLoan
            {
                UserId = userId,
                BookId = bookId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14), // 14 days loan period
                Status = LoanStatus.Pending, // Initial status is pending admin approval
                Notes = notes
            };
        }
    }
}