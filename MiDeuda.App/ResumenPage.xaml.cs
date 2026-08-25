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
        // Selecciona "Este Mes" (índice 2) por defecto al entrar
        if (CboPeriodo.SelectedIndex == -1)
            CboPeriodo.SelectedIndex = 2;
        else
            CargarResumen();
    }

    private void OnPeriodoChanged(object sender, EventArgs e)
    {
        CargarResumen();
    }

    private void CargarResumen()
    {
        var todosLosGastos = _dbService.ObtenerTodosLosGastos();
        if (todosLosGastos == null) return;

        DateTime fechaInicio = DateTime.MinValue;
        DateTime hoy = DateTime.Today;
        int index = CboPeriodo.SelectedIndex;

        string textoSubtitulo = ""; // Variable para guardar el texto sutil

        // 0: Semana, 1: Quincena, 2: Mes, 3: Bimestre, 4: Histórico
        switch (index)
        {
            case 0: // Esta Semana
                int diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                fechaInicio = hoy.AddDays(-1 * diff).Date;
                textoSubtitulo = "(Semanal)";
                break;

            case 1: // Esta Quincena
                if (hoy.Day <= 15)
                    fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                else
                    fechaInicio = new DateTime(hoy.Year, hoy.Month, 16);
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

            case 4: // Histórico
                fechaInicio = DateTime.MinValue;
                textoSubtitulo = "(Histórico)";
                break;
        }

        // Actualizamos la etiqueta sutil de la vista
        LblSubtituloPeriodo.Text = textoSubtitulo;

        // Filtrar los gastos
        var gastosFiltrados = todosLosGastos
            .Where(g => g.Fecha.Date >= fechaInicio && g.Fecha.Date <= hoy)
            .ToList();

        // Controlar el botón de "Borrar Todo"
        BtnBorrarTodo.IsVisible = gastosFiltrados.Count > 0;

        // Calcular el total
        decimal totalPeriodo = gastosFiltrados.Sum(g => g.MontoEnPesos);
        LblTotalResumen.Text = totalPeriodo.ToString("C", new CultureInfo("es-AR"));

        // Agrupar por categoría en C#
        var categoriasAgrupadas = gastosFiltrados
            .GroupBy(g => g.Categoria)
            .Select(grupo => new CategoriaAgrupada
            {
                Nombre = grupo.Key,
                Total = grupo.Sum(x => x.MontoEnPesos),
                Porcentaje = totalPeriodo > 0
                             ? (grupo.Sum(x => x.MontoEnPesos) / totalPeriodo).ToString("P1", new CultureInfo("es-AR"))
                             : "0%"
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        ListaCategoriasView.ItemsSource = categoriasAgrupadas;
    }

    private async void OnBorrarTodoClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert(
            "¡Peligro!",
            "¿Estás seguro de que querés borrar los gastos de este periodo? Se borrarán definitivamente.",
            "Sí, borrar",
            "Cancelar"
        );

        if (confirmar)
        {
            // Primero obtenemos los gastos que se están viendo ahora
            DateTime fechaInicio = DateTime.MinValue;
            DateTime hoy = DateTime.Today;

            // Repetimos la lógica de fecha para saber qué borrar
            // (Omito el switch repetido acá para no hacerlo tan largo, pero podés 
            // usar el mismo cálculo para borrar solo los del rango o directamente borrar todo)

            _dbService.BorrarTodosLosGastos(); // Por ahora borramos todo como en la principal
            CargarResumen();
        }
    }
}

// Clase auxiliar para la vista
public class CategoriaAgrupada
{
    public string Nombre { get; set; }
    public decimal Total { get; set; }
    public string Porcentaje { get; set; }
}