namespace UpperCube.Application.Abstractions.Authentication;

public interface IAccountService
{
    Task<AccountSignInResult> PasswordSignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken ct = default);

    Task<AccountOperationResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken ct = default);

    Task<AccountOperationResult> ConfirmEmailAsync(string userId, string code, CancellationToken ct = default);

    Task<AccountSignInResult> TwoFactorAuthenticatorSignInAsync(
        string code,
        bool rememberMe,
        bool rememberMachine,
        CancellationToken ct = default);

    Task SignOutAsync(CancellationToken ct = default);
}

public sealed record AccountSignInResult(
    bool Succeeded,
    bool RequiresTwoFactor = false,
    bool IsLockedOut = false,
    string? ErrorMessage = null);

public sealed record AccountOperationResult(
    bool Succeeded,
    IReadOnlyList<string> Errors,
    string? Message = null)
{
    public static AccountOperationResult Success(string? message = null)
    {
        return new AccountOperationResult(true, [], message);
    }

    public static AccountOperationResult Failed(params string[] errors)
    {
        return new AccountOperationResult(false, errors);
    }
}