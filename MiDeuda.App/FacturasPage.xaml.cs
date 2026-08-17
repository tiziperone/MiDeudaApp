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

        // Forzar refresco visual
        ListaFacturasView.ItemsSource = null;
        ListaFacturasView.ItemsSource = facturas;

        decimal totalPendiente = 0;
        foreach (var f in facturas)
        {
            if (f.EstadoPago != "Pagado")
            {
                totalPendiente += f.MontoTotal;
            }
        }

        LblTotalPendiente.Text = totalPendiente.ToString("C", new CultureInfo("es-AR"));
    }

    private async void OnMarcarPagadaClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Factura factura)
        {
            if (factura.EstadoPago == "Pagado")
            {
                return;
            }

            bool confirmar = await DisplayAlertAsync("Confirmar Pago", $"¿Deseas marcar la factura de {factura.Emisor} por ${factura.MontoTotal:N2} como pagada?", "Sí, Pagar", "Cancelar");

            if (confirmar)
            {
                // 1. Marcar como Pagado
                factura.EstadoPago = "Pagado";
                _dbService.GuardarFactura(factura);

                // 2. Registrar el gasto correspondiente en la base de datos
                var gastoFactura = new Gasto
                {
                    Descripcion = $"Pago Factura: {factura.Emisor} ({factura.Concepto})",
                    Monto = factura.MontoTotal,
                    Moneda = factura.TipoMoneda,
                    Categoria = "Servicios",
                    Fecha = DateTime.Today
                };
                _dbService.GuardarGasto(gastoFactura);

                // 3. Recargar la lista
                CargarFacturas();
            }
        }
    }
}