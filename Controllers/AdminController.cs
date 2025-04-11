using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;

        public AdminController(
            ApplicationDbContext context,
            IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // GET: api/admin/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalLoans = await _context.BookLoans.CountAsync();
            var activeLoans = await _context.BookLoans.CountAsync(bl => bl.ReturnDate == null);
            var overdueLoans = await _context.BookLoans
                .CountAsync(bl => bl.ReturnDate == null && bl.DueDate < DateTime.UtcNow);

            // Most borrowed books
            var popularBooks = await _context.Books
                .Include(b => b.BookLoans)
                .OrderByDescending(b => b.BookLoans.Count)
                .Take(5)
                .Select(b => new { b.Id, b.Title, LoanCount = b.BookLoans.Count })
                .ToListAsync();

            // Books with low stock
            var lowStockBooks = await _context.Books
                .Where(b => b.AvailableCopies <= 2 && b.TotalCopies > 0)
                .Select(b => new { b.Id, b.Title, b.AvailableCopies, b.TotalCopies })
                .ToListAsync();

            return new
            {
                TotalBooks = totalBooks,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                TotalLoans = totalLoans,
                ActiveLoans = activeLoans,
                OverdueLoans = overdueLoans,
                PopularBooks = popularBooks,
                LowStockBooks = lowStockBooks
            };
        }

        // GET: api/admin/loans
        [HttpGet("loans")]
        public async Task<ActionResult<IEnumerable<BookLoanDto>>> GetAllLoans([FromQuery] string status = null)
        {
            IQueryable<BookLoan> query = _context.BookLoans
                .Include(bl => bl.Book)
                .Include(bl => bl.User);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(bl => bl.Status == status);
            }

            var loans = await query
                .OrderByDescending(bl => bl.BorrowDate)
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

            return Ok(loans);
        }

        // POST: api/admin/users
        [HttpPost("users")]
        public async Task<ActionResult<UserDto>> CreateUser(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(x => x.Username == registerDto.Username))
                return ApiResponseHelper.ErrorResponse("Username is already taken");

            if (await _context.Users.AnyAsync(x => x.Email == registerDto.Email))
                return ApiResponseHelper.ErrorResponse("Email is already registered");

            // Hash password using the service
            string passwordHash = _passwordService.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Role = "User" // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        // PUT: api/admin/users/{id}/role
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] string role)
        {
            if (role != "Admin" && role != "User")
                return ApiResponseHelper.ErrorResponse("Role must be either 'Admin' or 'User'");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}