using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookLoansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BookLoanStatusService _loanStatusService;

        public BookLoansController(
            ApplicationDbContext context,
            BookLoanStatusService loanStatusService)
        {
            _context = context;
            _loanStatusService = loanStatusService;
        }

        // GET: api/bookloans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookLoanDto>>> GetBookLoans([FromQuery] BookLoanSearchDto searchDto)
        {
            IQueryable<BookLoan> query = _context.BookLoans
                .Include(bl => bl.Book)
                .Include(bl => bl.User);

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            // Admin can see all loans, users can only see their own
            if (!isAdmin)
            {
                query = query.Where(bl => bl.UserId == currentUserId);
            }
            // If admin is filtering by user
            else if (searchDto.UserId.HasValue)
            {
                query = query.Where(bl => bl.UserId == searchDto.UserId.Value);
            }

            // Filter by status
            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                query = query.Where(bl => bl.Status == searchDto.Status);
            }

            // Filter by date range
            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(bl => bl.BorrowDate >= searchDto.FromDate.Value);
            }
            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(bl => bl.BorrowDate <= searchDto.ToDate.Value);
            }

            // Apply pagination
            var totalItems = await query.CountAsync();
            var (skip, totalPages) = PaginationHelper.CalculatePagination(totalItems, searchDto.PageSize, searchDto.Page);

            var bookLoans = await query
                .OrderByDescending(bl => bl.BorrowDate)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .Select(bl => new BookLoanDto
                {
                    Id = bl.Id,
                    UserId = bl.UserId,
                    Username = bl.User.Username,
                    BookId = bl.BookId,
                    BookTitle = bl.Book.Title,
                    BorrowDate = bl.BorrowDate,
                    DueDate = bl.DueDate,
                    ReturnDate = bl.ReturnDate,
                    Status = bl.Status,
                    Notes = bl.Notes
                })
                .ToListAsync();

            PaginationHelper.AddPaginationHeaders(Response, totalItems, searchDto.PageSize, searchDto.Page);

            return Ok(bookLoans);
        }

        // GET: api/bookloans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookLoanDto>> GetBookLoan(int id)
        {
            var bookLoan = await _context.BookLoans
                .Include(bl => bl.Book)
                .Include(bl => bl.User)
                .FirstOrDefaultAsync(bl => bl.Id == id);

            if (bookLoan == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            // Users can only view their own loans
            if (!isAdmin && bookLoan.UserId != currentUserId)
                return Forbid();

            return new BookLoanDto
            {
                Id = bookLoan.Id,
                UserId = bookLoan.UserId,
                Username = bookLoan.User.Username,
                BookId = bookLoan.BookId,
                BookTitle = bookLoan.Book.Title,
                BorrowDate = bookLoan.BorrowDate,
                DueDate = bookLoan.DueDate,
                ReturnDate = bookLoan.ReturnDate,
                Status = bookLoan.Status,
                Notes = bookLoan.Notes
            };
        }

        // POST: api/bookloans
        [HttpPost]
        public async Task<ActionResult<BookLoanDto>> CreateBookLoan(BookLoanCreateDto bookLoanDto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Check if book exists and is available
            var book = await _context.Books.FindAsync(bookLoanDto.BookId);
            if (book == null)
                return ApiResponseHelper.ErrorResponse("Book not found");

            if (book.AvailableCopies <= 0)
                return ApiResponseHelper.ErrorResponse("No copies of this book are available");

            // Check if user already has an active loan for this book
            var existingLoan = await _context.BookLoans
                .AnyAsync(bl => bl.UserId == currentUserId &&
                           bl.BookId == bookLoanDto.BookId &&
                           bl.ReturnDate == null);

            if (existingLoan)
                return ApiResponseHelper.ErrorResponse("You already have an active loan for this book");

            // Create the loan using the service
            var bookLoan = _loanStatusService.CreateDefaultLoan(currentUserId, bookLoanDto.BookId, bookLoanDto.Notes);

            _context.BookLoans.Add(bookLoan);

            // Update book available copies
            book.AvailableCopies--;

            await _context.SaveChangesAsync();

            // Get the created loan with related data
            bookLoan = await _context.BookLoans
                .Include(bl => bl.Book)
                .Include(bl => bl.User)
                .FirstOrDefaultAsync(bl => bl.Id == bookLoan.Id);

            return CreatedAtAction(nameof(GetBookLoan), new { id = bookLoan.Id }, new BookLoanDto
            {
                Id = bookLoan.Id,
                UserId = bookLoan.UserId,
                Username = bookLoan.User.Username,
                BookId = bookLoan.BookId,
                BookTitle = bookLoan.Book.Title,
                BorrowDate = bookLoan.BorrowDate,
                DueDate = bookLoan.DueDate,
                ReturnDate = bookLoan.ReturnDate,
                Status = bookLoan.Status,
                Notes = bookLoan.Notes
            });
        }

        // PUT: api/bookloans/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBookLoan(int id, BookLoanUpdateDto bookLoanDto)
        {
            var bookLoan = await _context.BookLoans
                .Include(bl => bl.Book)
                .FirstOrDefaultAsync(bl => bl.Id == id);

            if (bookLoan == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            // Check permissions
            if (!isAdmin && bookLoan.UserId != currentUserId)
                return Forbid();

            // Regular users can only mark loans as returned
            if (!isAdmin && bookLoanDto.Status != BookLoanStatusService.LoanStatus.Returned)
                return ApiResponseHelper.ErrorResponse("You can only mark loans as returned");

            try
            {
                // Process the status change using the service
                _loanStatusService.ProcessStatusChange(bookLoan, bookLoanDto.Status);

                // Update notes if provided
                if (bookLoanDto.Notes != null)
                {
                    bookLoan.Notes = bookLoanDto.Notes;
                }

                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponseHelper.ErrorResponse(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookLoanExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        private bool BookLoanExists(int id)
        {
            return _context.BookLoans.Any(e => e.Id == id);
        }
    }
}