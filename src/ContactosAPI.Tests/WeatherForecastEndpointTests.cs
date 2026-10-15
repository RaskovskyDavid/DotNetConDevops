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
