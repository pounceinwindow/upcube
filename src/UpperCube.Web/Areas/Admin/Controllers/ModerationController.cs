using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Areas.Admin.Models.Moderation;

namespace UpperCube.Web.Areas.Admin.Controllers;

public sealed class ModerationController(
    IPropertyRepository propertyRepository,
    IModerationRepository moderationRepository,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var properties = await propertyRepository.GetByStatusAsync(PropertyStatus.PendingModeration, ct);
        var items = properties.Select(x => new AdminModerationItemViewModel
        {
            Id = x.Id,
            Title = x.Title,
            AgentId = x.AgentId,
            Location = string.Join(", ", new[] { x.City?.Name, x.District?.Name }
                .Where(y => !string.IsNullOrWhiteSpace(y))),
            Price = x.Price.Amount,
            Currency = x.Price.Currency,
            Status = x.Status.ToString(),
            CreatedAt = x.CreatedAt
        }).ToList();

        return View(new AdminModerationIndexViewModel { Items = items });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var property = await propertyRepository.GetByIdAsync(id, ct);
        if (property is null) return NotFound();

        property.Approve(DateTime.UtcNow);
        await propertyRepository.UpdateAsync(property, ct);
        await AddModerationActionAsync(property.Id, ModerationActionType.Approved, null, ct);
        await unitOfWork.SaveChangesAsync(ct);

        TempData["Success"] = "Объект опубликован.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason, CancellationToken ct)
    {
        var property = await propertyRepository.GetByIdAsync(id, ct);
        if (property is null) return NotFound();

        property.Reject();
        await propertyRepository.UpdateAsync(property, ct);
        await AddModerationActionAsync(property.Id, ModerationActionType.Rejected, reason, ct);
        await unitOfWork.SaveChangesAsync(ct);

        TempData["Success"] = "Объект отклонён.";
        return RedirectToAction(nameof(Index));
    }

    private async Task AddModerationActionAsync(
        int propertyId,
        ModerationActionType action,
        string? reason,
        CancellationToken ct)
    {
        var moderatorId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(moderatorId)) return;

        await moderationRepository.AddAsync(new ModerationAction
        {
            PropertyId = propertyId,
            ModeratorId = moderatorId,
            Action = action,
            Reason = reason
        }, ct);
    }
}
