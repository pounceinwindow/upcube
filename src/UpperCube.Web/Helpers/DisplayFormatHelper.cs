using System.Globalization;

namespace UpperCube.Web.Helpers;

public static class DisplayFormatHelper
{
    private const string DefaultCurrency = "RUB";

    public static string FormatMoney(decimal value, string? currency, CultureInfo culture, string uiCulture)
    {
        var code = string.IsNullOrWhiteSpace(currency) ? DefaultCurrency : currency;
        var suffix = uiCulture == "ru" && string.Equals(code, DefaultCurrency, StringComparison.OrdinalIgnoreCase)
            ? "₽"
            : code;

        return $"{value.ToString("N0", culture)} {suffix}";
    }

    public static string FormatArea(decimal value, CultureInfo culture, string uiCulture)
    {
        var unit = uiCulture == "ru" ? "м²" : "m²";
        return $"{value.ToString("N1", culture)} {unit}";
    }
}
