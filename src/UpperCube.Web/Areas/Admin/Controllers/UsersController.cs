using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Areas.Admin.Models.Users;

namespace UpperCube.Web.Areas.Admin.Controllers;

public sealed class UsersController(
    UserManager<ApplicationUser> userManager,
    ILogger<UsersController> logger) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var users = await userManager.Users
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var items = new List<AdminUserListItemViewModel>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            items.Add(ToListItem(user, roles));
        }

        return View(new AdminUsersIndexViewModel { Users = items });
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return View(ToListItem(user, roles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVerified(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.IsVerified = !user.IsVerified;
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = user.IsVerified
                ? "Пользователь отмечен как проверенный."
                : "Проверка пользователя снята.";
        }
        else
        {
            logger.LogWarning("Failed to update user verification state for {UserId}: {Errors}",
                id,
                string.Join("; ", result.Errors.Select(x => x.Description)));
            TempData["Error"] = "Не удалось обновить пользователя.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static AdminUserListItemViewModel ToListItem(ApplicationUser user, IEnumerable<string> roles)
    {
        var fullName = string.Join(' ', new[] { user.FirstName, user.LastName }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        return new AdminUserListItemViewModel
        {
            Id = user.Id,
            Email = user.Email ?? user.UserName ?? user.Id,
            FullName = fullName,
            Roles = roles.OrderBy(x => x).ToList(),
            EmailConfirmed = user.EmailConfirmed,
            IsVerified = user.IsVerified,
            IsBlocked = user.IsBlocked,
            CreatedAt = user.CreatedAt
        };
    }
}