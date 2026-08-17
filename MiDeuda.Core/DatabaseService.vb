Imports System.IO
Imports SQLite

Public Class DatabaseService

    Private ReadOnly _db As SQLiteConnection

    Public Sub New(dbPath As String) 'Recibe la ruta del archivo de base de datos 
        _db = New SQLiteConnection(dbPath)

        _db.CreateTable(Of Gasto)() 'Crea las tablas, si no existen
        _db.CreateTable(Of Factura)()

    End Sub

    'METODOS PARA GASTOS->

    Public Function GuardarGato(Gasto As Gasto) As Integer
        If Gasto.Id <> 0 Then
            Return _db.Update(Gasto)
        Else
            Return _db.Insert(Gasto)
        End If
    End Function

    Public Function ObtenerTodosLosGastos() As List(Of Gasto)
        Return _db.Table(Of Gasto)().OrderByDescending(Function(g) g.Fecha).ToList()
    End Function

    Public Function ObtenerGastosPorPeriodo(filtro As FiltroPeriodo) As List(Of Gasto)
        If Not filtro.EsValido() Then
            Throw New ArgumentException("El rango de fechas no es válido.")
        End If

        Return _db.Table(Of Gasto)().
            Where(Function(g) g.Fecha >= filtro.FechaInicio AndAlso g.Fecha <= filtro.FechaFin).
            OrderByDescending(Function(g) g.Fecha).
            ToList()

    End Function

    Public Function GuardarFactura(factura As Factura) As Integer
        If factura.Id <> 0 Then
            Return _db.Update(factura)
        Else
            Return _db.Insert(factura)
        End If
    End Function

    Public Function ObtenerTodasLasFacturas() As List(Of Factura)
        Return _db.Table(Of Factura)().OrderByDescending(Function(f) f.FechaEmision).ToList()
    End Function

    Public Function ObtenerFacturasPorPeriodo(filtro As FiltroPeriodo) As List(Of Factura)
        If Not filtro.EsValido() Then
            Throw New ArgumentException("El rango de fechas no es váido.")
        End If

        Return _db.Table(Of Factura)().
            Where(Function(f) f.FechaEmision >= filtro.FechaInicio AndAlso f.FechaEmision <= filtro.FechaFin).
        OrderByDescending(Function(f) f.fechaEmision).
            ToList()
    End Function

    Public Function ObtenerFacturasPorEstado(estado As String) As List(Of Factura)
        Return _db.Table(Of Factura)().
                   Where(Function(f) f.EstadoPago = estado).
                   OrderBy(Function(f) f.FechaVencimiento).
                   ToList()
    End Function

    Public Function EliminarFactura(id As Integer) As Integer
        Return _db.Delete(Of Factura)(id)
    End Function


End Class
