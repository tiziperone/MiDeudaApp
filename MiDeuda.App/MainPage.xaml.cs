using MiDeuda.Core;
using System.Globalization;

namespace MiDeuda.App;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public MainPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;

        // Valores iniciales
        CboMoneda.SelectedIndex = 0; // Selecciona ARS por defecto
        CboCategoria.SelectedIndex = 0; // Selecciona Comida por defecto
        DtpFecha.Date = DateTime.Today;

        CargarGastos();
    }

    private void CargarGastos()
    {
        // 1. Obtener los gastos desde SQLite usando el servicio de VB.NET
        var gastos = _dbService.ObtenerTodosLosGastos();

        // 2. Asignar la lista a la vista
        ListaGastosView.ItemsSource = gastos;

        // 3. Calcular el total acumulado
        decimal total = 0;
        foreach (var g in gastos)
        {
            total += g.Monto;
        }

        LblTotalGastos.Text = total.ToString("C", new CultureInfo("es-AR"));
    }

    private async void OnGuardarGastoClicked(object? sender, EventArgs e)
    {
        // 1. Validar descripción
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            await DisplayAlertAsync("Validación", "Por favor ingresa una descripción para el gasto.", "Aceptar");
            TxtDescripcion.Focus();
            return;
        }

        // 2. Validar formato y valor del monto
        if (!decimal.TryParse(TxtMonto.Text, out decimal monto) || monto <= 0)
        {
            await DisplayAlertAsync("Validación", "Ingresa un monto numérico válido y mayor a 0.", "Aceptar");
            TxtMonto.Focus();
            return;
        }

        // 3. Creación del objeto resolviendo el tipo anulable de la fecha
        var nuevoGasto = new Gasto
        {
            Descripcion = TxtDescripcion.Text.Trim(),
            Monto = monto,
            Moneda = CboMoneda.SelectedItem?.ToString() ?? "ARS",
            Categoria = CboCategoria.SelectedItem?.ToString() ?? "Varios",
            Fecha = DtpFecha.Date ?? DateTime.Today
        };

        // 4. Guardar en SQLite
        _dbService.GuardarGasto(nuevoGasto);

        // 5. Limpiar formulario
        TxtDescripcion.Text = string.Empty;
        TxtMonto.Text = string.Empty;
        TxtDescripcion.Focus();

        // 6. Actualizar la lista en pantalla
        CargarGastos();
    }
}