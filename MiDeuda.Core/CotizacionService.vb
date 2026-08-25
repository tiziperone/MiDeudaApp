Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading.Tasks

Public Class CotizacionService
    Private Shared ReadOnly client As New HttpClient()

    Public Shared Async Function ObtenerPrecioVentaDolarAsync(Optional tipo As String = "oficial") As Task(Of Decimal)
        Dim url As String = $"https://dolarapi.com/v1/dolares/{tipo.ToLower()}"
        Try
            Dim response As HttpResponseMessage = Await client.GetAsync(url)
            response.EnsureSuccessStatusCode()

            Dim jsonString As String = Await response.Content.ReadAsStringAsync()
            Dim cotizacion As CotizacionDolar = JsonSerializer.Deserialize(Of CotizacionDolar)(jsonString)

            Return cotizacion.venta
        Catch ex As Exception
            Throw New Exception("No se pudo obtener la cotización actual: " & ex.Message)
        End Try
    End Function
End Class