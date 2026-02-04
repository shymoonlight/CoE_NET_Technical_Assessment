using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TA_API.Controllers;
using TA_API.Mapper;
using TA_API.Models.Data;
using TA_API.Models.DTOs;
using TA_API.Services.Auth;
using TA_API.Services.Data;

namespace TA_API_Tests
{
    public class LoginControllerTests
    {
        /// <summary>
        /// Helps create a new in-memory database context for testing.
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private static AssessmentDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AssessmentDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new AssessmentDbContext(options);
        }

        /// <summary>
        /// Creates and configure a new instance of AutoMapper for testing.
        /// </summary>
        /// <returns></returns>
        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
            return config.CreateMapper();
        }

        /// <summary>
        /// Creates a new instance of the password hasher for user password operations in tests.
        /// </summary>
        /// <returns></returns>
        private static PasswordHasher<User> CreateHasher() => new();

        /// <summary>
        /// Inserts a test user into the provided database context.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="hasher"></param>
        /// <returns></returns>
        private static async Task SeedUserAsync(AssessmentDbContext ctx, PasswordHasher<User> hasher)
        {
            var user = new User
            {
                Name = "Login Test",
                Email = "login@example.com",
                UserName = "loginuser",
                Role = "User",
                CreationDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow
            };
            user.Password = hasher.HashPassword(user, "Secret123!");
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
        }

        /// <summary>
        /// Creates configuration for JWT settings used in tests.
        /// </summary>
        /// <returns></returns>
        private static IConfiguration CreateJwtConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"JwtSettings:Key", "ThisIsATestKeyForJwtThatContainsAtLeast32Characters"},
                {"JwtSettings:Issuer", "testIssuer"},
                {"JwtSettings:Audience", "testAudience"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        [Fact]
        public async Task Login_ReturnsToken_OnValidCredentials()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);
            var hasher = CreateHasher();
            await SeedUserAsync(ctx, hasher);

            var mapper = CreateMapper();
            var configuration = CreateJwtConfiguration();

            var authService = new AuthService(ctx, hasher, configuration, mapper);
            var controller = new LoginController(authService);

            var request = new UserLoginRequestDto
            {
                UserName = "loginuser",
                Password = "Secret123!"
            };

            var actionResult = await controller.Login(request);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var value = ok.Value as AuthResult;

            Assert.NotNull(value);
            Assert.NotNull(value.Token);
            Assert.NotNull(value.User);
            Assert.Equal("loginuser", value.User.UserName);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_OnInvalidCredentials()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);
            var hasher = CreateHasher();
            await SeedUserAsync(ctx, hasher);

            var mapper = CreateMapper();
            var configuration = CreateJwtConfiguration();

            var authService = new AuthService(ctx, hasher, configuration, mapper);
            var controller = new LoginController(authService);

            var request = new UserLoginRequestDto
            {
                UserName = "loginuser",
                Password = "WrongPassword!"
            };

            var actionResult = await controller.Login(request);
            Assert.IsType<UnauthorizedObjectResult>(actionResult);
        }
    }
}
