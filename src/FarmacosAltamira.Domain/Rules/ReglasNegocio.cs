using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Domain.ValueObjects;

namespace FarmacosAltamira.Domain.Rules;

public static class ReglasInventario
{
    public static bool TieneExistenciaDisponible(int cantidad, int cantidadReservada, int solicitado)
        => (cantidad - cantidadReservada) >= solicitado;

    public static int CalcularDisponible(int cantidad, int cantidadReservada)
        => cantidad - cantidadReservada;

    public static bool RequiereAlertaCaducidad(DateOnly fechaCaducidad, int diasAlerta)
        => fechaCaducidad <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(diasAlerta);

    public static bool RequiereAlertaStockMinimo(int disponible, int stockMinimo)
        => disponible <= stockMinimo;
}

public static class ReglasVenta
{
    public static Dinero CalcularSubtotal(IEnumerable<(Dinero Precio, int Cantidad, Dinero Descuento)> lineas)
    {
        var total = lineas.Sum(l => l.Precio.Valor * l.Cantidad - l.Descuento.Valor);
        return new Dinero(total);
    }

    public static Dinero CalcularIva(Dinero subtotal, Porcentaje tasaIva)
        => tasaIva.AplicarA(subtotal);

    public static Dinero CalcularTotal(Dinero subtotal, Dinero descuento, Dinero iva)
        => subtotal - descuento + iva;

    public static bool TotalesCuadran(Dinero subtotal, Dinero descuento, Dinero iva, Dinero total, Dinero sumaPagos, Dinero cambio)
        => Math.Abs((subtotal - descuento + iva).Valor - total.Valor) < 0.01m
        && Math.Abs(total.Valor - (sumaPagos - cambio).Valor) < 0.01m;
}

public static class ReglasComision
{
    public static Dinero CalcularComision(Dinero monto, TipoComision tipo, Dinero valorComision)
    {
        return tipo switch
        {
            TipoComision.Fija => valorComision,
            TipoComision.Porcentual => new Dinero(Math.Round(monto.Valor * valorComision.Valor / 100m, 2, MidpointRounding.AwayFromZero)),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };
    }

    public static Dinero CalcularTotalConComision(Dinero monto, Dinero comision) => monto + comision;
}

public static class ReglasCorte
{
    public static Dinero CalcularEfectivoEsperado(
        Dinero fondoInicial,
        Dinero efectivoVentas,
        Dinero efectivoPagosServicio,
        Dinero efectivoDevoluciones)
        => fondoInicial + efectivoVentas + efectivoPagosServicio - efectivoDevoluciones;

    public static Dinero CalcularDiferencia(Dinero totalContado, Dinero efectivoEsperado)
        => totalContado - efectivoEsperado;
}