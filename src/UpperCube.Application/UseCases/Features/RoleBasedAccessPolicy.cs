using System.Security.Claims;
using UpperCube.Application.Abstractions.Features;

namespace UpperCube.Application.UseCases.Features;

public sealed class RoleBasedAccessPolicy : IFeatureAccessPolicy
{
    public string PolicyName => "RoleBased";

    public Task<FeatureAccessResult> EvaluateAsync(string featureCode, ClaimsPrincipal user)
    {
        var isAuthenticated = user.Identity?.IsAuthenticated == true;
        var result = isAuthenticated
            ? new FeatureAccessResult(true)
            : new FeatureAccessResult(false, "Требуется авторизация");

        return Task.FromResult(result);
    }
}