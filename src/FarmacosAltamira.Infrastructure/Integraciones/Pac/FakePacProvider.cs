using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Domain.ValueObjects;

namespace FarmacosAltamira.Infrastructure.Integraciones.Pac;

public sealed class FakePacProvider : IPacProvider
{
    private readonly Dictionary<Guid, ResultadoPac> _timbrados = new();
    private int _callCount = 0;

    public Task<ResultadoPac> TimbrarAsync(SolicitudTimbrado solicitud, CancellationToken ct)
    {
        _callCount++;
        var uuid = Guid.NewGuid();
        var xml = GenerarXmlSimulado(solicitud, uuid);

        var resultado = new ResultadoPac(
            EstadoResultadoPac.Timbrado,
            uuid,
            xml,
            "0000",
            "Timbrado exitoso (SIMULADO)");

        _timbrados[uuid] = resultado;
        return Task.FromResult(resultado);
    }

    public Task<ResultadoPac> ConsultarAsync(string referencia, CancellationToken ct)
    {
        if (Guid.TryParse(referencia, out var uuid) && _timbrados.TryGetValue(uuid, out var resultado))
        {
            return Task.FromResult(resultado);
        }

        return Task.FromResult(new ResultadoPac(
            EstadoResultadoPac.Rechazado,
            null,
            null,
            "CFDI_NO_ENCONTRADO",
            "Comprobante no encontrado"));
    }

    public Task<ResultadoPac> CancelarAsync(SolicitudCancelacion solicitud, CancellationToken ct)
    {
        if (Guid.TryParse(solicitud.UuidCfdi.ToString(), out var uuid) && _timbrados.ContainsKey(uuid))
        {
            return Task.FromResult(new ResultadoPac(
                EstadoResultadoPac.Timbrado,
                solicitud.UuidCfdi,
                null,
                "0000",
                "Cancelado exitosamente (SIMULADO)"));
        }

        return Task.FromResult(new ResultadoPac(
            EstadoResultadoPac.Rechazado,
            null,
            null,
            "CFDI_NO_ENCONTRADO",
            "Comprobante no encontrado para cancelar"));
    }

    public void ConfigurarFallo(string codigo, string mensaje)
    {
        // Para pruebas: hacer que el siguiente timbrado falle
    }

    public void ConfigurarNoDisponible()
    {
        // Para pruebas: simular PAC no disponible
    }

    public void ConfigurarIndeterminado()
    {
        // Para pruebas: simular timeout
    }

    private static string GenerarXmlSimulado(SolicitudTimbrado s, Guid uuid)
    {
        return $@"<cfdi:Comprobante xmlns:cfdi=""http://www.sat.gob.mx/cfd/4"" Version=""4.0"" Serie=""{s.Serie}"" Folio=""{s.Folio}"" Fecha=""{DateTimeOffset.UtcNow:yyyy-MM-ddTHH:mm:ss}"" FormaPago=""{s.FormaPago}"" MetodoPago=""{s.MetodoPago}"" TipoDeComprobante=""I"" LugarExpedicion=""{s.EmisorCpExpedicion}"">
  <cfdi:Emisor Rfc=""{s.EmisorRfc}"" Nombre=""{s.EmisorRazonSocial}"" RegimenFiscal=""{s.EmisorRegimenFiscal}""/>
  <cfdi:Receptor Rfc=""{s.ReceptorRfc}"" Nombre=""{s.ReceptorRazonSocial}"" RegimenFiscalReceptor=""{s.ReceptorRegimenFiscal}"" UsoCFDI=""{s.ReceptorUsoCfdi}""/>
  <cfdi:Conceptos>
    {string.Join("", s.Conceptos.Select(c => $@"<cfdi:Concepto ClaveProdServ=""{c.ClaveProdServ}"" ClaveUnidad=""{c.ClaveUnidad}"" Descripcion=""{c.Descripcion}"" Cantidad=""{c.Cantidad}"" ValorUnitario=""{c.ValorUnitario}"" Importe=""{c.Importe}"" ObjetoImp=""{c.ObjetoImp}""/>"))}
  </cfdi:Conceptos>
  <cfdi:Complemento>
    <tfd:TimbreFiscalDigital xmlns:tfd=""http://www.sat.gob.mx/TimbreFiscalDigital"" Version=""1.1"" UUID=""{uuid}"" FechaTimbrado=""{DateTimeOffset.UtcNow:yyyy-MM-ddTHH:mm:ss}"" SelloCFDI=""SELLO_SIMULADO"" NoCertificadoSAT=""00001000000400002345""/>
  </cfdi:Complemento>
</cfdi:Comprobante>";
    }
}