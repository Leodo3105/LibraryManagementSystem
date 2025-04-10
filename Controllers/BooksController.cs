using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly FileService _fileService;

        public BooksController(ApplicationDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: api/books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks([FromQuery] BookSearchDto searchDto)
        {
            IQueryable<Book> query = _context.Books
                .Include(b => b.Category);

            // Filter by search term
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                searchDto.SearchTerm = searchDto.SearchTerm.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(searchDto.SearchTerm) ||
                    b.Author.ToLower().Contains(searchDto.SearchTerm) ||
                    b.ISBN.ToLower().Contains(searchDto.SearchTerm)
                );
            }

            // Filter by category
            if (searchDto.CategoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == searchDto.CategoryId.Value);
            }

            // Apply pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)searchDto.PageSize);

            var books = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    PublicationYear = b.PublicationYear,
                    Publisher = b.Publisher,
                    TotalCopies = b.TotalCopies,
                    AvailableCopies = b.AvailableCopies,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category.Name,
                    CoverImageUrl = b.CoverImageUrl,
                    Description = b.Description
                })
                .ToListAsync();

            Response.Headers.Add("X-Pagination-TotalItems", totalItems.ToString());
            Response.Headers.Add("X-Pagination-TotalPages", totalPages.ToString());
            Response.Headers.Add("X-Pagination-CurrentPage", searchDto.Page.ToString());

            return Ok(books);
        }

        // GET: api/books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                CategoryId = book.CategoryId,
                CategoryName = book.Category.Name,
                CoverImageUrl = book.CoverImageUrl,
                Description = book.Description
            };
        }

        // POST: api/books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BookDto>> CreateBook(BookCreateDto bookDto)
        {
            if (!string.IsNullOrEmpty(bookDto.ISBN) && await _context.Books.AnyAsync(b => b.ISBN == bookDto.ISBN))
                return BadRequest("ISBN already exists");

            var category = await _context.Categories.FindAsync(bookDto.CategoryId);
            if (category == null)
                return BadRequest("Invalid category ID");

            var book = new Book
            {
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                PublicationYear = bookDto.PublicationYear,
                Publisher = bookDto.Publisher,
                TotalCopies = bookDto.TotalCopies,
                AvailableCopies = bookDto.TotalCopies, // Initially, all copies are available
                CategoryId = bookDto.CategoryId,
                CoverImageUrl = bookDto.CoverImageUrl,
                Description = bookDto.Description
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                CategoryId = book.CategoryId,
                CategoryName = category.Name,
                CoverImageUrl = book.CoverImageUrl,
                Description = book.Description
            });
        }

        // PUT: api/books/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(int id, BookUpdateDto bookDto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            // Check if ISBN already exists (but not for this book)
            if (!string.IsNullOrEmpty(bookDto.ISBN) && await _context.Books.AnyAsync(b => b.ISBN == bookDto.ISBN && b.Id != id))
                return BadRequest("ISBN already exists");

            var category = await _context.Categories.FindAsync(bookDto.CategoryId);
            if (category == null)
                return BadRequest("Invalid category ID");

            // Calculate the difference in total copies
            var diffCopies = bookDto.TotalCopies - book.TotalCopies;

            // Nếu URL hình ảnh thay đổi, xóa hình ảnh cũ
            if (!string.IsNullOrEmpty(book.CoverImageUrl) &&
                book.CoverImageUrl != bookDto.CoverImageUrl)
            {
                _fileService.DeleteImage(book.CoverImageUrl);
            }

            book.Title = bookDto.Title;
            book.Author = bookDto.Author;
            book.ISBN = bookDto.ISBN;
            book.PublicationYear = bookDto.PublicationYear;
            book.Publisher = bookDto.Publisher;
            book.TotalCopies = bookDto.TotalCopies;
            // Adjust available copies proportionally
            book.AvailableCopies = Math.Max(0, book.AvailableCopies + diffCopies);
            book.CategoryId = bookDto.CategoryId;
            book.CoverImageUrl = bookDto.CoverImageUrl;
            book.Description = bookDto.Description;
            book.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/books/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.BookLoans)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            // Check if book has active loans
            if (book.BookLoans.Any(bl => bl.ReturnDate == null))
                return BadRequest("Cannot delete book with active loans");

            // Xóa file hình ảnh nếu có
            if (!string.IsNullOrEmpty(book.CoverImageUrl))
            {
                _fileService.DeleteImage(book.CoverImageUrl);
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}