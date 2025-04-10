using System;
using System.Threading.Tasks;
using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly FileService _fileService;

        public UploadsController(FileService fileService)
        {
            _fileService = fileService;
        }

        // POST: api/uploads/images
        [HttpPost("images")]
        [Authorize]
        public async Task<ActionResult<ImageResponseDto>> UploadImage([FromForm] ImageUploadDto imageDto)
        {
            if (imageDto.File == null || imageDto.File.Length == 0)
            {
                return BadRequest("Không có file được tải lên");
            }

            try
            {
                var imageUrl = await _fileService.SaveImageAsync(imageDto.File);
                return new ImageResponseDto { ImageUrl = imageUrl };
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi tải lên hình ảnh");
            }
        }
    }
}