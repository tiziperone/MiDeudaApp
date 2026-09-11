# 📱 MiDeuda - Gestor Financiero Multiplataforma & Control de Gastos

Aplicación multiplataforma desarrollada con **.NET MAUI** (C# / XAML) y núcleo de lógica de negocio en **Visual Basic .NET (VB.NET)**, diseñada para el seguimiento integral de facturas, control de pasivos, balances periódicos y conversión automática de divisas en tiempo real.

![Status](https://img.shields.io/badge/Status-En%20Desarrollo%20Activo-yellow)
![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-Multiplatform%20(Mobile%20%26%20Desktop)-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/UI-C%23%20%2F%20XAML-239120?logo=csharp&logoColor=white)
![VB.NET](https://img.shields.io/badge/Core-VB.NET-00599C)
![SQLite](https://img.shields.io/badge/Database-SQLite%20(Local)-003B57?logo=sqlite&logoColor=white)
![API REST](https://img.shields.io/badge/API-DolarAPI-orange)

---

## 📌 Visión del Proyecto

**MiDeuda** nace para resolver la fricción habitual al registrar egresos diarios y deudas pendientes. Combina una arquitectura híbrida moderna en el ecosistema .NET con un enfoque orientado a la agilidad del usuario: registrar obligaciones en moneda local o extranjera, centralizar vencimientos de facturas y estructurar resúmenes automáticos por categorías y períodos.

El objetivo a mediano plazo es transformar la aplicación en un asistente financiero reactivo que minimice la carga manual tras realizar pagos presenciales.

---

## ✨ Características Actuales

* **Arquitectura Híbrida (.NET Interop):** Capa visual e interactiva construida en .NET MAUI con C# y XAML, consumiendo una biblioteca de clases central desacoplada (`MiDeuda.Core`) programada en Visual Basic .NET.
* **Persistencia Local Segura:** Almacenamiento embebido con **SQLite-net**, garantizando funcionamiento offline y privacidad absoluta de los datos financieros.
* **Cotización de Divisas en Tiempo Real:** Integración asincrónica con la API de [DolarAPI](https://dolarapi.com/) para calcular equivalencias entre Pesos y Dólares al momento de registrar facturas o gastos bimonetarios.
* **Resumen Dinámico de Gastos:** Agrupación y filtrado por rangos temporales (`FiltroPeriodo`) y clasificación visual por categorías mediante convertidores XAML (`CategoriaColorConverter`, `EsDolarConverter`).
* **Control de Facturas:** Seguimiento de estados de pago (pendiente / abonado) con alertas por fecha de emisión y vencimiento.

---

## 🔮 Roadmap: Detección y Categorización Inteligente de Pagos QR

Actualmente en fase de diseño e investigación técnica para próximas versiones:

1. **Captura Reactiva de Transacciones QR:**
   * Detección o escucha de notificaciones de cobro/pago emitidas por billeteras electrónicas y aplicaciones bancarias en dispositivos móviles.
2. **Notificaciones Push Interactivas (Quick Categorization):**
   * Al concretar un pago (por ejemplo, en un supermercado), la aplicación dispara una notificación nativa en el teléfono con el monto exacto debitado.
   * La notificación ofrece acciones rápidas directamente en pantalla (botones de categoría: *Alimentos*, *Servicios*, *Ocio*, *Transporte*).
3. **Registro en Segundo Plano:**
   * Al presionar la categoría deseada, la notificación se descarta automáticamente y el gasto queda persistido en la base de datos local sin necesidad de abrir la aplicación completa.

---

## 🏗️ Estructura de la Solución

```text
MiDeuda/
├── MiDeuda.App/             # Capa de Presentación (.NET MAUI - C# / XAML)
│   ├── Platforms/           # Configuraciones nativas (Android, iOS, Windows, macOS)
│   ├── Resources/           # Estilos, íconos y fuentes del sistema
│   ├── AppShell.xaml        # Contenedor principal de navegación por pestañas
│   ├── FacturasPage.xaml    # Listado, scroll y administración de facturas
│   ├── MainPage.xaml        # Panel de operaciones rápidas y conversor
│   └── ResumenPage.xaml     # Métricas consolidadas y balances por categoría
├── MiDeuda.Core/            # Capa de Servicios y Dominio (VB.NET)
│   ├── CotizacionService.vb # Cliente HTTP para consulta asincrónica de divisas
│   ├── DatabaseService.vb   # Implementación CRUD y consultas tipadas en SQLite
│   └── Modelos/             # Entidades: Factura, Gasto, FiltroPeriodo, CotizacionDolar
├── .gitignore
└── MiDeuda.slnx
```

👨‍💻 Desarrollador
Tiziano Perone – Estudiante de Licenciatura en Sistemas de Información (FaCENA - UNNE)

    GitHub: @tiziperone
