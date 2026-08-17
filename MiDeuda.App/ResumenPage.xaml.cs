using MiDeuda.Core;
using System.Globalization;

namespace MiDeuda.App;

public partial class ResumenPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public ResumenPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarDashboard();
    }

    private void CargarDashboard()
    {
        var culture = new CultureInfo("es-AR");
        var filtroMesActual = new FiltroPeriodo(); // Constructor por defecto = Mes actual

        // 1. Gastos del mes actual vía MiDeuda.Core (VB.NET)
        var gastosMes = _dbService.ObtenerGastosPorPeriodo(filtroMesActual);
        decimal totalGastado = 0;
        foreach (var g in gastosMes)
        {
            totalGastado += g.Monto;
        }
        LblTotalMes.Text = totalGastado.ToString("C", culture);

        // 2. Facturas pendientes y vencimientos
        var facturas = _dbService.ObtenerTodasLasFacturas();
        decimal totalPendiente = 0;
        int porVencer = 0;
        int vencidas = 0;
        DateTime hoy = DateTime.Today;

        foreach (var f in facturas)
        {
            if (f.EstadoPago != "Pagado")
            {
                totalPendiente += f.MontoTotal;

                if (f.FechaVencimiento < hoy)
                {
                    vencidas++;
                }
                else
                {
                    porVencer++;
                }
            }
        }

        LblTotalPendiente.Text = totalPendiente.ToString("C", culture);
        LblFacturasPorVencer.Text = porVencer.ToString();
        LblFacturasVencidas.Text = vencidas.ToString();

        // 3. Agrupación por categoría
        var categoriasAgrupadas = gastosMes
            .GroupBy(g => string.IsNullOrWhiteSpace(g.Categoria) ? "Varios" : g.Categoria)
            .Select(grupo => new ResumenCategoria
            {
                Categoria = grupo.Key,
                Total = grupo.Sum(g => g.Monto),
                Porcentaje = totalGastado > 0 ? (grupo.Sum(g => g.Monto) / totalGastado) : 0
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        ListaCategoriasView.ItemsSource = null;
        ListaCategoriasView.ItemsSource = categoriasAgrupadas;
    }
}