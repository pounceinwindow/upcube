using Microsoft.AspNetCore.Mvc.Rendering;
using UpperCube.Application.DTOs;

namespace UpperCube.Web.Models.Catalog;

public class CatalogModelView
{
    public IEnumerable<PropertyListItemDto> Items { get; set; }
    
    public int? CityId { get; set; }
    public int? PropertyTypeId { get; set; }
    public int? TransactionType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Rooms { get; set; }
    public int Page { get; set; }
    public int TotalCount { get; set; }
    
    public IEnumerable<SelectListItem>? Cities { get; set; }
    public IEnumerable<SelectListItem>? PropertyTypes { get; set; }
}