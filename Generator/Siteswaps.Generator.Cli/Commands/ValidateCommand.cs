using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Generator.Cli.Commands;

[Command("validate", Description = "Validates a siteswap.")]
public class ValidateCommand : ICommand
{
    [CommandParameter(0, Name = "siteswap", Description = "The siteswap to validate (e.g. 531).")]
    public string Siteswap { get; init; } = "";

    public ValueTask ExecuteAsync(IConsole console)
    {
        if (string.IsNullOrWhiteSpace(Siteswap))
        {
            console.Error.WriteLine("No siteswap specified.");
            return default;
        }

        if (SiteswapDetails.TryCreate(Siteswap, out var siteswap))
        {
            console.Output.WriteLine($"The siteswap '{Siteswap}' is valid.");
            console.Output.WriteLine($"Number of objects: {siteswap.NumberOfObjects}");
        }
        else
        {
            console.Error.WriteLine($"The siteswap '{Siteswap}' is invalid.");
        }

        return default;
    }
}
