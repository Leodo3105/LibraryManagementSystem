using System;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IPasswordService _passwordService;

        public AuthController(
            ApplicationDbContext context,
            TokenService tokenService,
            IPasswordService passwordService)
        {
            _context = context;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
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
                Role = "User" // Default role for new registrations
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(x => x.Username == loginDto.Username);

            if (user == null) return Unauthorized("Invalid username");

            // Verify password using the service
            if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                return Unauthorized("Invalid password");

            return new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }
    }
}