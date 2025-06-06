using System.Runtime.InteropServices;
using System.Text.Json;

public class Rest
{
    //al principio lo usaba de comodin para los criterios del trabajo pero ahora me he dado cuenta que viene genial para
    //obtener hora con el gmt unificado porque los .now de todos los objetos DateTime son UTC
    public async Task<string> GetMadridTimeFormatted()
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync("https://timeapi.io/api/Time/current/zone?timeZone=Europe/Madrid");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(content);
            var root = json.RootElement;

            int year = root.GetProperty("year").GetInt32();
            int month = root.GetProperty("month").GetInt32();
            int day = root.GetProperty("day").GetInt32();
            int hour = root.GetProperty("hour").GetInt32();
            int minute = root.GetProperty("minute").GetInt32();

            var dateTime = new DateTime(year, month, day, hour, minute, 0);
            Console.WriteLine($"Fecha y hora obtenida de la API: {dateTime}");

            return dateTime.ToString("dd-MM-yyyy HH:mm");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Error HTTP al obtener la hora de la API, usando la hora local.");
            Console.WriteLine("Error: " + ex.Message);
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Error JSON al obtener la hora de la API, usando la hora local.");
            Console.WriteLine("Error: " + ex.Message);
        }

        try
        {
            var madridTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Romance Standard Time" : "Europe/Madrid");

            var madridTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, madridTimeZone);
            Console.WriteLine("Hora calculada localmente: " + madridTime);
            return madridTime.ToString("dd-MM-yyyy HH:mm");
        }
        catch (TimeZoneNotFoundException ex)
        {
            Console.WriteLine("Zona horaria de Madrid no encontrada. Error: " + ex.Message);
            return DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm");
        }
    }
}