using Serilog.Sinks.SystemConsole.Themes;

namespace Waybon.Api.Logging;

public static class LocalConsoleTheme
{
    // ===================================
    // Styles
    // ===================================

    private const string Bold = "\u001b[1m";


    // ===================================
    // Colors (ANSI 256)
    // ===================================

    private const string Light = "\u001b[38;5;253m";                // #dadada  message text
    private const string Grey = "\u001b[38;5;246m";                 // #949494  timestamp, stack frames
    private const string DarkGrey = "\u001b[38;5;242m";             // #6c6c6c  brackets, verbose
    private const string Mint = "\u001b[38;5;151m";                 // #afd7af  info
    private const string Sky = "\u001b[38;5;153m";                  // #afd7ff  debug, scalars
    private const string Aqua = "\u001b[38;5;152m";                 // #afd7d7  strings, class names
    private const string Lavender = "\u001b[38;5;183m";             // #d7afff  numbers
    private const string Periwinkle = "\u001b[38;5;189m";           // #d7d7ff  property names
    private const string Butter = "\u001b[38;5;229m";               // #ffffaf  booleans
    private const string DustyRose = "\u001b[38;5;181m";            // #d7afaf  null
    private const string Salmon = "\u001b[38;5;216m";               // #ffaf87  invalid values
    private const string Gold = "\u001b[38;5;222m";                 // #ffd787  warning
    private const string Coral = "\u001b[38;5;210m";                // #ff8787  error
    private const string FatalBadge = "\u001b[38;5;16;48;5;217m";   // black on #ffafaf  fatal


    // ===================================
    // Theme
    // ===================================

    public static AnsiConsoleTheme Theme { get; } = new(new Dictionary<ConsoleThemeStyle, string>
    {
        // Text
        [ConsoleThemeStyle.Text] = Light,
        [ConsoleThemeStyle.SecondaryText] = Grey,
        [ConsoleThemeStyle.TertiaryText] = DarkGrey,

        // Values
        [ConsoleThemeStyle.Invalid] = Salmon,
        [ConsoleThemeStyle.Null] = DustyRose,
        [ConsoleThemeStyle.Name] = Periwinkle,
        [ConsoleThemeStyle.String] = Aqua,
        [ConsoleThemeStyle.Number] = Lavender,
        [ConsoleThemeStyle.Boolean] = Butter,
        [ConsoleThemeStyle.Scalar] = Sky,

        // Levels
        [ConsoleThemeStyle.LevelVerbose] = DarkGrey,
        [ConsoleThemeStyle.LevelDebug] = Bold + Sky,
        [ConsoleThemeStyle.LevelInformation] = Bold + Mint,
        [ConsoleThemeStyle.LevelWarning] = Bold + Gold,
        [ConsoleThemeStyle.LevelError] = Bold + Coral,
        [ConsoleThemeStyle.LevelFatal] = Bold + FatalBadge
    });
}