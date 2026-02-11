namespace LogLens.Commands;

/// <summary>
/// A command that can be executed as part of the CLI tool.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <returns>A flag indicating whether to continue future commands or to exit. When true, continue, otherwise exit.</returns>
    public CommandResult Execute();
}
