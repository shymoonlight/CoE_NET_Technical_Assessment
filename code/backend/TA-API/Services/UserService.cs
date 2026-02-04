using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TA_API.Models.Data;
using TA_API.Models.DTOs;
using TA_API.Services.Data;
using TA_API.Services.Interfaces;

namespace TA_API.Services
{
    /// <summary>
    /// User management service implementing IUserService.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AssessmentDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the UserService.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="passwordHasher"></param>
        /// <param name="mapper"></param>
        public UserService(AssessmentDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        /// <summary>
        /// Determines whether the specified role represents an admin user.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        private static bool IsAdmin(string? role)
        {
            return !string.IsNullOrWhiteSpace(role) && string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserResponseDto>> GetUsersAsync(int? currentUserId, string? currentUserRole)
        {
            if (IsAdmin(currentUserRole))
            {
                var all = await _context.Users.AsNoTracking().ToListAsync();
                return _mapper.Map<List<UserResponseDto>>(all);
            }

            if (currentUserId == null || currentUserId == 0) return Array.Empty<UserResponseDto>();

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == currentUserId)
                .ToListAsync();

            return _mapper.Map<List<UserResponseDto>>(users);
        }

        /// <inheritdoc/>
        public async Task<UserResponseDto?> GetByIdAsync(int id, int? currentUserId, string? currentUserRole)
        {
            // Only admins or the user themselves can access
            if (!IsAdmin(currentUserRole) && currentUserId != id)
            {
                throw new UnauthorizedAccessException("You don't have permissions.");
            }

            // Retrieve user from database
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            // Map to response DTO or return null if not found
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }

        /// <inheritdoc/>
        public async Task<UserResponseDto> CreateUserAsync(UserDto dto, string? currentUserRole)
        {
            // Only admins can create users
            if (!IsAdmin(currentUserRole))
            {
                throw new UnauthorizedAccessException("You don't have permissions.");
            }

            // Basic DTO validation expected to be done by controller model state.
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException("Password is required for user creation.");
            }

            // Validate that email is unique
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (emailExists) throw new InvalidOperationException("Email already exists.");

            // Validate that username is unique
            var userNameExists = await _context.Users.AnyAsync(u => u.UserName.ToLower() == dto.UserName.ToLower());
            if (userNameExists) throw new InvalidOperationException("UserName already exists.");

            // Map DTO to entity and update values
            var user = _mapper.Map<User>(dto);
            user.CreationDate = DateTime.UtcNow;
            user.LastUpdateDate = DateTime.UtcNow;
            user.Password = _passwordHasher.HashPassword(user, dto.Password!);

            // Save user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return created user as response DTO
            return _mapper.Map<UserResponseDto>(user);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUserAsync(int id, UserDto dto, int? currentUserId, string? currentUserRole)
        {
            // Only admins or the user themselves can update
            if (!IsAdmin(currentUserRole) && currentUserId != id)
            {
                throw new UnauthorizedAccessException("You don't have permissions.");
            }

            // Find existing user
            var existing = await _context.Users.FindAsync(id);

            // If not found, return false
            if (existing is null)
            {
                return false;
            }

            // Prevent non-admins from changing role
            if (IsAdmin(currentUserRole))
            {
                existing.Role = dto.Role ?? existing.Role;
            }

            // Validate that email is unique if changed
            if (!string.Equals(existing.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.Id != id);
                if (emailExists) throw new InvalidOperationException("Email already exists.");
                existing.Email = dto.Email;
            }

            // Validate that username is unique if changed
            if (!string.Equals(existing.UserName, dto.UserName, StringComparison.OrdinalIgnoreCase))
            {
                var userNameExists = await _context.Users.AnyAsync(u => u.UserName.ToLower() == dto.UserName.ToLower() && u.Id != id);
                if (userNameExists) throw new InvalidOperationException("UserName already exists.");
                existing.UserName = dto.UserName;
            }

            // Update other fields
            existing.Name = dto.Name;

            // Update password if provided (if not empty)
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existing.Password = _passwordHasher.HashPassword(existing, dto.Password!);
            }

            existing.LastUpdateDate = DateTime.UtcNow;

            // Save changes to database
            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Return true indicating successful update
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteUserAsync(int id, string? currentUserRole)
        {
            // Only admins can delete users
            if (!IsAdmin(currentUserRole))
            {
                throw new UnauthorizedAccessException("You don't have permissions.");
            }

            // Find existing user
            var user = await _context.Users.FindAsync(id);

            // If not found, return false
            if (user is null)
            {
                return false;
            }

            // Remove user from database
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Return true indicating successful deletion
            return true;
        }
    }
}