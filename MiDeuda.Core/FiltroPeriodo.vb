Public Class FiltroPeriodo

    'Propiedades para almacenar rangos de fecha
    Public Property FechaInicio As DateTime
    Public Property FechaFin As DateTime

    'Constructor sin parametros (por defecto toma todo el mes actual)
    Public Sub New()

        Dim hoy As DateTime = DateTime.Today
        FechaInicio = New DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0) 'Primer dia del mes a las 00:00:00
        FechaFin = FechaInicio.AddMonths(1).AddSeconds(-1) 'Ultimo instante del mes (sumando un mes y restando un segundo)

    End Sub

    Public Sub New(inicio As DateTime, fin As DateTime) 'constructor personalizado (se le pasa las fechas especificas)

        FechaInicio = New DateTime(inicio.Year, inicio.Month, inicio.Day, 0, 0, 0)
        FechaFin = New DateTime(fin.Year, fin.Month, fin.Day, 23, 59, 59)

    End Sub

    Public Function EsValido() As Boolean 'valida que inicio no es posterior a fin
        Return FechaInicio <= FechaFin
    End Function

End Class
