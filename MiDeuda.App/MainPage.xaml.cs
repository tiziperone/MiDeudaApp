using MiDeuda.Core;

namespace MiDeuda.App;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _dbService;

    // Recibe el servicio automáticamente por inyector de dependencias
    public MainPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        // Prueba de inserción
        var nuevoGasto = new Gasto
        {
            Descripcion = "Gasto de prueba",
            Monto = 1500.50m,
            Fecha = DateTime.Now,
            Moneda = "ARS",
            Categoria = "General"
        };

        _dbService.GuardarGasto(nuevoGasto);

        // Verificamos cuántos hay guardados
        var total = _dbService.ObtenerTodosLosGastos().Count;

        BtnPrueba.Text = $"Gastos guardados en SQLite: {total}";
        SemanticScreenReader.Announce(BtnPrueba.Text);
    }
}