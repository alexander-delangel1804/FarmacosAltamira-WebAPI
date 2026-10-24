using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.ValueObjects;

public sealed record ResultadoPac(
    EstadoResultadoPac Estado,
    Guid? UuidCfdi,
    string? Xml,
    string? CodigoPac,
    string? Mensaje);

public sealed record SolicitudTimbrado(
    int IdVenta,
    string EmisorRfc,
    string EmisorRazonSocial,
    string EmisorRegimenFiscal,
    string EmisorCpExpedicion,
    string Serie,
    string Folio,
    string ReceptorRfc,
    string ReceptorRazonSocial,
    string ReceptorRegimenFiscal,
    string ReceptorCp,
    string ReceptorUsoCfdi,
    IReadOnlyList<ConceptoTimbrado> Conceptos,
    IReadOnlyList<PagoTimbrado> Pagos,
    string MetodoPago,
    string FormaPago);

public sealed record ConceptoTimbrado(
    string ClaveProdServ,
    string ClaveUnidad,
    string Descripcion,
    decimal Cantidad,
    decimal ValorUnitario,
    decimal Importe,
    string ObjetoImp,
    decimal? TasaIva,
    decimal? TasaIeps);

public sealed record PagoTimbrado(
    string FormaPago,
    decimal Monto);

public sealed record ResultadoServicio(
    EstadoResultadoServicio Estado,
    string? FolioAutorizacion,
    string? CodigoProveedor,
    string? Mensaje);

public sealed record SolicitudPagoServicio(
    int IdCatalogoServicio,
    string ReferenciaCliente,
    Dinero Monto,
    Dinero Comision,
    int IdUbicacion,
    int IdUsuario,
    int IdCorte);

public sealed record ConsultaAdeudo(
    int IdCatalogoServicio,
    string ReferenciaCliente);

public sealed record SolicitudReverso(
    int IdPagoServicio,
    string Motivo);