using System.Globalization;
//obtenemos el tiempo que hace que ha pasado un evento (en este caso recibir un mensaje) y te lo da a modo
//whatsapp, por ejemplo: "hace 2 min", "ayer", "hace 3 semanas", etc.
public static class DateUtils
{
    public static string GetRelativeTime(string rawTime)
    {
        var formats = new[] { "dd-MM-yyyy HH:mm", "hh:mm tt" };
        if (DateTime.TryParseExact(rawTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            var now = DateTime.Now;
            var diff = now - parsedDate;
            // Console.WriteLine($"Parsed date: {parsedDate}, Now: {now}, Diff: {diff}");
            if (diff.TotalMinutes < 1)
                return "justo ahora";
            if (diff.TotalMinutes < 60)
                return $"hace {Math.Floor(diff.TotalMinutes)} min";
            if (diff.TotalHours < 24)
                return $"hace {Math.Floor(diff.TotalHours)} h";
            if (diff.TotalDays < 2)
                return "ayer";
            if (diff.TotalDays < 7)
                return $"{Math.Floor(diff.TotalDays)} días atrás";
            if (diff.TotalDays < 30)
                return $"Hace {Math.Floor(diff.TotalDays / 7)} semanas";

            return parsedDate.ToString("dd-MM-yyyy HH:mm");
        }

        return rawTime;
    }
}
