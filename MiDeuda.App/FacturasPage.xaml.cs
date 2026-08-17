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

        CargarFacturas();
    }

    private void CargarFacturas()
    {
        var facturas = _dbService.ObtenerTodasLasFacturas();
        ListaFacturasView.ItemsSource = facturas;

        decimal totalPendiente = 0;
        foreach (var f in facturas)
        {
            if (f.EstadoPago == "Pendiente")
            {
                totalPendiente += f.MontoTotal;
            }
        }

        LblTotalPendiente.Text = totalPendiente.ToString("C", new CultureInfo("es-AR"));
    }

    private async void OnGuardarFacturaClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtEmisor.Text))
        {
            await DisplayAlertAsync("Validación", "Por favor ingresa el nombre del emisor/proveedor.", "Aceptar");
            TxtEmisor.Focus();
            return;
        }

        if (!decimal.TryParse(TxtMonto.Text, out decimal monto) || monto <= 0)
        {
            await DisplayAlertAsync("Validación", "Ingresa un monto válido mayor a 0.", "Aceptar");
            TxtMonto.Focus();
            return;
        }

        var nuevaFactura = new Factura
        {
            Emisor = TxtEmisor.Text.Trim(),
            Concepto = string.IsNullOrWhiteSpace(TxtConcepto.Text) ? "Servicio / Factura" : TxtConcepto.Text.Trim(),
            NroFactura = TxtNumero.Text?.Trim() ?? string.Empty,
            MontoTotal = monto,
            TipoMoneda = CboMoneda.SelectedItem?.ToString() ?? "ARS",
            FechaEmision = DtpEmision.Date ?? DateTime.Today,
            FechaVencimiento = DtpVencimiento.Date ?? DateTime.Today,
            EstadoPago = "Pendiente"
        };

        _dbService.GuardarFactura(nuevaFactura);

        TxtEmisor.Text = string.Empty;
        TxtConcepto.Text = string.Empty;
        TxtNumero.Text = string.Empty;
        TxtMonto.Text = string.Empty;
        TxtEmisor.Focus();

        CargarFacturas();
    }

    private async void OnMarcarPagadaClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Factura factura)
        {
            if (factura.EstadoPago == "Pagada")
            {
                await DisplayAlertAsync("Información", "Esta factura ya se encuentra pagada.", "OK");
                return;
            }

            bool confirmar = await DisplayAlertAsync("Confirmar Pago", $"¿Deseas marcar la factura de {factura.Emisor} por ${factura.MontoTotal:N2} como pagada y registrar el gasto?", "Sí, Pagar", "Cancelar");

            if (confirmar)
            {
                // 1. Actualizar estado de la factura
                factura.EstadoPago = "Pagada";
                _dbService.GuardarFactura(factura);

                // 2. Crear automáticamente el gasto correspondiente en SQLite
                var gastoFactura = new Gasto
                {
                    Descripcion = $"Pago Factura: {factura.Emisor} ({factura.Concepto})",
                    Monto = factura.MontoTotal,
                    Moneda = factura.TipoMoneda,
                    Categoria = "Servicios",
                    Fecha = DateTime.Today
                };

                _dbService.GuardarGasto(gastoFactura);

                // 3. Recargar lista
                CargarFacturas();
            }
        }
    }
}