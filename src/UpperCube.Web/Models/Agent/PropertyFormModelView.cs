using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UpperCube.Web.Models.Agent;

public class PropertyFormModelView : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название.")]
    [StringLength(180, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 180 символов.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите описание.")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Описание должно быть от 10 до 5000 символов.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 1_000_000_000, ErrorMessage = "Цена должна быть больше 0.")]
    public decimal Price { get; set; } = decimal.Zero;

    [Required(ErrorMessage = "Укажите валюту.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Валюта должна быть кодом из 3 символов.")]
    public string Currency { get; set; } = "RUB";

    [Range(1, 100_000, ErrorMessage = "Площадь должна быть больше 0.")]
    public decimal Area { get; set; } = decimal.Zero;

    [Range(1, 100, ErrorMessage = "Количество комнат должно быть больше 0.")]
    public int Rooms { get; set; } = 1;

    [Range(0, 300, ErrorMessage = "Этаж должен быть 0 или больше.")]
    public int Floor { get; set; } = 1;

    [Range(1, 300, ErrorMessage = "Количество этажей должно быть больше 0.")]
    public int TotalFloors { get; set; } = 1;

    [Range(1, 2, ErrorMessage = "Выберите тип сделки.")]
    public int TransactionType { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Выберите город.")]
    public int CityId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите район.")]
    public int DistrictId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите тип недвижимости.")]
    public int PropertyTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Введите адрес.")]
    [StringLength(256, MinimumLength = 5, ErrorMessage = "Адрес должен быть от 5 до 256 символов.")]
    public string Address { get; set; } = string.Empty;

    [ValidateNever] public List<IFormFile>? ImageFiles { get; set; }

    public IReadOnlyList<string> ExistingImages { get; set; } = [];

    public IEnumerable<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Districts { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> PropertyTypes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TotalFloors < Floor)
            yield return new ValidationResult(
                "Количество этажей не может быть меньше этажа объекта.",
                [nameof(TotalFloors)]);
    }
}