Imports SQLite

<Table("Facturas")>
Public Class Factura

    <PrimaryKey, AutoIncrement>
    Public Property Id As Integer

    ' Código o nro de comprobante
    Public Property NroFactura As String

    ' Quién emite la factura
    Public Property Emisor As String

    ' Descripción o concepto
    Public Property Concepto As String

    ' Total a pagar
    Public Property MontoTotal As Decimal

    ' Fecha de emisión
    Public Property FechaEmision As DateTime

    ' Fecha de vencimiento
    Public Property FechaVencimiento As DateTime

    ' Tipo de moneda
    Public Property TipoMoneda As String

    ' Estado del pago (Pagado, Pendiente, Vencido)
    Public Property EstadoPago As String

    <Ignore>
    Public ReadOnly Property EsPagada As Boolean
        Get
            Return EstadoPago = "Pagado"
        End Get
    End Property

End Class