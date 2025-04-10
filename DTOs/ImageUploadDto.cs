using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LibraryManagementSystem.DTOs
{
    public class ImageUploadDto
    {
        [Required]
        public IFormFile File { get; set; }
    }

    public class ImageResponseDto
    {
        public string ImageUrl { get; set; }
    }
}