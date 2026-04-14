using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedAdminAsync();
    }

    private async Task SeedAdminAsync()
    {
        var email = _configuration["AdminSeed:Email"];
        var password = _configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("AdminSeed config is missing. Skipping admin seed.");
            return;
        }

        var adminExists = await _context.Users.AnyAsync(u => u.Role == UserRole.ADMIN);
        if (adminExists)
            return;

        var admin = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            FullName = "Administrator",
            Role = UserRole.ADMIN,
        };

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin account seeded: {Email}", email);
    }
}
