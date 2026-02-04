using TA_API.Models.DTOs;

namespace TA_API.Services.Interfaces
{
    /// <summary>
    /// Defines operations for managing user accounts, including retrieval, creation, update, and deletion
    /// functionality.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Asynchronously retrieves a collection of user records based on the current user role.
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="currentUserRole"></param>
        /// <returns></returns>
        Task<IEnumerable<UserResponseDto>> GetUsersAsync(int? currentUserId, string? currentUserRole);

        /// <summary>
        /// Retrieves an individual user by ID with authorization checks.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currentUserId"></param>
        /// <param name="currentUserRole"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        Task<UserResponseDto?> GetByIdAsync(int id, int? currentUserId, string? currentUserRole);

        /// <summary>
        /// Creates a new user with validation and authorization checks.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="currentUserRole"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        Task<UserResponseDto> CreateUserAsync(UserDto dto, string? currentUserRole);

        /// <summary>
        /// Updates an existing user with validation and authorization checks.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <param name="currentUserId"></param>
        /// <param name="currentUserRole"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        Task<bool> UpdateUserAsync(int id, UserDto dto, int? currentUserId, string? currentUserRole);

        /// <summary>
        /// Deletes a user by ID with authorization checks.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currentUserRole"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        Task<bool> DeleteUserAsync(int id, string? currentUserRole);
    }
}
