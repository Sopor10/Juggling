namespace Siteswap.Details;

public class Orbit(List<int> items)
{
    public List<int> Items => items;
    public string DisplayValue => Items.ToSiteswapString();
    public bool HasBalls => new Siteswap(Items.ToArray()).NumberOfObjects() > 0;

    public static IEnumerable<Orbit> CreateFrom(Siteswap siteswap)
    {
        var visited = new bool[siteswap.Items.Length];

        for (var i = 0; i < siteswap.Length; i++)
        {
            if (visited[i])
            {
                continue;
            }

            var orbitIndices = new List<int>();
            var current = i;

            // Sammle alle Indizes im aktuellen Orbit
            do
            {
                visited[current] = true;
                orbitIndices.Add(current);
                current = (current + siteswap.Items[current]) % siteswap.Length;
            } while (current != i);

            // Erstelle eine Orbit-Liste mit 0 als Standardwert
            var orbitValues = new int[siteswap.Length];
            for (var j = 0; j < orbitValues.Length; j++)
            {
                orbitValues[j] = 0;
            }

            // Setze die Werte an den Orbit-Positionen
            foreach (var index in orbitIndices)
            {
                orbitValues[index] = siteswap.Items[index];
            }

            var orbit = new Orbit(orbitValues.ToList());
            if (orbit.HasBalls)
            {
                yield return orbit;
            }
        }
    }
}
