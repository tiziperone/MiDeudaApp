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
        CboMoneda.SelectedIndex = 0; // ARS por defecto
        CboCategoria.SelectedIndex = 0; // Comida por defecto
        DtpFecha.Date = DateTime.Today;

        CargarGastos();
    }

    private void CargarGastos()
    {
        // 1. Obtener los gastos desde SQLite usando el servicio de VB.NET
        var gastos = _dbService.ObtenerTodosLosGastos();

        // 2. Asignar la lista a la vista
        ListaGastosView.ItemsSource = null;
        ListaGastosView.ItemsSource = gastos;

        // 3. Calcular el total acumulado
        decimal total = 0;
        if (gastos != null)
        {
            foreach (var g in gastos)
            {
                total += g.Monto;
            }
        }

        LblTotalGastos.Text = total.ToString("C", new CultureInfo("es-AR"));
    }

    private async void OnMonedaOrMontoChanged(object? sender, EventArgs e)
    {
        if (CboMoneda.SelectedItem?.ToString() == "USD" &&
            decimal.TryParse(TxtMonto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal montoUsd) &&
            montoUsd > 0)
        {
            try
            {
                decimal cotizacion = await CotizacionService.ObtenerPrecioVentaDolarAsync("oficial");
                decimal montoArs = montoUsd * cotizacion;

                LblConversion.Text = $"≈ ${montoArs:N2} ARS (TC Oficial: ${cotizacion:N2})";
                LblConversion.IsVisible = true;
            }
            catch
            {
                LblConversion.Text = "No se pudo obtener la cotización actual";
                LblConversion.IsVisible = true;
            }
        }
        else
        {
            LblConversion.Text = string.Empty;
            LblConversion.IsVisible = false;
        }
    }

    private async void OnGuardarGastoClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            await DisplayAlert("Validación", "Ingresa una descripción.", "Aceptar");
            return;
        }

        if (!decimal.TryParse(TxtMonto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
        {
            await DisplayAlert("Validación", "Ingresa un monto válido mayor a 0.", "Aceptar");
            return;
        }

        string monedaSeleccionada = CboMoneda.SelectedItem?.ToString() ?? "ARS";
        decimal cotizacion = 1m;
        decimal montoEnPesos = monto;

        if (monedaSeleccionada == "USD")
        {
            try
            {
                cotizacion = await CotizacionService.ObtenerPrecioVentaDolarAsync("oficial");
                montoEnPesos = monto * cotizacion;
            }
            catch
            {
                await DisplayAlert("Error", "No se pudo obtener la cotización oficial del dólar. Verifica tu conexión.", "Aceptar");
                return;
            }
        }

        var nuevoGasto = new Gasto
        {
            Descripcion = TxtDescripcion.Text.Trim(),
            Monto = monto,
            Moneda = monedaSeleccionada,
            CotizacionUsada = cotizacion,
            MontoEnPesos = montoEnPesos,
            Categoria = CboCategoria.SelectedItem?.ToString() ?? "Varios",
            Fecha = DtpFecha.Date ?? DateTime.Today
        };

        _dbService.GuardarGasto(nuevoGasto);

        TxtDescripcion.Text = string.Empty;
        TxtMonto.Text = string.Empty;
        LblConversion.IsVisible = false;
        CargarGastos();
    }

    private async void OnEliminarGastoInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Gasto gasto)
        {
            bool confirmar = await DisplayAlert(
                "Eliminar Gasto",
                $"¿Estás seguro de eliminar el gasto \"{gasto.Descripcion}\" de ${gasto.Monto:N2}?",
                "Sí, Eliminar",
                "Cancelar"
            );

            if (confirmar)
            {
                // Llama al método de eliminación en VB.NET (DatabaseService)
                _dbService.EliminarGasto(gasto.Id);

                // Recargar la lista y el total acumulado
                CargarGastos();
            }
        }
    }
}