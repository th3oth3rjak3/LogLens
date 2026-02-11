using Spectre.Console;

using static LogLens.Shared;

namespace LogLens.Commands;

public class PagedCommand(FileInfo file, string? searchText) : ICommand
{
    private const int PageSize = 3;

    public CommandResult Execute()
    {
        if (!file.Exists)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File not found at '{file.FullName}'");
            return CommandResult.FileNotFound;
        }

        try
        {
            using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream);

            string? line;
            int linesPrinted = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // If search text is specified and the line does not contain the search text, skip to the next line.
                if (searchText is not null && !line.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (linesPrinted % PageSize == 0)
                {
                    AnsiConsole.MarkupLine($"[bold yellow]Viewing log file:[/] {file.FullName}");
                    AnsiConsole.WriteLine();
                }

                RenderLogLine(line);

                linesPrinted++;
                if (linesPrinted % PageSize == 0 && !reader.EndOfStream)
                {
                    // ShouldQuitPaging() pauses and waits for user input.
                    if (ShouldQuitPaging())
                    {
                        return CommandResult.UserQuit;
                    }

                    AnsiConsole.Clear();
                }
            }

            return linesPrinted == 0 ? CommandResult.NoMatchingEntries : CommandResult.Success;
        }
        catch (IOException ex)
        {
            AnsiConsole.MarkupLine($"[red]IO Error:[/] Could not read file. Details: {ex.Message}");
            return CommandResult.FileNotFound;
        }
    }
}
