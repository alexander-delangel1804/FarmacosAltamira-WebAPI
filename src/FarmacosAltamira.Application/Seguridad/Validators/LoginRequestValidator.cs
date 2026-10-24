using FarmacosAltamira.Application.Seguridad.Dto;
using FluentValidation;

namespace FarmacosAltamira.Application.Seguridad.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.NombreUsuario).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Contrasena).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IdEstacion).GreaterThan(0);
        RuleFor(x => x.SecretoEstacion).NotEmpty().When(x => x.IdEstacion > 0);
    }
}