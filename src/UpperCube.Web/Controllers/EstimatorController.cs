using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using UpperCube.Application.Abstractions.AI;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Application.Abstractions.Valuation;
using UpperCube.Application.DTOs;
using UpperCube.Application.UseCases.Valuation;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web;
using UpperCube.Web.Models.Estimator;

namespace UpperCube.Web.Controllers;

public sealed class EstimatorController(
    IEnumerable<IValuator> valuators,
    ILocalLlmClient localLlmClient,
    ValuationExplanationPromptBuilder promptBuilder,
    IValuationRepository valuationRepository,
    IPropertyRepository propertyRepository,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IDictionaryRepository<City> cityRepository,
    IDictionaryRepository<District> districtRepository,
    IDictionaryRepository<PropertyType> propertyTypeRepository,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = new EstimatorModelView();
        await PopulateDictionariesAsync(model, ct);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EstimatorModelView model, CancellationToken ct)
    {
        if (model.Floor.HasValue && model.TotalFloors.HasValue && model.TotalFloors.Value < model.Floor.Value)
        {
            ModelState.AddModelError(nameof(model.TotalFloors), localizer["ValidationInvalidFloor"]);
        }

        if (model.Area.HasValue && model.Area.Value <= 0)
        {
            ModelState.AddModelError(nameof(model.Area), localizer["ValidationAreaPositive"]);
        }

        if (!ModelState.IsValid)
        {
            await PopulateDictionariesAsync(model, ct);
            return View(model);
        }

        var city = await cityRepository.GetByIdAsync(model.CityId!.Value, ct);
        var district = await districtRepository.GetByIdAsync(model.DistrictId!.Value, ct);
        var propertyType = await propertyTypeRepository.GetByIdAsync(model.PropertyTypeId!.Value, ct);

        if (city is null) ModelState.AddModelError(nameof(model.CityId), localizer["ValidationRequired"]);
        if (district is null) ModelState.AddModelError(nameof(model.DistrictId), localizer["ValidationRequired"]);
        if (propertyType is null) ModelState.AddModelError(nameof(model.PropertyTypeId), localizer["ValidationRequired"]);
        if (district is not null && district.CityId != model.CityId)
        {
            ModelState.AddModelError(nameof(model.DistrictId), localizer["ValidationDistrictCity"]);
        }

        if (!ModelState.IsValid)
        {
            await PopulateDictionariesAsync(model, ct);
            return View(model);
        }

        var floor = model.Floor ?? 0;
        var totalFloors = model.TotalFloors ?? Math.Max(floor, 0);
        var valuationRequest = new ValuationRequest(
            model.CityId.Value,
            model.DistrictId.Value,
            model.PropertyTypeId.Value,
            model.Area!.Value,
            model.Rooms!.Value,
            floor,
            totalFloors);

        var valuator = GetCompositeValuator();
        var valuationResult = await valuator.EstimateAsync(valuationRequest, ct);
        var districts = await districtRepository.ListAsync(ct);
        var districtNames = districts.ToDictionary(x => x.Id, x => x.Name);
        var comparables = await BuildComparablesAsync(valuationRequest, districtNames, ct);
        var culture = CultureInfo.CurrentUICulture.Name;
        var context = new ValuationExplanationContext(
            culture,
            city!.Name,
            district!.Name,
            propertyType!.Name,
            valuationRequest.Area,
            valuationRequest.Rooms,
            model.Floor,
            model.TotalFloors,
            valuationResult.Min,
            valuationResult.Max,
            valuationResult.Currency,
            valuator.Name,
            comparables);

        var prompt = promptBuilder.Build(context);
        var aiResult = await localLlmClient.GenerateAsync(prompt, ct);

        model.EstimatedMin = valuationResult.Min;
        model.EstimatedMax = valuationResult.Max;
        model.Currency = valuationResult.Currency;
        model.StrategyUsed = valuator.Name;
        model.Comparables = comparables;

        if (aiResult.IsSuccess)
        {
            model.AiExplanation = aiResult.Text;
        }
        else
        {
            model.AiExplanationUnavailable = true;
            model.AiErrorMessage = aiResult.ErrorMessage;
        }

        var userId = userManager.GetUserId(User);
        await valuationRepository.AddAsync(new Valuation
        {
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            InputJson = BuildInputJson(model, context),
            EstimatedMin = valuationResult.Min,
            EstimatedMax = valuationResult.Max,
            Currency = valuationResult.Currency,
            StrategyUsed = valuator.Name
        }, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await PopulateDictionariesAsync(model, ct);
        return View(model);
    }

    private IValuator GetCompositeValuator()
    {
        return valuators.FirstOrDefault(x => string.Equals(x.Name, "Composite", StringComparison.OrdinalIgnoreCase))
               ?? valuators.Last();
    }

    private async Task PopulateDictionariesAsync(EstimatorModelView model, CancellationToken ct)
    {
        var cities = await cityRepository.ListAsync(ct);
        var districts = await districtRepository.ListAsync(ct);
        var propertyTypes = await propertyTypeRepository.ListAsync(ct);

        model.Cities = cities.Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.CityId)).ToList();
        model.Districts = districts.Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.DistrictId))
            .ToList();
        model.PropertyTypes = propertyTypes
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.PropertyTypeId))
            .ToList();
    }

    private async Task<IReadOnlyList<ComparablePropertyContext>> BuildComparablesAsync(
        ValuationRequest request,
        IReadOnlyDictionary<int, string> districtNames,
        CancellationToken ct)
    {
        var minArea = request.Area * 0.7m;
        var maxArea = request.Area * 1.3m;
        var (items, _) = await propertyRepository.SearchAsync(
            new PropertySearchFilter(
                request.CityId,
                PropertyTypeId: request.PropertyTypeId,
                Status: (int)PropertyStatus.Published,
                MinArea: minArea,
                MaxArea: maxArea),
            1,
            100,
            ct);

        return items
            .Where(x => x.Area.Value > 0 && Math.Abs(x.Rooms - request.Rooms) <= 1)
            .OrderBy(x => SimilarityScore(x, request))
            .Take(5)
            .Select(x => new ComparablePropertyContext(
                x.Id,
                x.Title,
                districtNames.GetValueOrDefault(x.DistrictId),
                x.Area.Value,
                x.Price.Amount,
                decimal.Round(x.Price.Amount / x.Area.Value, 2),
                x.Rooms))
            .ToList();
    }

    private static decimal SimilarityScore(Property property, ValuationRequest request)
    {
        return Math.Abs(property.Area.Value - request.Area)
               + Math.Abs(property.Rooms - request.Rooms) * 10
               + Math.Abs(property.Floor - request.Floor) * 2
               + Math.Abs(property.TotalFloors - request.TotalFloors);
    }

    private static string BuildInputJson(EstimatorModelView model, ValuationExplanationContext context)
    {
        return JsonSerializer.Serialize(new
        {
            Culture = context.Culture,
            model.CityId,
            context.CityName,
            model.DistrictId,
            context.DistrictName,
            model.PropertyTypeId,
            context.PropertyTypeName,
            model.Area,
            model.Rooms,
            model.Floor,
            model.TotalFloors,
            context.EstimatedMin,
            context.EstimatedMax,
            context.Currency,
            context.StrategyUsed,
            Comparables = context.Comparables.Select(x => new
            {
                x.PropertyId,
                x.Title,
                x.DistrictName,
                x.Area,
                x.Price,
                x.PricePerSquareMeter,
                x.Rooms
            })
        });
    }
}
