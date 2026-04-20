using Application.Common.Interfaces;
using Application.Common.Services;
using Application.Features.Payment.Jobs;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayOS;

namespace Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DatabaseSeeder>();

        var payosSettings = configuration.GetSection("PayOSSettings").Get<PayOSSettings>()!;
        services.Configure<PayOSSettings>(configuration.GetSection("PayOSSettings"));
        services.AddSingleton(new PayOSClient(payosSettings.ClientId, payosSettings.ApiKey, payosSettings.ChecksumKey));
        services.AddScoped<IPaymentService, PayOSService>();

        services.AddScoped<ExpiredPaymentJob>();

        return services;
    }
}
