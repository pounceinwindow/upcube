using FluentValidation;
using UpperCube.Application.DTOs;

namespace UpperCube.Application.Validation;

public sealed class CreatePropertyValidator : AbstractValidator<CreatePropertyDto>
{
    public CreatePropertyValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(3, 180);
        RuleFor(x => x.Description).NotEmpty().Length(10, 5000);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Area).GreaterThan(0);
        RuleFor(x => x.Rooms).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Floor).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalFloors).GreaterThanOrEqualTo(x => x.Floor);
        RuleFor(x => x.CityId).GreaterThan(0);
        RuleFor(x => x.DistrictId).GreaterThan(0);
        RuleFor(x => x.PropertyTypeId).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Address).NotEmpty().Length(5, 256);
    }
}