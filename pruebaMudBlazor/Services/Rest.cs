using System.Text.Json;

public class Rest
{
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
                var dateTime = DateTime.Parse(dateTimeStr);
                return dateTime.ToString("dd-MM-yyyy HH:mm");
            }

            return DateTime.Now.ToString("dd-MM-yyyy HH:mm");
        }
        catch
        {
            return DateTime.Now.ToString("dd-MM-yyyy HH:mm");
        }
    }
}