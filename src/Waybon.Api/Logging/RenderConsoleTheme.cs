using Serilog.Sinks.SystemConsole.Themes;

namespace Waybon.Api.Logging;

public static class RenderConsoleTheme
{
    // ===================================
    // Styles
    // ===================================

    private const string Bold = "\u001b[1m";


    // ===================================
    // Colors (ANSI 16)
    // ===================================

    private const string Default = "\u001b[39m";
    private const string Grey = "\u001b[90m";
    private const string Red = "\u001b[31m";
    private const string Green = "\u001b[32m";
    private const string Yellow = "\u001b[33m";
    private const string Blue = "\u001b[34m";
    private const string Magenta = "\u001b[35m";
    private const string Cyan = "\u001b[36m";
    private const string FatalBadge = "\u001b[37;41m"; // white on red


    // ===================================
    // Theme
    // ===================================

    public static AnsiConsoleTheme Theme { get; } = new(new Dictionary<ConsoleThemeStyle, string>
    {
        // Text
        [ConsoleThemeStyle.Text] = Default,
        [ConsoleThemeStyle.SecondaryText] = Grey,
        [ConsoleThemeStyle.TertiaryText] = Grey,

        // Values
        [ConsoleThemeStyle.Invalid] = Yellow,
        [ConsoleThemeStyle.Null] = Blue,
        [ConsoleThemeStyle.Name] = Blue,
        [ConsoleThemeStyle.String] = Cyan,
        [ConsoleThemeStyle.Number] = Magenta,
        [ConsoleThemeStyle.Boolean] = Blue,
        [ConsoleThemeStyle.Scalar] = Cyan,

        // Levels
        [ConsoleThemeStyle.LevelVerbose] = Grey,
        [ConsoleThemeStyle.LevelDebug] = Bold + Blue,
        [ConsoleThemeStyle.LevelInformation] = Bold + Green,
        [ConsoleThemeStyle.LevelWarning] = Bold + Yellow,
        [ConsoleThemeStyle.LevelError] = Bold + Red,
        [ConsoleThemeStyle.LevelFatal] = Bold + FatalBadge
    });
}