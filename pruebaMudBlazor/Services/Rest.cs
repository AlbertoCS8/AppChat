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
            var response = await httpClient.GetAsync("http://worldtimeapi.org/api/timezone/Europe/Madrid");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(content);
            var dateTimeStr = json.RootElement.GetProperty("datetime").GetString();

            if (dateTimeStr != null)
            {
                Console.WriteLine($"Fecha y hora obtenida de la API: {dateTimeStr}");
                var dateTime = DateTime.Parse(dateTimeStr);
                return dateTime.ToString("dd-MM-yyyy HH:mm");
            }
            Console.WriteLine("No se pudo obtener la fecha y hora de la API, usando la hora local.");
            return DateTime.Now.ToString("dd-MM-yyyy HH:mm");
        }
        catch(HttpRequestException ex)
        {
            Console.WriteLine("Error al realizar la solicitud HTTP a la API de hora, usando la hora local.");
            Console.WriteLine("Error: " + ex.Message);
            return DateTime.Now.ToString("dd-MM-yyyy HH:mm");
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Error al obtener la fecha y hora de la API, usando la hora local.");
            Console.WriteLine("Error: " + ex.Message);
            return DateTime.Now.ToString("dd-MM-yyyy HH:mm");
        }
    }
}