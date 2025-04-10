using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            // Users can only view their own profile, unless they're an admin
            if (!isAdmin && id != currentUserId)
                return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        // GET: api/users/profile
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(currentUserId);

            if (user == null)
                return NotFound();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        // GET: api/users/loans
        [HttpGet("loans")]
        public async Task<ActionResult<IEnumerable<BookLoanDto>>> GetUserLoans()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var loans = await _context.BookLoans
                .Include(bl => bl.Book)
                .Where(bl => bl.UserId == currentUserId)
                .OrderByDescending(bl => bl.BorrowDate)
                .Select(bl => new BookLoanDto
                {
                    Id = bl.Id,
                    UserId = bl.UserId,
                    Username = User.FindFirst(ClaimTypes.Name).Value,
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
    }
}