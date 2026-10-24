using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FarmacosAltamira.Infrastructure.Persistence;

public sealed class SnakeUpperEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public SnakeUpperEnumConverter() : base(
        v => ToSnakeUpper(v.ToString()),
        s => Enum.Parse<TEnum>(s.Replace("_", ""), true))
    {
    }

    private static string ToSnakeUpper(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return Regex.Replace(input, "(?<!^)([A-Z])", "_$1").ToUpperInvariant();
    }
}