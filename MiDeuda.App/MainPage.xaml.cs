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

        // Valores por defecto
        CboMoneda.SelectedIndex = 0;
        CboCategoria.SelectedIndex = 0;

        CargarGastos();
    }

    private void CargarGastos()
    {
        // 1. Obtenemos todos los gastos
        var gastos = _dbService.ObtenerTodosLosGastos();

        // 2. Cargamos la lista completa abajo (Últimos Movimientos)
        ListaGastosView.ItemsSource = null;
        ListaGastosView.ItemsSource = gastos;

        // 3. Calculamos el total SOLO de hoy
        decimal totalHoyEnPesos = 0;
        if (gastos != null)
        {
            foreach (var g in gastos)
            {
                // Comparamos si la fecha del gasto coincide con la fecha actual
                if (g.Fecha.Date == DateTime.Today)
                {
                    totalHoyEnPesos += g.MontoEnPesos;
                }
            }
        }

        // 4. Actualizamos los textos en pantalla
        LblTotalGastos.Text = totalHoyEnPesos.ToString("C", new CultureInfo("es-AR"));
        LblTituloTotal.Text = $"Total Gastado (Hoy, {DateTime.Today:dd/MM})";
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
                LblConversion.Text = "Cotización no disponible";
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
            await DisplayAlert("Faltan datos", "Ingresa en qué gastaste el dinero.", "OK");
            TxtDescripcion.Focus();
            return;
        }

        if (!decimal.TryParse(TxtMonto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
        {
            await DisplayAlert("Monto inválido", "Ingresa un número mayor a cero.", "OK");
            TxtMonto.Focus();
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
                await DisplayAlert("Sin Conexión", "No se pudo obtener el valor del dólar para calcular el total.", "OK");
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
            Fecha = DateTime.Today // Asigna la fecha actual automáticamente
        };

        _dbService.GuardarGasto(nuevoGasto);

        // Limpiar formulario
        TxtDescripcion.Text = string.Empty;
        TxtMonto.Text = string.Empty;
        LblConversion.IsVisible = false;

        CargarGastos();
    }

    private async void OnEliminarGastoInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Gasto gasto)
        {
            string simbolo = gasto.Moneda == "USD" ? "U$S " : "$";
            bool confirmar = await DisplayAlert(
                "Borrar Registro",
                $"¿Borrar \"{gasto.Descripcion}\" por {simbolo}{gasto.Monto:N2}?",
                "Borrar",
                "Cancelar"
            );

            if (confirmar)
            {
                _dbService.EliminarGasto(gasto.Id);
                CargarGastos();
            }
        }
    }
}