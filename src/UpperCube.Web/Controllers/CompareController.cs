using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Helpers;
using UpperCube.Web.Models.Compare;

namespace UpperCube.Web.Controllers;

public sealed class CompareController(
    IComparisonRepository comparisonRepository,
    IPropertyRepository propertyRepository,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    private const string DefaultComparisonName = "Default";
    private const string CompareSuccessKey = "CompareSuccess";
    private const string CompareInfoKey = "CompareInfo";
    private const string CompareErrorKey = "CompareError";

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return View(new CompareModelView { IsAuthenticated = false });

        var comparison = await comparisonRepository.GetByUserIdAsync(userId, ct);
        return View(new CompareModelView
        {
            IsAuthenticated = true,
            Items = comparison?.Items
                .Where(x => x.Property is not null)
                .OrderBy(x => x.Position)
                .Select(ToModel)
                .ToList() ?? []
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int propertyId, string? returnUrl, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var property = await propertyRepository.GetByIdAsync(propertyId, ct);
        if (property is null || property.Status != PropertyStatus.Published) return NotFound();

        var comparison = await GetOrCreateComparisonAsync(userId, ct);

        if (comparison.Items.Any(x => x.PropertyId == propertyId))
        {
            SetTempDataMessage(CompareInfoKey, "CompareAlreadyAdded");
            return RedirectBack(returnUrl);
        }

        if (comparison.Items.Count >= CompareModelView.MaxItems)
        {
            SetTempDataMessage(CompareErrorKey, "CompareLimitReached");
            return RedirectBack(returnUrl);
        }

        comparison.Items.Add(new ComparisonItem
        {
            PropertyId = propertyId,
            Position = GetFirstAvailablePosition(comparison)
        });

        await unitOfWork.SaveChangesAsync(ct);
        SetTempDataMessage(CompareSuccessKey, "CompareAdded");
        return RedirectBack(returnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int propertyId, string? returnUrl, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var comparison = await comparisonRepository.GetByUserIdAsync(userId, ct);
        var item = comparison?.Items.FirstOrDefault(x => x.PropertyId == propertyId);
        if (item is not null)
        {
            comparisonRepository.RemoveItem(item);
            await unitOfWork.SaveChangesAsync(ct);
            SetTempDataMessage(CompareSuccessKey, "CompareRemoved");
        }

        return RedirectBack(returnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var comparison = await comparisonRepository.GetByUserIdAsync(userId, ct);
        if (comparison is not null && comparison.Items.Count > 0)
        {
            comparisonRepository.RemoveItems(comparison.Items.ToList());
            await unitOfWork.SaveChangesAsync(ct);
            SetTempDataMessage(CompareSuccessKey, "CompareCleared");
        }

        return RedirectToAction(nameof(Index));
    }

    private string? GetCurrentUserId()
    {
        return userManager.GetUserId(User);
    }

    private async Task<Comparison> GetOrCreateComparisonAsync(string userId, CancellationToken ct)
    {
        var comparison = await comparisonRepository.GetByUserIdAsync(userId, ct);
        if (comparison is not null) return comparison;

        comparison = new Comparison
        {
            UserId = userId,
            Name = DefaultComparisonName
        };

        await comparisonRepository.AddAsync(comparison, ct);
        return comparison;
    }

    private static int GetFirstAvailablePosition(Comparison comparison)
    {
        var usedPositions = comparison.Items.Select(x => x.Position).ToHashSet();
        return Enumerable.Range(1, CompareModelView.MaxItems).First(x => !usedPositions.Contains(x));
    }

    private void SetTempDataMessage(string key, string localizerKey)
    {
        TempData[key] = localizer[localizerKey].ToString();
    }

    private IActionResult RedirectBack(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    private static CompareItemModelView ToModel(ComparisonItem item)
    {
        var property = item.Property!;

        return new CompareItemModelView
        {
            Position = item.Position,
            Id = property.Id,
            Title = property.Title,
            Price = property.Price.Amount,
            Currency = property.Price.Currency,
            Area = property.Area.Value,
            Rooms = property.Rooms,
            Floor = property.Floor,
            TotalFloors = property.TotalFloors,
            City = property.City?.Name ?? string.Empty,
            District = property.District?.Name ?? string.Empty,
            PropertyType = property.PropertyType?.Name ?? string.Empty,
            Address = property.Address,
            PrimaryImagePath = PropertyImagePathHelper.GetPrimaryOrFirstPath(property.Images)
        };
    }
}
