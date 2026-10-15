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
