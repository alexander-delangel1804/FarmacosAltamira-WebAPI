using FarmacosAltamira.Application.Seguridad.Dto;
using FluentValidation;

namespace FarmacosAltamira.Application.Seguridad.Validators;

public sealed class RolCreateDtoValidator : AbstractValidator<RolCreateDto>
{
    public RolCreateDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Descripcion).MaximumLength(150);
        RuleFor(x => x.PermisosIds).NotEmpty();
        RuleForEach(x => x.PermisosIds).GreaterThan(0);
    }
}

public sealed class RolUpdateDtoValidator : AbstractValidator<RolUpdateDto>
{
    public RolUpdateDtoValidator()
    {
        RuleFor(x => x.Descripcion).MaximumLength(150);
        RuleFor(x => x.PermisosIds).NotEmpty();
        RuleForEach(x => x.PermisosIds).GreaterThan(0);
    }
}