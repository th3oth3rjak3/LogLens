using System.CommandLine;

namespace LogLens;

public static class Arguments
{
    /// <summary>
    /// Gets the command line argument for using a directory for searching all log files.
    /// </summary>
    public static readonly Argument<string> PathArgument = new("path")
    {
        Description = "The path to a file or directory containing json log files.",
    };

    /// <summary>
    /// Gets the command line argument for using a search text.
    /// </summary>
    public static readonly Argument<string> SearchTextArgument = new("text")
    {
        Description = "The text to search for within the log entries.",
    };
}
