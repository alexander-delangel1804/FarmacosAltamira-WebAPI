using FarmacosAltamira.Application.Seguridad.Dto;
using FluentValidation;

namespace FarmacosAltamira.Application.Seguridad.Validators;

public sealed class UsuarioCreateDtoValidator : AbstractValidator<UsuarioCreateDto>
{
    public UsuarioCreateDtoValidator()
    {
        RuleFor(x => x.NombreUsuario).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Contrasena).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.IdRol).GreaterThan(0);
        RuleFor(x => x.IdUbicacion).GreaterThan(0).When(x => x.IdUbicacion.HasValue);
    }
}

public sealed class UsuarioUpdateDtoValidator : AbstractValidator<UsuarioUpdateDto>
{
    public UsuarioUpdateDtoValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IdRol).GreaterThan(0);
        RuleFor(x => x.IdUbicacion).GreaterThan(0).When(x => x.IdUbicacion.HasValue);
    }
}

public sealed class CambiarContrasenaDtoValidator : AbstractValidator<CambiarContrasenaDto>
{
    public CambiarContrasenaDtoValidator()
    {
        RuleFor(x => x.ContrasenaActual).NotEmpty();
        RuleFor(x => x.NuevaContrasena).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}