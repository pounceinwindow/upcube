using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpperCube.Application.Abstractions.Media;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Domain.ValueObjects;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Mapping;
using UpperCube.Web.Models.Agent;

namespace UpperCube.Web.Controllers;

[Authorize(Roles = "Agent")]
[Route("agent")]
public sealed class AgentController(
    IPropertyRepository repository,
    UserManager<ApplicationUser> userManager,
    IUnitOfWork unitOfWork,
    IImageStorage imageStorage,
    IDictionaryRepository<City> city,
    IDictionaryRepository<District> district,
    IDictionaryRepository<PropertyType> propertyType,
    IDictionaryRepository<Category> category) : Controller
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    [HttpGet("properties")]
    public async Task<IActionResult> MyProperties(CancellationToken ct)
    {
        var agentId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(agentId)) return Challenge();

        var property = await repository.GetByAgentIdAsync(agentId, ct);
        var propertyListItem = property
            .Select(x => x.ToListItemDto()).ToList();
        return View(propertyListItem);
    }

    [HttpGet("inquiries")]
    public async Task<IActionResult> Inquiries(
        [FromServices] IInquiryRepository inquiryRepository,
        CancellationToken ct)
    {
        var agentId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(agentId)) return Challenge();

        var inquiries = await inquiryRepository.GetByAgentIdAsync(agentId, ct);
        var model = inquiries.Select(x => x.ToListItemModelView()).ToList();

        return View(model);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var create = new PropertyFormModelView();
        await PopulateDictionariesAsync(create, ct);
        return View(create);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("create")]
    public async Task<IActionResult> Create(PropertyFormModelView model, CancellationToken ct)
    {
        ValidateImageFiles(model.ImageFiles);

        if (!ModelState.IsValid)
        {
            await PopulateDictionariesAsync(model, ct);
            return View(model);
        }

        var agentId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(agentId)) return Challenge();

        var property = new Property
        {
            AgentId = agentId
        };

        ApplyFormModel(property, model);
        property.Submit();
        await AppendUploadedImagesAsync(property, model.ImageFiles, ct);

        await repository.AddAsync(property, ct);
        await unitOfWork.SaveChangesAsync(ct);

        TempData["AgentMessage"] = "Объявление создано и отправлено на модерацию.";
        return RedirectToAction(nameof(MyProperties));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var property = await repository.GetByIdAsync(id, ct);
        if (property is null) return NotFound();

        var agentId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(agentId)) return Challenge();
        if (!IsOwner(property, agentId)) return Forbid();

        var model = ToFormModel(property);
        await PopulateDictionariesAsync(model, ct);

        return View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, PropertyFormModelView model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();

        var property = await repository.GetByIdAsync(id, ct);
        if (property is null) return NotFound();

        var agentId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(agentId)) return Challenge();
        if (!IsOwner(property, agentId)) return Forbid();

        model.ExistingImages = property.Images.OrderBy(x => x.Order).Select(x => x.Path).ToList();
        ValidateImageFiles(model.ImageFiles);

        if (!ModelState.IsValid)
        {
            await PopulateDictionariesAsync(model, ct);
            return View(model);
        }

        ApplyFormModel(property, model);
        await AppendUploadedImagesAsync(property, model.ImageFiles, ct);

        await repository.UpdateAsync(property, ct);
        await unitOfWork.SaveChangesAsync(ct);

        TempData["AgentMessage"] = "Объявление обновлено.";
        return RedirectToAction(nameof(MyProperties));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var property = await repository.GetByIdAsync(id, ct);
        if (property is null) return NotFound();

        var agentId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(agentId)) return Challenge();
        if (!IsOwner(property, agentId)) return Forbid();

        foreach (var image in property.Images.Where(x => x.Path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)))
        {
            await imageStorage.DeleteAsync(image.Path, ct);
        }

        await repository.DeleteAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);

        TempData["AgentMessage"] = "Объявление удалено.";
        return RedirectToAction(nameof(MyProperties));
    }

    private async Task PopulateDictionariesAsync(PropertyFormModelView model, CancellationToken ct)
    {
        var cities = await city.ListAsync(ct);
        var districts = await district.ListAsync(ct);
        var propertyTypes = await propertyType.ListAsync(ct);
        var categories = await category.ListAsync(ct);
        model.Cities = cities.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        model.Districts = districts.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        model.PropertyTypes = propertyTypes.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        model.Categories = categories.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
    }

    private static bool IsOwner(Property property, string agentId)
    {
        return string.Equals(property.AgentId, agentId, StringComparison.Ordinal);
    }

    private static PropertyFormModelView ToFormModel(Property property)
    {
        return new PropertyFormModelView
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price.Amount,
            Currency = property.Price.Currency,
            Area = property.Area.Value,
            Rooms = property.Rooms,
            Floor = property.Floor,
            TotalFloors = property.TotalFloors,
            TransactionType = (int)property.TransactionType,
            CityId = property.CityId,
            DistrictId = property.DistrictId,
            PropertyTypeId = property.PropertyTypeId,
            CategoryId = property.CategoryId,
            Address = property.Address,
            ExistingImages = property.Images.OrderBy(x => x.Order).Select(x => x.Path).ToList()
        };
    }

    private static void ApplyFormModel(Property property, PropertyFormModelView model)
    {
        property.Title = model.Title.Trim();
        property.Description = model.Description.Trim();
        property.Price = new Money(model.Price, model.Currency.Trim().ToUpperInvariant());
        property.Area = new Area(model.Area, AreaUnit.SquareMeter);
        property.Rooms = model.Rooms;
        property.Floor = model.Floor;
        property.TotalFloors = model.TotalFloors;
        property.TransactionType = (TransactionType)model.TransactionType;
        property.CityId = model.CityId;
        property.DistrictId = model.DistrictId;
        property.PropertyTypeId = model.PropertyTypeId;
        property.CategoryId = model.CategoryId;
        property.Address = model.Address.Trim();
    }

    private void ValidateImageFiles(IEnumerable<IFormFile>? imageFiles)
    {
        foreach (var file in (imageFiles ?? []).Where(x => x.Length > 0))
        {
            if (file.Length > MaxImageSizeBytes)
            {
                ModelState.AddModelError(nameof(PropertyFormModelView.ImageFiles),
                    $"Файл {file.FileName} больше 5 МБ.");
            }

            if (!AllowedImageContentTypes.Contains(file.ContentType))
            {
                ModelState.AddModelError(nameof(PropertyFormModelView.ImageFiles),
                    $"Файл {file.FileName} должен быть JPG, PNG или WebP.");
            }
        }
    }

    private async Task AppendUploadedImagesAsync(Property property, IEnumerable<IFormFile>? imageFiles, CancellationToken ct)
    {
        var order = property.Images.Count == 0 ? 0 : property.Images.Max(x => x.Order) + 1;

        foreach (var file in (imageFiles ?? []).Where(x => x.Length > 0))
        {
            await using var stream = file.OpenReadStream();
            var path = await imageStorage.SaveAsync(stream, file.FileName, file.ContentType, ct);

            property.Images.Add(new PropertyImage
            {
                Path = path,
                IsPrimary = property.Images.All(x => !x.IsPrimary),
                Order = order++,
                UploadedAt = DateTime.UtcNow
            });
        }
    }
}
