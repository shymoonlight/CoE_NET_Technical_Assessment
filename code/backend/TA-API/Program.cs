using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TA_API.Mapper;
using TA_API.Models.Data;
using TA_API.Services;
using TA_API.Services.Auth;
using TA_API.Services.Data;
using TA_API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

    // EF DbContext
    builder.Services.AddDbContext<AssessmentDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("AssessmentDB")));

    // AutoMapper (Map User to DTOs)
    var mappingAssemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Where(assembly => assembly.FullName!.StartsWith("TA_API"))
        .ToArray();

    builder.Services.AddAutoMapper(cfg => { }, mappingAssemblies); 

    var mapperConfig = new MapperConfiguration(config =>
    {
        Mappings.Configure(config);
    });

    IMapper mapper = mapperConfig.CreateMapper();
    builder.Services.AddSingleton(mapper);

    // Password hasher (Avoid storing plain text passwords)
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

    // Register controllers
    builder.Services.AddControllers();

    // CORS settings
    builder.Services.AddCors(o =>
    {
        o.AddPolicy("CorsPolicy", p =>
        {
            p.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader();
        });
    });

    // Register AuthService & UserService
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();

    // Swagger (API documentation and testing)
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configure JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();
}
var app = builder.Build();
{
    app.UseSerilogRequestLogging();

    // Seed admin
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var db = services.GetRequiredService<AssessmentDbContext>();

            // Get admin user settings from configuration
            var adminUserName = configuration["AdminUser:UserName"] ?? "admin";
            var adminEmail = configuration["AdminUser:Email"] ?? "admin@example.com";
            var adminPassword = configuration["AdminUser:Password"] ?? "ChangeMeInProd!";

            // Check if admin user exists
            if (!await db.Users.AnyAsync(u => u.UserName == adminUserName || u.Email == adminEmail))
            {
                // Create admin user
                var hasher = services.GetRequiredService<IPasswordHasher<User>>();
                var admin = new User
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    Name = "Administrator",
                    Role = "Admin",
                    CreationDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow
                };
                admin.Password = hasher.HashPassword(admin, adminPassword);

                // Save admin user to database
                db.Users.Add(admin);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Log errors during migration/seed
            var logger = services.GetService<ILogger<Program>>();
            logger?.LogError(ex, "Error during DB migration/seed.");
            throw;
        }
    }

    app.UseCors("CorsPolicy");

    // Set authorization & authentication
    app.UseAuthentication();
    app.UseAuthorization();

    // Map controllers
    app.MapControllers();

    // Swagger UI in Development environment
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger";
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TA API V1");
        });
    }

    app.MapGet("/", () => "Technical Assessment API");
    app.MapGet("/lbhealth", () => "Technical Assessment API");
}
app.Run();
