using System.Text.Json;

using Spectre.Console;
using Spectre.Console.Json;

namespace LogLens;

public static class Shared
{
    public static bool ShouldQuitPaging()
    {
        AnsiConsole.Markup("[grey]-- Press Enter to show more, or Q to quit --[/]");
        var keyInfo = Console.ReadKey(true);
        Console.CursorLeft = 0;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.CursorLeft = 0;
        return keyInfo.Key == ConsoleKey.Q;
    }

    public static string GetTimestampHeader(string jsonLine)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonLine);
            if (doc.RootElement.TryGetProperty("time", out var timeProperty) && timeProperty.ValueKind == JsonValueKind.String)
            {
                var timeString = timeProperty.GetString();
                if (!string.IsNullOrEmpty(timeString) && DateTime.TryParse(timeString, out var parsedTime))
                {
                    var utcTime = DateTime.SpecifyKind(parsedTime, DateTimeKind.Utc);
                    return utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
            }
        }
        catch (JsonException) { /* Fall through to default */ }

        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    public static JsonText CreateStyledJsonText(string jsonLine)
    {
        var jsonText = new JsonText(jsonLine)
        {
            // Your chosen styles for high contrast
            MemberStyle = new Style(Color.Blue, Color.Black),
            StringStyle = new Style(Color.White, Color.Black),
            NumberStyle = new Style(Color.Aqua, Color.Black),
            BooleanStyle = new Style(Color.Fuchsia, Color.Black),
            NullStyle = new Style(Color.Grey, Color.Black)
        };
        return jsonText;
    }

    public static Color GetColorForLogLevel(string jsonText)
    {
        if (jsonText.Contains("\"Error\"", StringComparison.OrdinalIgnoreCase)) return Color.Red;
        if (jsonText.Contains("\"Warning\"", StringComparison.OrdinalIgnoreCase)) return Color.Yellow;
        if (jsonText.Contains("\"Information\"", StringComparison.OrdinalIgnoreCase)) return Color.Blue;
        return Color.Grey;
    }

    public static void RenderLogLine(string line)
    {
        try
        {
            string timestampHeader = GetTimestampHeader(line);
            var jsonText = CreateStyledJsonText(line);

            var jsonPanel = new Panel(jsonText)
                .Header($"[grey]{timestampHeader}[/]")
                .Collapse()
                .RoundedBorder()
                .BorderColor(GetColorForLogLevel(line));

            AnsiConsole.Write(jsonPanel);
        }
        catch (JsonException)
        {
            AnsiConsole.MarkupLine($"[dim]Not JSON:[/] {line}");
        }
    }
}
