using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Authentication;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Infrastructure.Identity;
using UpperCube.Models.Account;
using UpperCube.Web.Mapping;

namespace UpperCube.Web.Controllers;

[Route("account")]
public sealed class AccountController(
    IAccountService accountService,
    UserManager<ApplicationUser> userManager) : Controller
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
        NormalizeRememberMeModelState(model);

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

    private void NormalizeRememberMeModelState(LoginViewModel model)
    {
        if (!ModelState.TryGetValue(nameof(LoginViewModel.RememberMe), out var rememberMeState) ||
            !string.Equals(rememberMeState.AttemptedValue, "on", StringComparison.OrdinalIgnoreCase))
            return;

        model.RememberMe = true;
        ModelState.Remove(nameof(LoginViewModel.RememberMe));
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
        NormalizeAcceptTermsModelState(model);

        if (!model.AcceptTerms)
            ModelState.AddModelError(nameof(RegisterViewModel.AcceptTerms), "Необходимо принять условия сервиса.");

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

    private void NormalizeAcceptTermsModelState(RegisterViewModel model)
    {
        if (!ModelState.TryGetValue(nameof(RegisterViewModel.AcceptTerms), out var acceptTermsState) ||
            !string.Equals(acceptTermsState.AttemptedValue, "on", StringComparison.OrdinalIgnoreCase))
            return;

        model.AcceptTerms = true;
        ModelState.Remove(nameof(RegisterViewModel.AcceptTerms));
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

    [Authorize]
    [HttpGet("favorites")]
    public async Task<IActionResult> Favorites(
        [FromServices] IFavoriteRepository favoriteRepository,
        CancellationToken ct)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var properties = await favoriteRepository.GetUserFavoritesAsync(userId, ct);
        var items = properties.Select(p => p.ToListItemDto()).ToList();

        ViewData["FavoriteIds"] = items.Select(i => i.Id).ToHashSet();

        return View(items);
    }

    [Authorize]
    [HttpGet("/account/dashboard")]
    public async Task<IActionResult> Dashboard(
        [FromServices] IFavoriteRepository favoriteRepository,
        [FromServices] IPropertyRepository propertyRepository,
        [FromServices] IInquiryRepository inquiryRepository,
        CancellationToken ct)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var favoriteIds = await favoriteRepository.GetUserFavoritePropertyIdsAsync(userId, ct);
        var inquiries = await inquiryRepository.GetByUserIdAsync(userId, ct);

        var myPropertiesCount = 0;
        var agentInquiriesCount = 0;
        if (User.IsInRole("Agent"))
        {
            var myProps = await propertyRepository.GetByAgentIdAsync(userId, ct);
            myPropertiesCount = myProps.Count;

            var agentInquiries = await inquiryRepository.GetByAgentIdAsync(userId, ct);
            agentInquiriesCount = agentInquiries.Count;
        }

        ViewData["FavoritesCount"] = favoriteIds.Count;
        ViewData["InquiriesCount"] = inquiries.Count;
        ViewData["MyPropertiesCount"] = myPropertiesCount;
        ViewData["AgentInquiriesCount"] = agentInquiriesCount;
        ViewData["IsAgent"] = User.IsInRole("Agent");

        return View();
    }

    [Authorize]
    [HttpGet("inquiries")]
    public async Task<IActionResult> Inquiries(
        [FromServices] IInquiryRepository inquiryRepository,
        CancellationToken ct)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var inquiries = await inquiryRepository.GetByUserIdAsync(userId, ct);
        var model = inquiries.Select(x => x.ToListItemModelView()).ToList();

        return View(model);
    }

    [Authorize]
    [HttpGet("/account/profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        return View(user);
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
