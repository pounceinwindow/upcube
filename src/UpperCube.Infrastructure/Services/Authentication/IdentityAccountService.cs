using Microsoft.AspNetCore.Identity;
using UpperCube.Application.Abstractions.Authentication;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Infrastructure.Services.Authentication;

public sealed class IdentityAccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager) : IAccountService
{
    public async Task<AccountSignInResult> PasswordSignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken ct = default)
    {
        var result = await signInManager.PasswordSignInAsync(
            email,
            password,
            rememberMe,
            true);

        return new AccountSignInResult(
            result.Succeeded,
            result.RequiresTwoFactor,
            result.IsLockedOut,
            result.Succeeded ? null : "Неверный email или пароль.");
    }

    public async Task<AccountOperationResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
            return AccountOperationResult.Failed("Аккаунт с таким email уже зарегистрирован. Войдите в аккаунт.");

        if (!await roleManager.RoleExistsAsync("User"))
        {
            var createRoleResult = await roleManager.CreateAsync(new IdentityRole("User"));
            if (!createRoleResult.Succeeded)
                return AccountOperationResult.Failed(createRoleResult.Errors.Select(x => x.Description).ToArray());
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            PreferredLanguage = "ru",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) return AccountOperationResult.Failed(result.Errors.Select(x => x.Description).ToArray());

        if (!await userManager.IsInRoleAsync(user, "User"))
        {
            var roleResult = await userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
                return AccountOperationResult.Failed(roleResult.Errors.Select(x => x.Description).ToArray());
        }

        return AccountOperationResult.Success("Регистрация завершена. Теперь вы можете войти.");
    }

    public async Task<AccountOperationResult> ConfirmEmailAsync(
        string userId,
        string code,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return AccountOperationResult.Failed("Пользователь не найден.");

        var result = await userManager.ConfirmEmailAsync(user, code);
        return result.Succeeded
            ? AccountOperationResult.Success("Email подтверждён.")
            : AccountOperationResult.Failed("Не удалось подтвердить email.");
    }

    public async Task<AccountSignInResult> TwoFactorAuthenticatorSignInAsync(
        string code,
        bool rememberMe,
        bool rememberMachine,
        CancellationToken ct = default)
    {
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(code, rememberMe, rememberMachine);
        return new AccountSignInResult(
            result.Succeeded,
            result.RequiresTwoFactor,
            result.IsLockedOut,
            result.Succeeded ? null : "Неверный код подтверждения.");
    }

    public Task SignOutAsync(CancellationToken ct = default)
    {
        return signInManager.SignOutAsync();
    }
}
