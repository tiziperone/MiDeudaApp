Imports SQLite

<Table("Gastos")>
Public Class Gasto
    'identificador único del gasto
    <PrimaryKey, AutoIncrement> 'indican al motor de base de datos cómo tratar esa columna al crear la tabla
    Public Property Id As Integer 'Define una propiedad pública en la clase VB.NET llamada Id

    'Concepto o detalle de la compra
    Public Property Descripcion As String

    'Monto del gasto
    Public Property Monto As Decimal

    'Fecha y hora del gasto para filtrar por periodo
    Public Property Fecha As DateTime

    'Seleccionar el tipo de moneda
    Public Property Moneda As String

    'Tipo de gasto (por ejemplo, comida, transporte, entretenimiento, etc.)
    Public Property Categoria As String 'fin

End Class
