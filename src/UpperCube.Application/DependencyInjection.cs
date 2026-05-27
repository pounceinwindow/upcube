using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UpperCube.Application.Abstractions.Features;
using UpperCube.Application.Abstractions.Valuation;
using UpperCube.Application.UseCases.Features;
using UpperCube.Application.UseCases.Valuation;

namespace UpperCube.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegionalAverageValuator>();
        services.AddScoped<ComparableSalesValuator>();
        services.AddScoped<CompositeValuator>();
        services.AddSingleton<ValuationExplanationPromptBuilder>();
        services.AddScoped<IValuator>(serviceProvider => serviceProvider.GetRequiredService<RegionalAverageValuator>());
        services.AddScoped<IValuator>(serviceProvider => serviceProvider.GetRequiredService<ComparableSalesValuator>());
        services.AddScoped<IValuator>(serviceProvider => serviceProvider.GetRequiredService<CompositeValuator>());

        services.AddScoped<IFeatureAccessChecker, FeatureAccessChecker>();
        services.AddScoped<IFeatureAccessPolicy, RoleBasedAccessPolicy>();
        services.AddScoped<IFeatureAccessPolicy, VerificationAccessPolicy>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}