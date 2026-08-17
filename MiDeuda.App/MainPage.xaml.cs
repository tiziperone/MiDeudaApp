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
        // Validación de descripción
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            await DisplayAlert("Validación", "Por favor ingresa una descripción para el gasto.", "Aceptar");
            TxtDescripcion.Focus();
            return;
        }

        // Validación y conversión de monto
        if (!decimal.TryParse(TxtMonto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) &&
            !decimal.TryParse(TxtMonto.Text, out monto))
        {
            await DisplayAlert("Validación", "Ingresa un monto numérico válido.", "Aceptar");
            TxtMonto.Focus();
            return;
        }

        if (monto <= 0)
        {
            await DisplayAlert("Validación", "El monto debe ser mayor a 0.", "Aceptar");
            return;
        }

        // Creación del objeto de MiDeuda.Core (VB.NET)
        var nuevoGasto = new Gasto
        {
            Descripcion = TxtDescripcion.Text.Trim(),
            Monto = monto,
            Moneda = CboMoneda.SelectedItem?.ToString() ?? "ARS",
            Categoria = CboCategoria.SelectedItem?.ToString() ?? "Varios",
            Fecha = FechaEmision.Date
        };

        // Guardar en base de datos SQLite
        _dbService.GuardarGasto(nuevoGasto);

        // Limpiar campos del formulario
        TxtDescripcion.Text = string.Empty;
        TxtMonto.Text = string.Empty;
        TxtDescripcion.Focus();

        // Actualizar la lista en pantalla
        CargarGastos();
    }
}