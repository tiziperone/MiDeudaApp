Imports SQLite

<Table("Facturas")>
Public Class Factura

    <PrimaryKey, AutoIncrement>
    Public Property Id As Integer

    'Codigo o nro de comprobante
    Public Property NroFactura As String

    'Quien emite la factura
    Public Property Emisor As String

    'Descripcion o concepto
    Public Property Concepto As String

    'Total a pagar
    Public Property MontoTotal As Decimal

    'Fecha de emision
    Public Property FechaEmision As DateTime

    'Fecha vencimiento
    Public Property FechaVencimiento As DateTime

    'Tipo de moneda
    Public Property TipoMoneda As String

    'Estado del pago (pagado, pendiente, vencido)
    Public Property EstadoPago As String

End Class
