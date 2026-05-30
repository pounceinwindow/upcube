using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UpperCube.Application.Abstractions.AI;
using UpperCube.Application.Abstractions.Authentication;
using UpperCube.Application.Abstractions.Media;
using UpperCube.Application.Abstractions.Notifications;
using UpperCube.Application.Abstractions.Persistence;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Infrastructure.Identity;
using UpperCube.Infrastructure.Mongo;
using UpperCube.Infrastructure.Persistence;
using UpperCube.Infrastructure.Persistence.Interceptors;
using UpperCube.Infrastructure.Persistence.Repositories;
using UpperCube.Infrastructure.Services.AI;
using UpperCube.Infrastructure.Services.Authentication;
using UpperCube.Infrastructure.Services.Email;
using UpperCube.Infrastructure.Services.ImageStorage;

namespace UpperCube.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'Postgres' is not configured.");

        services.AddSingleton<AuditableEntityInterceptor>();
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

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
                options.SignIn.RequireConfirmedEmail = false;
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

        services.AddSingleton<MongoContext>();
        services.AddScoped<IAuditLogStore, AuditLogStore>();
        services.AddScoped<IErrorLogStore, ErrorLogStore>();
        services.AddScoped<IAccountService, IdentityAccountService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IImageStorage, LocalDiskImageStorage>();
        services.AddHttpClient<ILocalLlmClient, OllamaLocalLlmClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(options.TimeoutSeconds, 1));
        });

        return services;
    }
}
