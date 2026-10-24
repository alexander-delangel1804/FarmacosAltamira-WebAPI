namespace FarmacosAltamira.Application.Caja.Dto;

public sealed record CorteDto(
    int IdCorte,
    int IdCaja,
    string CajaNumero,
    int IdUsuario,
    string UsuarioNombre,
    DateTimeOffset FechaApertura,
    DateTimeOffset? FechaCierre,
    decimal FondoInicial,
    decimal? TotalSistema,
    decimal? TotalContado,
    decimal? Diferencia,
    string Estado,
    Guid? UuidOperacion);

public sealed record CorteAperturaDto(
    int IdCaja,
    decimal FondoInicial,
    Guid UuidOperacion);

public sealed record CorteCierreDto(
    decimal TotalContado,
    IReadOnlyList<PagoResumenDto> PagosDesglose);

public sealed record PagoResumenDto(
    string FormaPago,
    decimal Monto);

public sealed record VentaCreateDto(
    Guid UuidOperacion,
    string Folio,
    int IdCorte,
    DateTimeOffset FechaHora,
    decimal Subtotal,
    decimal Descuento,
    decimal Iva,
    decimal Total,
    bool RequiereFactura,
    IReadOnlyList<DetalleVentaCreateDto> Detalles,
    IReadOnlyList<PagoCreateDto> Pagos,
    RecetaCreateDto? Receta);

public sealed record DetalleVentaCreateDto(
    int IdProducto,
    int IdLote,
    int? IdPromocion,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Descuento,
    decimal Importe);

public sealed record PagoCreateDto(
    string FormaPago,
    decimal Monto,
    string? Referencia,
    decimal Cambio);

public sealed record RecetaCreateDto(
    string FolioReceta,
    string TipoReceta,
    DateOnly FechaExpedicion,
    string NombreMedico,
    string CedulaMedico,
    string? Institucion,
    string NombrePaciente);

public sealed record VentaResultadoDto(
    string Resultado,
    int IdVenta);

public sealed record DevolucionCreateDto(
    int IdVenta,
    string Motivo,
    IReadOnlyList<DetalleDevolucionCreateDto> Detalles,
    decimal MontoReembolsado,
    string? FormaReembolso);

public sealed record DetalleDevolucionCreateDto(
    int IdDetalleVenta,
    int Cantidad,
    bool ReingresaInventario);

public sealed record CancelacionCreateDto(
    int IdVenta,
    string Motivo);