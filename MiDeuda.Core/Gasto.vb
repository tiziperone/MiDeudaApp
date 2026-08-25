Imports SQLite

<Table("Gastos")>
Public Class Gasto
    ' Identificador único autoincremental
    <PrimaryKey, AutoIncrement>
    Public Property Id As Integer

    ' Concepto o detalle de la compra
    Public Property Descripcion As String

    ' Monto ingresado por el usuario
    Public Property Monto As Decimal

    ' Moneda ingresada ("ARS" o "USD")
    Public Property Moneda As String

    ' Tipo de cambio utilizado al momento de registrar el gasto (1 si es ARS)
    Public Property CotizacionUsada As Decimal

    ' Monto convertido y estandarizado en ARS para cálculos globales
    Public Property MontoEnPesos As Decimal

    ' Categoría del gasto (Comida, Transporte, etc.)
    Public Property Categoria As String

    ' Fecha del gasto
    Public Property Fecha As DateTime
End Class