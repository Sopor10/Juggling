using System.Text.Json;
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

    [CommandOption("jugglers", 'j', Description = "Number of jugglers for analysis.")]
    public int NumberOfJugglers { get; init; } = 2;

    public ValueTask ExecuteAsync(IConsole console)
    {
        if (string.IsNullOrWhiteSpace(Siteswap))
        {
            console.Error.WriteLine("No siteswap specified.");
            return default;
        }

        var isValid = SiteswapDetails.TryCreate(Siteswap, out var siteswapObj);
        if (isValid && siteswapObj != null)
        {
            var orbits = siteswapObj.GetOrbits().Select(x => x.DisplayValue);
            var state = siteswapObj.State;

            var interfacePassOrSelf = siteswapObj
                .Interface.GetPassOrSelf(NumberOfJugglers)
                .Select(MapPassOrSelf)
                .ToList()
                .Aggregate((x, y) => x + y);
            var clubs = siteswapObj.GetClubDistribution(NumberOfJugglers);
            var jugglers = Enumerable
                .Range(0, NumberOfJugglers)
                .Select(i =>
                {
                    var local = siteswapObj.GetLocalSiteswap(i, NumberOfJugglers);
                    return new JugglerInfo
                    {
                        JugglerIndex = i,
                        LocalNotation = local.LocalNotation,
                        GlobalNotation = local.GlobalNotation,
                        AverageObjects = local.Average(),
                        ClubDistribution = string.Join(
                            "|",
                            clubs.Hands.Where(x => x.Item1.Juggler == i).Select(x => x.Item2)
                        ),
                    };
                })
                .ToList();

            var result = new ValidateResult
            {
                IsValid = true,
                Siteswap = siteswapObj.ToString(),
                Period = siteswapObj.Period.Value,
                NumberOfObjects = siteswapObj.NumberOfObjects,
                MaxHeight = siteswapObj.Max(),
                Length = siteswapObj.Length,
                IsGroundState = siteswapObj.IsGroundState(),
                CurrentState = state.ToString(),
                Orbits = orbits.ToList(),
                Interface = interfacePassOrSelf,
                Jugglers = jugglers,
                State = siteswapObj.State.ToString()
            };
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            console.Output.WriteLine(json);
        }
        else
        {
            var result = new ValidationError();
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            console.Output.WriteLine(json);
        }

        return default;
    }

    private static string MapPassOrSelf(Siteswap.Details.PassOrSelf passOrSelf) =>
        passOrSelf switch
        {
            global::Siteswap.Details.PassOrSelf.Pass => "p",
            global::Siteswap.Details.PassOrSelf.Self => "s",
            _ => throw new ArgumentOutOfRangeException(nameof(passOrSelf), passOrSelf, null),
        };
}

public record ValidationError
{
    public bool IsValid => false;
}

public record ValidateResult
{
    public required bool IsValid { get; init; }
    public required string Siteswap { get; init; }
    public required int Period { get; init; }
    public required decimal NumberOfObjects { get; init; }
    public required int MaxHeight { get; init; }
    public required int Length { get; init; }
    public required bool IsGroundState { get; init; }
    public required string CurrentState { get; init; }
    public required List<string> Orbits { get; init; }
    public required string State { get; init; }
    public required string Interface { get; init; }
    public required List<JugglerInfo> Jugglers { get; init; }
}

public class JugglerInfo
{
    public int JugglerIndex { get; set; }
    public string LocalNotation { get; set; } = string.Empty;
    public string GlobalNotation { get; set; } = string.Empty;
    public double AverageObjects { get; set; }
    public required string ClubDistribution { get; set; }
}