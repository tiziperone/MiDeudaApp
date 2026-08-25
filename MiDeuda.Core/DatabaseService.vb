Imports System.IO
Imports SQLite

Public Class DatabaseService

    Private ReadOnly _db As SQLiteConnection

    Public Sub New(dbPath As String)
        _db = New SQLiteConnection(dbPath)

        ' Crea las tablas automáticamente si no existen o actualiza el esquema
        _db.CreateTable(Of Gasto)()
        _db.CreateTable(Of Factura)()
    End Sub

    ' =========================================================================
    ' MÉTODOS PARA GASTOS
    ' =========================================================================

    Public Function GuardarGasto(gasto As Gasto) As Integer
        If gasto.Id <> 0 Then
            Return _db.Update(gasto)
        Else
            Return _db.Insert(gasto)
        End If
    End Function

    Public Function BorrarTodosLosGastos() As Integer
        Return _db.DeleteAll(Of Gasto)()
    End Function

    Public Function BorrarTodasLasFacturas() As Integer
        Return _db.DeleteAll(Of Factura)()
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

    Public Function ObtenerTotalGastosEnPesos() As Decimal
        Dim gastos = _db.Table(Of Gasto)().ToList()
        Return gastos.Sum(Function(g) g.MontoEnPesos)
    End Function

    Public Function EliminarGasto(id As Integer) As Integer
        Return _db.Delete(Of Gasto)(id)
    End Function

    ' =========================================================================
    ' MÉTODOS PARA FACTURAS
    ' =========================================================================

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
            Throw New ArgumentException("El rango de fechas no es válido.")
        End If

        Return _db.Table(Of Factura)().
                   Where(Function(f) f.FechaEmision >= filtro.FechaInicio AndAlso f.FechaEmision <= filtro.FechaFin).
                   OrderByDescending(Function(f) f.FechaEmision).
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