using System.CommandLine;

using LogLens.Commands;

using Spectre.Console;

namespace LogLens;

public static class Program
{
    public static void Main(string[] args)
    {
        var pathArgument = Arguments.PathArgument;

        RootCommand rootCommand = new("LogLens: Prettifies a structured JSON log file.")
        {
            pathArgument,
        };

        rootCommand.SetAction(parseResult =>
        {
            var path = parseResult.GetValue(pathArgument);

            if (path is null)
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Path is required.");
                return;
            }

            HandlePathSearch(path, null);
        });

        var searchTextArgument = Arguments.SearchTextArgument;
        var searchCommand = new Command("search", "Filters log entries containing specific text.")
        {
            pathArgument,
            searchTextArgument,
        };

        searchCommand.SetAction(parseResult =>
        {
            var path = parseResult.GetValue(pathArgument);
            var text = parseResult.GetValue(searchTextArgument);

            if (path is null || string.IsNullOrEmpty(text))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Both a file path and search text are required.");
                return;
            }

            HandlePathSearch(path, text);
        });

        rootCommand.Add(searchCommand);

        rootCommand.Parse(args).Invoke();
    }

    private static void HandlePathSearch(string path, string? searchText)
    {
        if (File.Exists(path))
        {
            new PagedCommand(new FileInfo(path), null).Execute();
        }
        else if (Directory.Exists(path))
        {
            AnsiConsole.MarkupLine($"[bold yellow]Searching directory:[/] {path}");
            var files = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No '.json' files found in this directory.[/]");
                return;
            }

            foreach (var filePath in files)
            {
                var result = new PagedCommand(new FileInfo(filePath), searchText).Execute();

                if (result != CommandResult.NoMatchingEntries)
                {
                    AnsiConsole.Write(new Rule($"End of {Path.GetFileName(filePath)}").RuleStyle("grey").Centered());
                    AnsiConsole.WriteLine();
                }

                if (result == CommandResult.UserQuit)
                {
                    return;
                }
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Path not found: '{path}'");
        }
    }
}