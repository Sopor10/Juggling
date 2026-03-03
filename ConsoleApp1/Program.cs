using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using LocalSiteswap = Siteswap.Details.LocalSiteswap;

var res = new List<Siteswaps.Generator.Core.Generator.Siteswap>();
foreach (var i in Enumerable.Range(1, 15))
{
    var input = new SiteswapGeneratorInput(10, i, 2, 11)
    {
        StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 50000000),
    };
    var siteswaps = await new SiteswapGenerator(
        new FilterBuilder(input)
            // .ExactNumberOfPasses(6, 2)
            .InterfaceFilter(
                [
                    [-3],
                    [-2],
                    [-2],
                    [-3],
                    [-3],
                    [-3],
                    [-3],
                    [-3],
                    [-3],
                    [-3],
                ],
                2
            )
            .ExactOccurence([3], 0)
            .ExactOccurence([1], 0)
            .Build(),
        input
    ).GenerateAsync(new CancellationTokenSource().Token).ToListAsync();
    res.AddRange(siteswaps);
}

var result = res.Select(x => new Siteswap.Details.Siteswap(x.ToString())).ToList();

var locals = new List<LocalSiteswap>();
foreach (var s in result)
{
    locals.Add(s.GetLocalSiteswap(0, 2));
    locals.Add(s.GetLocalSiteswap(1, 2));
}

var unique = locals.Select(x => x.UniqueGlobalNotation).Distinct().ToList();
foreach (var se in unique)
{
    Console.WriteLine(se);
}

Console.WriteLine(unique.Count);
