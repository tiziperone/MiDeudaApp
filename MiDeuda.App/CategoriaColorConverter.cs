using System.Globalization;

namespace MiDeuda.App;

public class CategoriaColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string categoria = value?.ToString()?.Trim() ?? string.Empty;

        return categoria.ToLower() switch
        {
            "salud" => Color.FromArgb("#2ECC71"),       // Verde
            "servicios" => Color.FromArgb("#3498DB"),   // Azul
            "transporte" => Color.FromArgb("#7F8C8D"),  // Gris
            "varios" => Color.FromArgb("#C49A6C"),      // Marrón claro
            "comida" => Color.FromArgb("#E67E22"),      // Naranja
            _ => Color.FromArgb("#95A5A6")              // Gris por defecto
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw continentalException();
    }

    private static NotImplementedException continentalException() => new();
}