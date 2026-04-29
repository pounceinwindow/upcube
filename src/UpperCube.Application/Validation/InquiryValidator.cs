using FluentValidation;
using UpperCube.Application.DTOs;

namespace UpperCube.Application.Validation;

public sealed class InquiryValidator : AbstractValidator<InquiryDto>
{
    public InquiryValidator()
    {
        RuleFor(x => x.InitialMessage).NotEmpty().Length(5, 2000);
        RuleFor(x => x.PropertyId).GreaterThan(0);
    }
}