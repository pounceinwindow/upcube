using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Authentication;
using UpperCube.Models.Account;

namespace UpperCube.Web.Controllers;

[Route("account")]
public sealed class AccountController(IAccountService accountService) : Controller
{
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await accountService.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            HttpContext.RequestAborted);

        if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(TwoFactor),
                new { rememberMe = model.RememberMe, returnUrl = model.ReturnUrl });

        if (result.Succeeded) return LocalRedirect(model.ReturnUrl ?? Url.Action("Index", "Home")!);

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Неверный email или пароль.");
        return View(model);
    }

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await accountService.RegisterAsync(
            model.FirstName,
            model.LastName,
            model.Email,
            model.Password,
            HttpContext.RequestAborted);

        if (result.Succeeded)
        {
            TempData["AccountMessage"] = result.Message;
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);

        return View(model);
    }

    [HttpGet("forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ViewData["Message"] = "Если пользователь найден, ссылка для восстановления будет отправлена на email.";
        return View(model);
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            return RedirectToAction("Index", "Home");

        var result = await accountService.ConfirmEmailAsync(userId, code, HttpContext.RequestAborted);
        TempData["AccountMessage"] = result.Succeeded
            ? result.Message
            : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet("2fa")]
    [AllowAnonymous]
    public IActionResult TwoFactor(bool rememberMe = false)
    {
        return View(new TwoFactorViewModel { RememberMe = rememberMe });
    }

    [HttpPost("2fa")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(TwoFactorViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var authenticatorCode = model.Code.Replace(" ", string.Empty, StringComparison.Ordinal);
        var result = await accountService.TwoFactorAuthenticatorSignInAsync(
            authenticatorCode,
            model.RememberMe,
            model.RememberMachine,
            HttpContext.RequestAborted);

        if (result.Succeeded) return RedirectToAction("Index", "Home");

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Неверный код подтверждения.");
        return View(model);
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await accountService.SignOutAsync(HttpContext.RequestAborted);
        return RedirectToAction("Index", "Home");
    }
}