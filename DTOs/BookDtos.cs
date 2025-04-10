using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public int? PublicationYear { get; set; }
        public string Publisher { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CoverImageUrl { get; set; }
        public string Description { get; set; }
    }

    public class BookCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        [StringLength(20)]
        public string ISBN { get; set; }

        public int? PublicationYear { get; set; }

        [StringLength(100)]
        public string Publisher { get; set; }

        public int TotalCopies { get; set; } = 1;

        public int CategoryId { get; set; }

        public string CoverImageUrl { get; set; }

        public string Description { get; set; }
    }

    public class BookUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        [StringLength(20)]
        public string ISBN { get; set; }

        public int? PublicationYear { get; set; }

        [StringLength(100)]
        public string Publisher { get; set; }

        public int TotalCopies { get; set; }

        public int CategoryId { get; set; }

        public string CoverImageUrl { get; set; }

        public string Description { get; set; }
    }

    public class BookSearchDto
    {
        public string SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}