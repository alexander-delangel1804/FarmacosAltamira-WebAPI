using System.ComponentModel.DataAnnotations;

namespace FarmacosAltamira.Application.Seguridad.Dto;

public sealed record LoginRequest(
    [Required] string NombreUsuario,
    [Required] string Contrasena,
    [Required] int IdEstacion,
    string? SecretoEstacion);