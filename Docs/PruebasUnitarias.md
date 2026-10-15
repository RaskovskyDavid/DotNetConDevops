# Pruebas del proyecto ContactosAPI (Minimal API, .NET 10)

Estructura final del repositorio:

```
DotNetConDevops/
├── ContactosAPI.slnx
└── src/
    ├── ContactosAPI/                 # Web API (Minimal API)
    │   ├── Program.cs
    │   └── Properties/AssemblyInfo.cs
    └── ContactosAPI.Tests/           # Proyecto xUnit
        ├── ContactosAPI.Tests.csproj
        ├── WeatherForecastTests.cs           # pruebas unitarias
        └── WeatherForecastEndpointTests.cs   # pruebas de integración del endpoint
```

## 1. Crear la solución y el proyecto de pruebas

Ejecutar desde la raíz del repositorio:

```powershell
# 1. Crear la solución (en .NET 10 genera ContactosAPI.slnx)
dotnet new sln -n ContactosAPI

# 2. Crear el proyecto de pruebas con la plantilla oficial de xUnit
dotnet new xunit -n ContactosAPI.Tests -o src/ContactosAPI.Tests

# 3. Referenciar el proyecto principal desde el de pruebas
dotnet add src/ContactosAPI.Tests reference src/ContactosAPI/ContactosAPI.csproj

# 4. Paquete para probar la API completa en memoria (sin levantar un servidor real)
dotnet add src/ContactosAPI.Tests package Microsoft.AspNetCore.Mvc.Testing --version 10.0.12

# 5. Agregar ambos proyectos a la solución
dotnet sln add src/ContactosAPI/ContactosAPI.csproj src/ContactosAPI.Tests/ContactosAPI.Tests.csproj
```

Borra el archivo `UnitTest1.cs` que genera la plantilla.

## 2. Cambios necesarios en el proyecto principal

Con Minimal API todo está en `Program.cs` como *top-level statements*, y la clase `Program` generada es `internal`.
Los tests no pueden verla ni ver el `record WeatherForecast` (también interno) sin estos dos ajustes.

**a) Al final de `src/ContactosAPI/Program.cs`**, para usar `WebApplicationFactory<Program>`:

```csharp
// Necesario para que WebApplicationFactory<Program> pueda acceder al punto de entrada desde el proyecto de pruebas.
public partial class Program { }
```

**b) Crear `src/ContactosAPI/Properties/AssemblyInfo.cs`**, para que los tests vean los tipos `internal`:

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ContactosAPI.Tests")]
```

## 3. Código de pruebas

### Pruebas unitarias: `src/ContactosAPI.Tests/WeatherForecastTests.cs`

Prueban la lógica pura del `record` (conversión °C → °F) sin levantar la API.

```csharp
namespace ContactosAPI.Tests;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 211)]
    [InlineData(-40, -39)]
    public void TemperatureF_SeCalculaDesdeCelsius(int celsius, int esperadoF)
    {
        var forecast = new WeatherForecast(new DateOnly(2026, 1, 1), celsius, "Test");

        Assert.Equal(esperadoF, forecast.TemperatureF);
    }

    [Fact]
    public void Record_ConservaLosValoresRecibidos()
    {
        var fecha = new DateOnly(2026, 5, 10);

        var forecast = new WeatherForecast(fecha, 25, null);

        Assert.Equal(fecha, forecast.Date);
        Assert.Equal(25, forecast.TemperatureC);
        Assert.Null(forecast.Summary);
    }
}
```

### Pruebas de integración: `src/ContactosAPI.Tests/WeatherForecastEndpointTests.cs`

Levantan la API en memoria con `WebApplicationFactory` y llaman al endpoint `GET /weatherforecast` como lo haría un cliente HTTP.

```csharp
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ContactosAPI.Tests;

