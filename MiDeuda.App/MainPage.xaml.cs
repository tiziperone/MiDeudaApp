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
        // 1. Obtener los gastos desde SQLite usando MiDeuda.Core (VB.NET)
        var gastos = _dbService.ObtenerTodosLosGastos();

        // 2. Asignar la lista a la vista
        ListaGastosView.ItemsSource = null;
        ListaGastosView.ItemsSource = gastos;

        // 3. Calcular el total acumulado en base a los montos normalizados en ARS
        decimal totalEnPesos = 0;
        if (gastos != null)
        {
            foreach (var g in gastos)
            {
                // Suma el equivalente en pesos para no mezclar unidades USD con ARS
                totalEnPesos += g.MontoEnPesos;
            }
        }

        LblTotalGastos.Text = totalEnPesos.ToString("C", new CultureInfo("es-AR"));
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

                LblConversion.Text = $"≈ ${montoArs:N2} ARS (Cotización Oficial: ${cotizacion:N2})";
                PnlConversion.IsVisible = true;
            }
            catch
            {
                LblConversion.Text = "Cotización no disponible sin conexión";
                PnlConversion.IsVisible = true;
            }
        }
        else
        {
            LblConversion.Text = string.Empty;
            PnlConversion.IsVisible = false;
        }
    }

    private async void OnGuardarGastoClicked(object? sender, EventArgs e)
    {
        // 1. Validar descripción
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            await DisplayAlert("Validación", "Ingresa una descripción para el gasto.", "Aceptar");
            TxtDescripcion.Focus();
            return;
        }

        // 2. Validar formato y valor numérico
        if (!decimal.TryParse(TxtMonto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
        {
            await DisplayAlert("Validación", "Ingresa un monto numérico válido y mayor a 0.", "Aceptar");
            TxtMonto.Focus();
            return;
        }

        string monedaSeleccionada = CboMoneda.SelectedItem?.ToString() ?? "ARS";
        decimal cotizacion = 1m;
        decimal montoEnPesos = monto;

        // 3. Consultar cotización si la divisa es USD
        if (monedaSeleccionada == "USD")
        {
            try
            {
                cotizacion = await CotizacionService.ObtenerPrecioVentaDolarAsync("oficial");
                montoEnPesos = monto * cotizacion;
            }
            catch
            {
                await DisplayAlert("Error de Conexión", "No se pudo consultar el valor del dólar. Verifica tu conexión a internet.", "Aceptar");
                return;
            }
        }

        // 4. Instanciar el objeto de MiDeuda.Core
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

        // 5. Guardar en SQLite mediante VB.NET
        _dbService.GuardarGasto(nuevoGasto);

        // 6. Limpieza y refresco
        TxtDescripcion.Text = string.Empty;
        TxtMonto.Text = string.Empty;
        LblConversion.IsVisible = false;
        TxtDescripcion.Focus();

        CargarGastos();
    }

    private async void OnEliminarGastoInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Gasto gasto)
        {
            string simboloMoneda = gasto.Moneda == "USD" ? "USD " : "$";
            bool confirmar = await DisplayAlert(
                "Eliminar Gasto",
                $"¿Deseas eliminar el gasto \"{gasto.Descripcion}\" por {simboloMoneda}{gasto.Monto:N2}?",
                "Sí, Eliminar",
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