using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TA_API.Controllers;
using TA_API.Mapper;
using TA_API.Models.Data;
using TA_API.Models.DTOs;
using TA_API.Services;
using TA_API.Services.Data;
using Xunit;

namespace TA_API.Tests
{
    public class UserControllerTests
    {
        private static AssessmentDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AssessmentDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AssessmentDbContext(options);
        }

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
            return config.CreateMapper();
        }

        private static PasswordHasher<User> CreateHasher() => new();

        private static async Task SeedUsersAsync(AssessmentDbContext ctx, PasswordHasher<User> hasher)
        {
            var admin = new User
            {
                Name = "Admin",
                Email = "admin@example.com",
                UserName = "admin",
                Role = "Admin",
                CreationDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow
            };
            admin.Password = hasher.HashPassword(admin, "AdminPass123!");

            var user1 = new User
            {
                Name = "User One",
                Email = "user1@example.com",
                UserName = "user1",
                Role = "User",
                CreationDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow
            };
            user1.Password = hasher.HashPassword(user1, "User1Pass!");

            ctx.Users.AddRange(admin, user1);
            await ctx.SaveChangesAsync();
        }

        private static ClaimsPrincipal CreatePrincipal(int id, string userName, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        }

        private static UserService CreateUserService(AssessmentDbContext ctx, PasswordHasher<User> hasher, IMapper mapper)
        {
            return new UserService(ctx, hasher, mapper);
        }

        [Fact]
        public async Task Get_ReturnsAllUsers_ForAdmin()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);
            var hasher = CreateHasher();
            await SeedUsersAsync(ctx, hasher);

            var mapper = CreateMapper();
            var userService = CreateUserService(ctx, hasher, mapper);
            var controller = new UserController(userService);

            var admin = await ctx.Users.FirstAsync(u => u.UserName == "admin");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = CreatePrincipal(admin.Id, admin.UserName, "Admin") }
            };

            var result = await controller.Get();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<UserResponseDto>>(ok.Value);
            Assert.True(list.Count() >= 2);
        }

        [Fact]
        public async Task Get_ReturnsOnlySelf_ForNonAdmin()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);
            var hasher = CreateHasher();
            await SeedUsersAsync(ctx, hasher);

            var mapper = CreateMapper();
            var userService = CreateUserService(ctx, hasher, mapper);
            var controller = new UserController(userService);

            var user = await ctx.Users.FirstAsync(u => u.UserName == "user1");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = CreatePrincipal(user.Id, user.UserName, "User") }
            };

            var result = await controller.Get();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<UserResponseDto>>(ok.Value);
            Assert.Single(list);
            Assert.Equal("user1", list.First().UserName);
        }

        [Fact]
        public async Task Post_ReturnsForbidden_ForNonAdmin()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);
            var hasher = CreateHasher();
            await SeedUsersAsync(ctx, hasher);

            var mapper = CreateMapper();
            var userService = CreateUserService(ctx, hasher, mapper);
            var controller = new UserController(userService);

            var user = await ctx.Users.FirstAsync(u => u.UserName == "user1");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = CreatePrincipal(user.Id, user.UserName, "User") }
            };

            var dto = new UserDto { Name = "New", Email = "new@example.com", UserName = "newuser", Password = "pwd123!", Role = "User" };
            var result = await controller.Post(dto);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task Put_AllowsSelfUpdate()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);
            var hasher = CreateHasher();
            await SeedUsersAsync(ctx, hasher);

            var mapper = CreateMapper();
            var userService = CreateUserService(ctx, hasher, mapper);
            var controller = new UserController(userService);

            var user = await ctx.Users.FirstAsync(u => u.UserName == "user1");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = CreatePrincipal(user.Id, user.UserName, "User") }
            };

            var updateDto = new UserDto { Name = "User One Updated", Email = user.Email, UserName = user.UserName, Password = "", Role = user.Role };
            var result = await controller.Put(user.Id, updateDto);

            Assert.IsType<OkResult>(result);

            var updated = await ctx.Users.FindAsync(user.Id);
            Assert.Equal("User One Updated", updated!.Name);
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_ForNonAdmin()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);
            var hasher = CreateHasher();
            await SeedUsersAsync(ctx, hasher);

            var mapper = CreateMapper();
            var userService = CreateUserService(ctx, hasher, mapper);
            var controller = new UserController(userService);

            var user = await ctx.Users.FirstAsync(u => u.UserName == "user1");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = CreatePrincipal(user.Id, user.UserName, "User") }
            };

            var result = await controller.Delete(user.Id);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }
    }
}