public class WeatherForecastEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string[] ResumenesValidos =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    private readonly HttpClient _client;

    public WeatherForecastEndpointTests(WebApplicationFactory<Program> factory)
    {
        // La app usa UseHttpsRedirection, por eso la base es https.
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Get_DevuelveOkYJson()
    {
        var response = await _client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_DevuelveCincoPronosticosConFechasFuturas()
    {
        var pronosticos = await _client.GetFromJsonAsync<JsonElement[]>("/weatherforecast");

        Assert.NotNull(pronosticos);
        Assert.Equal(5, pronosticos.Length);

        var hoy = DateOnly.FromDateTime(DateTime.Now);
        foreach (var p in pronosticos)
            Assert.True(DateOnly.Parse(p.GetProperty("date").GetString()!) > hoy.AddDays(-1));
    }

    [Fact]
    public async Task Get_CadaPronosticoTieneDatosValidos()
    {
        var pronosticos = await _client.GetFromJsonAsync<JsonElement[]>("/weatherforecast");

        Assert.NotNull(pronosticos);
        foreach (var p in pronosticos)
        {
            var c = p.GetProperty("temperatureC").GetInt32();
            var f = p.GetProperty("temperatureF").GetInt32();

            Assert.InRange(c, -20, 54);
            Assert.Equal(32 + (int)(c / 0.5556), f);
            Assert.Contains(p.GetProperty("summary").GetString(), ResumenesValidos);
        }
    }

    [Fact]
    public async Task RutaInexistente_Devuelve404()
    {
        var response = await _client.GetAsync("/no-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MetodoNoPermitido_Devuelve405()
    {
        var response = await _client.PostAsync("/weatherforecast", null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
```

## 4. Ejecutar las pruebas

```powershell
# Todas las pruebas de la solución
dotnet test

# Con detalle de cada prueba
dotnet test --logger "console;verbosity=normal"

# Solo una clase o un test
dotnet test --filter "FullyQualifiedName~WeatherForecastTests"

# Con cobertura (usa coverlet.collector, ya incluido en la plantilla)
dotnet test --collect:"XPlat Code Coverage"
```

Resultado esperado: `Correctas! - Con error: 0, Superado: 9, Omitido: 0, Total: 9`.

## 5. Aclaraciones importantes

- **Unitaria vs. integración:** `WeatherForecastTests` es unitaria (sin infraestructura). `WeatherForecastEndpointTests` es de integración: ejecuta el pipeline real de ASP.NET Core en memoria. Ambas corren con `dotnet test`.
- **Datos aleatorios:** el endpoint usa `Random.Shared` y `DateTime.Now`, así que los tests validan *propiedades* (rangos, cantidad, valores permitidos) y no valores exactos. Evita comparar contra valores fijos: serían tests inestables (*flaky*).
- **Fecha de los tests:** el endpoint usa `DateTime.Now` (hora local). Los tests de fecha toleran el desfase de medianoche comparando con `hoy.AddDays(-1)`. Si más adelante inyectas un `TimeProvider`, podrás probar esto de forma determinista.
- **HTTPS:** `UseHttpsRedirection()` responde 307 a peticiones `http://`; por eso el cliente de prueba usa `https://localhost`.
- **Entorno:** `WebApplicationFactory` arranca en entorno `Development` por defecto, por lo que `MapOpenApi()` está activo (`/openapi/v1.json`). Podrías agregar un test que compruebe que responde 200.
- **Al crecer el proyecto (contactos reales):** cuando agregues endpoints CRUD de contactos, saca la lógica a servicios/repositorios inyectables (`IContactosRepository`) para poder probarlos con mocks (p. ej. NSubstitute o Moq), y en las pruebas de integración reemplaza dependencias con `factory.WithWebHostBuilder(b => b.ConfigureServices(...))`.
- **CI/DevOps:** en el pipeline basta con el paso `dotnet test --configuration Release --no-build` (tras `dotnet build`) y publicar el resultado con `--logger trx`.
- **Versiones:** xUnit 2.9.3 y `Microsoft.AspNetCore.Mvc.Testing` 10.0.12 (misma versión que `Microsoft.AspNetCore.OpenApi` del proyecto). Mantén alineados los paquetes `Microsoft.AspNetCore.*` con la versión del framework.
