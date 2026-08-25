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
        // 1. Validar descripción
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            await DisplayAlert("Validación", "Por favor ingresa una descripción para el gasto.", "Aceptar");
            TxtDescripcion.Focus();
            return;
        }

        // 2. Validar formato y valor del monto
        if (!decimal.TryParse(TxtMonto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
        {
            await DisplayAlert("Validación", "Ingresa un monto numérico válido y mayor a 0.", "Aceptar");
            TxtMonto.Focus();
            return;
        }

        // 3. Creación del objeto
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
        LblConversion.IsVisible = false;
        TxtDescripcion.Focus();

        // 6. Actualizar la lista en pantalla
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