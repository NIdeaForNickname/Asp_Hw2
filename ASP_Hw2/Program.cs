using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var weatherApi = builder.Configuration["WeatherAPI:Key"];

app.Map("/{city}", async (string city, HttpContext context) =>
{
    var client = new HttpClient();
    var url = $"http://api.weatherapi.com/v1/forecast.json?key={weatherApi}&q={city}&days=3&aqi=no&alerts=no";

    try
    {
        var weather = await client.GetFromJsonAsync<WeatherResponse>(url);

        if (weather.Forecast == null || weather.Location == null) throw new Exception("Data missing");

        await context.Response.WriteAsync($"""
                                           <html>
                                           <body>
                                               <h1>7-Day Forecast for {weather.Location.Name}, {weather.Location.Country}</h1>
                                               <table border="1" cellpadding="5">
                                                   <tr>
                                                       <th>Date</th>
                                                       <th>Condition</th>
                                                       <th>Avg Temp (C)</th>
                                                       <th>Avg Humidity (%)</th>
                                                   </tr>
                                           """);

        foreach (var day in weather.Forecast.Forecastday)
        {
            await context.Response.WriteAsync($"""
                                                   <tr>
                                                       <td>{day.Date}</td>
                                                       <td><img alt="{day.Day.Condition.Text}" src="{day.Day.Condition.Icon}"></td>
                                                       <td>{day.Day.avgtemp_c}</td>
                                                       <td>{day.Day.avghumidity}</td>
                                                   </tr>
                                               """);
        }

        await context.Response.WriteAsync("</table></body></html>");
        
        

    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.Message}");
        await context.Response.WriteAsync($"{ex.Message}");
    }
});

app.Map("/", async (context) =>
{
    await context.Response.WriteAsync("""
                                      <html>
                                      <body>
                                        <a href="/london"> London </a>
                                        <br>
                                        <a href="/berlin"> Berlin </a>
                                        <br>
                                        <a href="/paris"> Paris </a>
                                      </body>
                                      </html>
                                      """);
});

app.Run();

public class WeatherResponse
{
    public Location? Location { get; set; }
    public Forecast? Forecast { get; set; }
}


public class Location
{
    public string Name { get; set; }
    public string Country { get; set; }
}

public class Forecast
{
    public List<Forecastday> Forecastday { get; set; }
}

public class Forecastday
{
    public string Date { get; set; }
    public Day Day { get; set; }
}

public class Day
{
    public float avgtemp_c { get; set; }

    public Condition Condition { get; set; }
    
    public int avghumidity { get; set; }
}

public class Condition
{
    public string Text { get; set; }
    
    public string Icon { get; set; }
}