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

        // Valores por defecto
        if (CboPeriodo.SelectedIndex == -1)
        {
            CboPeriodo.SelectedIndex = 2; // "Este Mes"
        }

        if (CboMonedaVista != null && CboMonedaVista.SelectedIndex == -1)
        {
            CboMonedaVista.SelectedIndex = 0; // "Ver en ARS"
        }

        CargarResumen();
    }

    private void OnPeriodoChanged(object? sender, EventArgs e)
    {
        CargarResumen();
    }

    private void OnFiltroChanged(object? sender, EventArgs e)
    {
        CargarResumen();
    }

    private async void CargarResumen()
    {
        var todosLosGastos = _dbService.ObtenerTodosLosGastos();
        if (todosLosGastos == null)
        {
            return;
        }

        DateTime fechaInicio = DateTime.MinValue;
        DateTime hoy = DateTime.Today;
        int index = CboPeriodo.SelectedIndex;
        string textoSubtitulo = string.Empty;

        // 1. EVALUAR EL PERIODO
        switch (index)
        {
            case 0: // Esta Semana (Lunes a Domingo)
                int diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                fechaInicio = hoy.AddDays(-1 * diff).Date;
                textoSubtitulo = "(Semanal)";
                break;

            case 1: // Esta Quincena (Días 1 al 15, o 16 hasta fin de mes)
                fechaInicio = hoy.Day <= 15
                    ? new DateTime(hoy.Year, hoy.Month, 1)
                    : new DateTime(hoy.Year, hoy.Month, 16);
                textoSubtitulo = "(Quincenal)";
                break;

            case 2: // Este Mes
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                textoSubtitulo = "(Mensual)";
                break;

            case 3: // Este Bimestre
                int mesInicioBimestre = hoy.Month % 2 == 0 ? hoy.Month - 1 : hoy.Month;
                fechaInicio = new DateTime(hoy.Year, mesInicioBimestre, 1);
                textoSubtitulo = "(Bimestral)";
                break;

            case 4: // Histórico (Todos)
                fechaInicio = DateTime.MinValue;
                textoSubtitulo = "(Histórico)";
                break;
        }

        if (LblSubtituloPeriodo != null)
        {
            LblSubtituloPeriodo.Text = textoSubtitulo;
        }

        // 2. FILTRAR GASTOS POR FECHA
        var gastosFiltrados = todosLosGastos
            .Where(g => g.Fecha.Date >= fechaInicio && g.Fecha.Date <= hoy)
            .ToList();

        if (BtnBorrarTodo != null)
        {
            BtnBorrarTodo.IsVisible = gastosFiltrados.Count > 0;
        }

        // 3. EVALUAR MONEDA Y CONVERSIÓN
        bool verEnUsd = CboMonedaVista != null && CboMonedaVista.SelectedIndex == 1;
        decimal divisorCotizacion = 1m;

        if (verEnUsd)
        {
            try
            {
                divisorCotizacion = await CotizacionService.ObtenerPrecioVentaDolarAsync("oficial");
            }
            catch
            {
                await DisplayAlert("Sin Conexión", "No se pudo obtener la cotización oficial del dólar. Se mostrará en Pesos.", "OK");
                if (CboMonedaVista != null)
                {
                    CboMonedaVista.SelectedIndex = 0;
                }
                divisorCotizacion = 1m;
                verEnUsd = false;
            }
        }

        // 4. CALCULAR TOTALES Y DESGLOSE
        decimal totalPeriodoPesos = gastosFiltrados.Sum(g => g.MontoEnPesos);
        decimal totalFinal = totalPeriodoPesos / divisorCotizacion;

        LblTotalResumen.Text = verEnUsd
            ? $"USD {totalFinal:N2}"
            : totalFinal.ToString("C", new CultureInfo("es-AR"));

        var categoriasAgrupadas = gastosFiltrados
            .GroupBy(g => g.Categoria)
            .Select(grupo => new CategoriaAgrupada
            {
                Nombre = grupo.Key,
                Total = grupo.Sum(x => x.MontoEnPesos) / divisorCotizacion,
                TotalFormateado = verEnUsd
                    ? $"USD {(grupo.Sum(x => x.MontoEnPesos) / divisorCotizacion):N2}"
                    : (grupo.Sum(x => x.MontoEnPesos)).ToString("C", new CultureInfo("es-AR")),
                Porcentaje = totalPeriodoPesos > 0
                    ? (grupo.Sum(x => x.MontoEnPesos) / totalPeriodoPesos).ToString("P1", new CultureInfo("es-AR"))
                    : "0%"
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        ListaCategoriasView.ItemsSource = categoriasAgrupadas;
    }

    private async void OnBorrarTodoClicked(object? sender, EventArgs e)
    {
        // 1. Desplegamos el menú de opciones
        string accion = await DisplayActionSheet(
            "¿Qué datos querés eliminar?",
            "Cancelar",
            null,
            "Eliminar Solo Gastos",
            "Eliminar Solo Facturas",
            "Eliminar TODO (Gastos y Facturas)"
        );

        if (accion == "Cancelar" || string.IsNullOrEmpty(accion))
            return;

        // 2. Pedimos confirmación final
        bool confirmar = await DisplayAlert(
            "¡Atención!",
            $"Estás a punto de {accion.ToUpper()}. Esta acción no se puede deshacer. ¿Deseas continuar?",
            "Sí, eliminar",
            "Cancelar"
        );

        if (confirmar)
        {
            // 3. Borramos según lo elegido
            if (accion == "Eliminar Solo Gastos" || accion == "Eliminar TODO (Gastos y Facturas)")
            {
                _dbService.BorrarTodosLosGastos();
            }

            if (accion == "Eliminar Solo Facturas" || accion == "Eliminar TODO (Gastos y Facturas)")
            {
                _dbService.BorrarTodasLasFacturas();
            }

            // 4. Refrescamos la vista
            CargarResumen();
            await DisplayAlert("Éxito", "Los datos fueron eliminados correctamente.", "OK");
        }
    }
}

public class CategoriaAgrupada
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string TotalFormateado { get; set; } = string.Empty;
    public string Porcentaje { get; set; } = string.Empty;
}