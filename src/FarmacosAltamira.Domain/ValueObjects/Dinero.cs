namespace FarmacosAltamira.Domain.ValueObjects;

public readonly record struct Dinero(decimal Valor)
{
    public static implicit operator decimal(Dinero d) => d.Valor;
    public static implicit operator Dinero(decimal v) => new(Math.Round(v, 2, MidpointRounding.AwayFromZero));

    public static Dinero operator +(Dinero a, Dinero b) => new(a.Valor + b.Valor);
    public static Dinero operator -(Dinero a, Dinero b) => new(a.Valor - b.Valor);
    public static Dinero operator *(Dinero a, decimal b) => new(a.Valor * b);
    public static Dinero operator /(Dinero a, decimal b) => new(a.Valor / b);

    public override string ToString() => Valor.ToString("N2");
    
    public static Dinero Cero => new(0m);
}

public readonly record struct Porcentaje(decimal Valor)
{
    public static implicit operator decimal(Porcentaje p) => p.Valor;
    public static implicit operator Porcentaje(decimal v) => new(Math.Clamp(Math.Round(v, 4, MidpointRounding.AwayFromZero), 0, 100));
    
    public Dinero AplicarA(Dinero baseMonto) => new(baseMonto.Valor * Valor / 100m);
    
    public override string ToString() => Valor.ToString("N2") + "%";
}