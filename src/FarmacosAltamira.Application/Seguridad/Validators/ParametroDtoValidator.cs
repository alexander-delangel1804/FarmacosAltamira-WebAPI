using FarmacosAltamira.Application.Seguridad.Dto;
using FluentValidation;

namespace FarmacosAltamira.Application.Seguridad.Validators;

public sealed class ParametroUpdateDtoValidator : AbstractValidator<ParametroUpdateDto>
{
    public ParametroUpdateDtoValidator()
    {
        RuleFor(x => x.Valor).NotEmpty().MaximumLength(100);
    }
}