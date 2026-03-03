using System.Collections.Concurrent;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

var res = new ConcurrentBag<string>();
await Parallel.ForEachAsync(
    Enumerable.Range(1, 15),
    async (i, token) =>
    {
        var input = new SiteswapGeneratorInput(14, i, 2, 11)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(600), 500_000_000),
        };
        var siteswaps = new SiteswapGenerator(
            new FilterBuilder(input)
                .Not(new FilterBuilder(input).ExactNumberOfPasses(0, 2).Build())
                .ExactOccurence([3], 0)
                .ExactOccurence([1], 0)
                .Build(),
            input
        ).GenerateAsync(new CancellationTokenSource().Token);

        await foreach (var siteswap in siteswaps)
        {
            var s = new Siteswap.Details.Siteswap(siteswap.ToString());
            res.Add(s.GetLocalSiteswap(0).UniqueGlobalNotation);
            res.Add(s.GetLocalSiteswap(1).UniqueGlobalNotation);
        }
    }
);

// foreach (var se in res.Distinct())
// {
//     Console.WriteLine(se);
// }

Console.WriteLine("count: " + res.Count);
