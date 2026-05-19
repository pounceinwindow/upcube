using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Helpers;
using UpperCube.Web.Mapping;

namespace UpperCube.Web.Controllers;

public sealed class PropertyController(
    IPropertyRepository repository,
    IFavoriteRepository favoriteRepository,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IInquiryRepository inquiryRepository) : Controller
{
    private const int InquiryMessageMaxLength = 2000;

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendInquiry(
        [FromForm] int propertyId,
        [FromForm] string? message,
        CancellationToken ct)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var property = await repository.GetByIdAsync(propertyId, ct);
        if (property is null) return NotFound();

        var normalizedMessage = message?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMessage))
        {
            TempData["Error"] = "Введите сообщение перед отправкой заявки.";
            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        if (normalizedMessage.Length > InquiryMessageMaxLength)
        {
            TempData["Error"] = $"Сообщение слишком длинное. Максимум {InquiryMessageMaxLength} символов.";
            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        var now = DateTime.UtcNow;
        var inquiry = new Inquiry()
        {
            PropertyId = propertyId,
            FromUserId = userId,
            InitialMessage = normalizedMessage,
            CreatedAt = now,
            UpdatedAt = now,
            Status = InquiryStatus.Open,
            Messages = new List<Message>
            {
                new()
                {
                    SenderId = userId,
                    Text = normalizedMessage,
                    SentAt = now
                }
            }
        };

        await inquiryRepository.AddAsync(inquiry, ct);
        await unitOfWork.SaveChangesAsync(ct);

        TempData["Success"] = "Заявка отправлена агенту.";
        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var model = await repository
            .GetByIdAsync(id, ct);

        if (model == null) return NotFound();

        await repository.IncrementViewsAsync(id, ct);

        var details = model.ToDetailsDto();

        await FavoritesViewDataHelper.PopulateFavoriteIdsAsync(this, favoriteRepository, userManager, ct);
        return View(details);
    }
}
