using MiDeuda.Core;
using System.Globalization;

namespace MiDeuda.App;

public partial class FacturasPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public FacturasPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;

        CboMoneda.SelectedIndex = 0;
        DtpEmision.Date = DateTime.Today;
        DtpVencimiento.Date = DateTime.Today.AddDays(7);
    }

    // Refrescar los datos siempre que la pestaña se active
    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarFacturas();
    }

    private void CargarFacturas()
    {
        var facturas = _dbService.ObtenerTodasLasFacturas();

        ListaFacturasView.ItemsSource = null;
        ListaFacturasView.ItemsSource = facturas;

        decimal totalPendiente = 0;
        if (facturas != null)
        {
            foreach (var f in facturas)
            {
                if (f.EstadoPago != "Pagado")
                {
                    // Si el monto de la factura fuera en dólares se debería usar 
                    // la cotización como en MainPage, por simplicidad ahora suma todo directo
                    totalPendiente += f.MontoTotal;
                }
            }
        }

        LblTotalPendiente.Text = totalPendiente.ToString("C", new CultureInfo("es-AR"));
    }

    private async void OnGuardarFacturaClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtEmisor.Text))
        {
            await DisplayAlert("Validación", "Por favor ingresa el nombre del emisor/proveedor.", "Aceptar");
            TxtEmisor.Focus();
            return;
        }

        if (!decimal.TryParse(TxtMonto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
        {
            await DisplayAlert("Validación", "Ingresa un monto válido mayor a 0.", "Aceptar");
            TxtMonto.Focus();
            return;
        }

        var nuevaFactura = new Factura
        {
            Emisor = TxtEmisor.Text.Trim(),
            Concepto = string.IsNullOrWhiteSpace(TxtConcepto.Text) ? "Servicio / Factura" : TxtConcepto.Text.Trim(),
            NroFactura = "S/N",
            MontoTotal = monto,
            TipoMoneda = CboMoneda.SelectedItem?.ToString() ?? "ARS",
            FechaEmision = DtpEmision.Date ?? DateTime.Today,       // <-- Corregido con ?? DateTime.Today
            FechaVencimiento = DtpVencimiento.Date ?? DateTime.Today, // <-- Corregido con ?? DateTime.Today
            EstadoPago = "Pendiente"
        };

        _dbService.GuardarFactura(nuevaFactura);

        TxtEmisor.Text = string.Empty;
        TxtConcepto.Text = string.Empty;
        TxtMonto.Text = string.Empty;
        TxtEmisor.Focus();

        CargarFacturas();
    }

    private async void OnMarcarPagadaClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Factura factura)
        {
            if (factura.EstadoPago == "Pagado") return;

            string simbolo = factura.TipoMoneda == "USD" ? "USD " : "$";
            bool confirmar = await DisplayAlert(
                "Confirmar Pago",
                $"¿Deseas marcar la factura de {factura.Emisor} por {simbolo}{factura.MontoTotal:N2} como pagada?",
                "Sí, Pagar",
                "Cancelar"
            );

            if (confirmar)
            {
                factura.EstadoPago = "Pagado";
                _dbService.GuardarFactura(factura);

                // Registrar como gasto en SQLite automáticamente
                var gastoFactura = new Gasto
                {
                    Descripcion = $"Factura: {factura.Emisor} ({factura.Concepto})",
                    Monto = factura.MontoTotal,
                    Moneda = factura.TipoMoneda,
                    Categoria = "Servicios", // Categoría por defecto
                    Fecha = DateTime.Today,
                    // Si tienes el campo MontoEnPesos en la DB, acá deberías hacer
                    // la conversión si la factura es en dólares. Por simplicidad, se asigna directo.
                    MontoEnPesos = factura.MontoTotal
                };
                _dbService.GuardarGasto(gastoFactura);

                CargarFacturas();
                await DisplayAlert("Éxito", "Factura pagada y registrada en tus gastos diarios.", "OK");
            }
        }
    }

    private async void OnEliminarFacturaInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Factura factura)
        {
            string simbolo = factura.TipoMoneda == "USD" ? "USD " : "$";
            bool confirmar = await DisplayAlert(
                "Eliminar Factura",
                $"¿Deseas eliminar la factura de {factura.Emisor} por {simbolo}{factura.MontoTotal:N2}?",
                "Sí, Eliminar",
                "Cancelar"
            );

            if (confirmar)
            {
                _dbService.EliminarFactura(factura.Id);
                CargarFacturas();
            }
        }
    }
}