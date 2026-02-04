using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TA_API.Models.Data;
using TA_API.Models.DTOs;
using TA_API.Services.Data;

namespace TA_API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AssessmentDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(
            AssessmentDbContext context,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration,
            IMapper mapper)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<AuthResult?> AuthenticateAsync(string userNameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var identifier = userNameOrEmail.Trim();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == identifier.ToLower() || u.Email.ToLower() == identifier.ToLower());

            if (user is null)
            {
                return null;
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (verification == PasswordVerificationResult.Failed) return null;

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(key)) throw new InvalidOperationException("JWT Key is not configured.");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(1);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            if (!string.IsNullOrEmpty(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            var writtenToken = new JwtSecurityTokenHandler().WriteToken(token);

            var dto = _mapper.Map<UserResponseDto>(user);

            return new AuthResult
            {
                Token = writtenToken,
                Expires = expires,
                User = dto
            };
        }
    }
}