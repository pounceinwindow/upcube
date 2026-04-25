using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Infrastructure.Identity;
using UpperCube.Infrastructure.Persistence;
using UpperCube.Infrastructure.Persistence.Interceptors;
using UpperCube.Infrastructure.Persistence.Repositories;

namespace UpperCube.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IInquiryRepository, InquiryRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IComparisonRepository, ComparisonRepository>();
        services.AddScoped<IValuationRepository, ValuationRepository>();
        services.AddScoped<IFeatureCatalogRepository, FeatureCatalogRepository>();
        services.AddScoped(typeof(IDictionaryRepository<>), typeof(DictionaryRepository<>));
        services.AddScoped<IModerationRepository, ModerationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
