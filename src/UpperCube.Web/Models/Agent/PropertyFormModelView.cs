using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UpperCube.Web.Models.Agent;

public class PropertyFormModelView
{
    [Required] [MinLength(3)] public string Title { get; set; } = string.Empty;

    [Required] [MinLength(10)] public string Description { get; set; } = string.Empty;

    [Range(1,1000000000)]public decimal Price { get; set; } = decimal.Zero;

    [Required] public string Currency { get; set; } = string.Empty;
    public decimal Area { get; set; } = decimal.Zero;
    public int Rooms { get; set; } = 0;
    public int Floor { get; set; } = 0;
    public int TotalFloors { get; set; } = 0;
    public int TransactionType { get; set; } = 0;
    public int CityId { get; set; } = 0;
    public int DistrictId { get; set; } = 0;
    public int PropertyTypeId { get; set; } = 0;
    public int CategoryId { get; set; } = 0;
    public string Address { get; set; } = string.Empty;
    public IEnumerable<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Districts { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> PropertyTypes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
}