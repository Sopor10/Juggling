using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class AnalyzeSiteswapTool
{
    [McpServerTool]
    [Description(
        "Analyzes a siteswap and returns detailed information including orbits, states, period, number of objects, and other properties."
    )]
    public ToolResult<SiteswapAnalysis> AnalyzeSiteswap(
        [Description("The siteswap string to analyze (e.g., '5,3,1', '4,4,1', 'a,7,2,4,2')")]
            string siteswap,
        [Description("Number of jugglers (defaults to 2)")]
        [DefaultValue(2)]
            int numberOfJugglers = 2
    )
    {
        return ToolResult.From(() =>
        {
            var coreSiteswap = SiteswapMapper.ToCoreFormat(siteswap);
            if (string.IsNullOrWhiteSpace(coreSiteswap))
            {
                throw new ArgumentException(
                    "Siteswap string cannot be null or empty.",
                    nameof(siteswap)
                );
            }

            if (numberOfJugglers < 1)
            {
                throw new ArgumentException(
                    "Number of jugglers must be at least 1.",
                    nameof(numberOfJugglers)
                );
            }

            if (
                !Siteswap.Details.Siteswap.TryCreate(coreSiteswap, out var siteswapObj)
                || siteswapObj == null
            )
            {
                throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
            }

            var orbits = siteswapObj.GetOrbits();
            var state = siteswapObj.State;
            var allStates = siteswapObj.AllStates();

            var passOrSelf = siteswapObj
                .GetPassOrSelf(numberOfJugglers)
                .Select(MapPassOrSelf)
                .ToList();
            var interfacePassOrSelf = siteswapObj
                .Interface.GetPassOrSelf(numberOfJugglers)
                .Select(MapPassOrSelf)
                .ToList()
                .Aggregate((x, y) => x + y);
            var clubs = siteswapObj.GetClubDistribution(numberOfJugglers);
            var jugglers = Enumerable
                .Range(0, numberOfJugglers)
                .Select(i =>
                {
                    var local = siteswapObj.GetLocalSiteswap(i, numberOfJugglers);
                    return new JugglerInfo
                    {
                        JugglerIndex = i,
                        LocalNotation = SiteswapMapper.LocalToDisplayFormat(local.LocalNotation),
                        GlobalNotation = SiteswapMapper.ToDisplayFormat(local.GlobalNotation),
                        AverageObjects = local.Average(),
                        ClubDistribution = string.Join(
                            "|",
                            clubs.Hands.Where(x => x.Item1.Juggler == i).Select(x => x.Item2)
                        ),
                    };
                })
                .ToList();

            return new SiteswapAnalysis
            {
                Siteswap = SiteswapMapper.ToDisplayFormat(siteswapObj),
                Period = siteswapObj.Period.Value,
                NumberOfObjects = siteswapObj.NumberOfObjects,
                MaxHeight = siteswapObj.Max(),
                Length = siteswapObj.Length,
                IsExcitedState = siteswapObj.IsExcitedState(),
                CurrentState = state.ToString(),
                Orbits = orbits
                    .Select(o => new OrbitInfo
                    {
                        DisplayValue = string.Join(
                            ",",
                            o.Items.Select(Siteswap.Details.Siteswap.Transform)
                        ),
                        Items = o.Items,
                    })
                    .ToList(),
                AllStates = allStates
                    .Select(kvp => new StateInfo
                    {
                        State = kvp.Key.ToString(),
                        Siteswaps = kvp
                            .Value.Select(s => SiteswapMapper.ToDisplayFormat(s))
                            .ToList(),
                    })
                    .ToList(),
                NumberOfJugglers = numberOfJugglers,
                PassOrSelf = passOrSelf,
                Interface = interfacePassOrSelf,
                Jugglers = jugglers,
            };
        });
    }

    private static string MapPassOrSelf(PassOrSelf passOrSelf) =>
        passOrSelf switch
        {
            PassOrSelf.Pass => "p",
            PassOrSelf.Self => "s",
            _ => throw new ArgumentOutOfRangeException(nameof(passOrSelf), passOrSelf, null),
        };
}

public class SiteswapAnalysis
{
    public string Siteswap { get; set; } = string.Empty;
    public int Period { get; set; }
    public decimal NumberOfObjects { get; set; }
    public int MaxHeight { get; set; }
    public int Length { get; set; }
    public bool IsExcitedState { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public List<OrbitInfo> Orbits { get; set; } = new();
    public List<StateInfo> AllStates { get; set; } = new();
    public required string Interface { get; set; }
    public int NumberOfJugglers { get; set; }
    public List<string> PassOrSelf { get; set; } = new();
    public List<JugglerInfo> Jugglers { get; set; } = new();
}

public class JugglerInfo
{
    public int JugglerIndex { get; set; }
    public string LocalNotation { get; set; } = string.Empty;
    public string GlobalNotation { get; set; } = string.Empty;
    public double AverageObjects { get; set; }
    public required string ClubDistribution { get; set; }
}

public class OrbitInfo
{
    public string DisplayValue { get; set; } = string.Empty;
    public List<int> Items { get; set; } = new();
}

public class StateInfo
{
    public string State { get; set; } = string.Empty;
    public List<string> Siteswaps { get; set; } = new();
}
