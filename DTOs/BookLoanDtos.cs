using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DTOs
{
    public class BookLoanDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class BookLoanCreateDto
    {
        [Required]
        public int BookId { get; set; }

        // DueDate được tính từ ngày mượn + số ngày cho phép mượn
        // Thường được cấu hình ở phía server (ví dụ: 14 ngày)

        public string Notes { get; set; }
    }

    public class BookLoanUpdateDto
    {
        [Required]
        public string Status { get; set; }  // "Approved", "Rejected", "Returned"

        public string Notes { get; set; }
    }

    public class BookLoanSearchDto
    {
        public string Status { get; set; }
        public int? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}