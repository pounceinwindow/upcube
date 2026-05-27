using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpperCube.Application.Abstractions.Valuation;

namespace UpperCube.Web.Models.Estimator;

public class EstimatorModelView
{
    [Required(ErrorMessage = "ValidationRequired")]
    [Range(1, int.MaxValue, ErrorMessage = "ValidationRequired")]
    public int? CityId { get; set; }

    [Required(ErrorMessage = "ValidationRequired")]
    [Range(1, int.MaxValue, ErrorMessage = "ValidationRequired")]
    public int? DistrictId { get; set; }

    [Required(ErrorMessage = "ValidationRequired")]
    [Range(1, int.MaxValue, ErrorMessage = "ValidationRequired")]
    public int? PropertyTypeId { get; set; }

    [Required(ErrorMessage = "ValidationRequired")]
    public decimal? Area { get; set; }

    [Required(ErrorMessage = "ValidationRequired")]
    [Range(1, 100, ErrorMessage = "ValidationRoomsPositive")]
    public int? Rooms { get; set; }

    [Range(0, 300, ErrorMessage = "ValidationInvalidFloor")]
    public int? Floor { get; set; }

    [Range(0, 300, ErrorMessage = "ValidationInvalidFloor")]
    public int? TotalFloors { get; set; }

    public decimal? EstimatedMin { get; set; }

    public decimal? EstimatedMax { get; set; }

    public string? Currency { get; set; }

    public string? StrategyUsed { get; set; }

    public string? AiExplanation { get; set; }

    public bool AiExplanationUnavailable { get; set; }

    public IReadOnlyList<ComparablePropertyContext> Comparables { get; set; } = [];

    [ValidateNever] public IEnumerable<SelectListItem> Cities { get; set; } = [];

    [ValidateNever] public IEnumerable<SelectListItem> Districts { get; set; } = [];

    [ValidateNever] public IEnumerable<SelectListItem> PropertyTypes { get; set; } = [];
}
