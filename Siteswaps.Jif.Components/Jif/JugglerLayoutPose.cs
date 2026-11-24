using Siteswap.Details.Jif;

namespace Siteswaps.Jif.Components;

public sealed record JugglerLayoutPose(double X, double Z, double Rotation);

public sealed record JugglerLayoutChange(
    string Preset,
    IReadOnlyList<JugglerLayoutPose> Poses,
    bool Commit = false
);

public static class JugglerLayoutPresets
{
    public static JugglerLayoutPose[] CreateCircleLayout(int personCount)
    {
        var poses = new JugglerLayoutPose[personCount];
        for (var person = 0; person < personCount; person++)
        {
            var angle = 2 * Math.PI * person / personCount;
            var x = 0.68 * Math.Cos(angle);
            var z = 0.68 * Math.Sin(angle);
            poses[person] = new JugglerLayoutPose(x, z, RotationTowardCenter(x, z));
        }

        return poses;
    }

    public static JugglerLayoutPose[] CreateWvLayout(IReadOnlyList<int> timeZones)
    {
        var poses = new JugglerLayoutPose[timeZones.Count];
        var zones = timeZones.Distinct().Order().ToArray();
        for (var zoneIndex = 0; zoneIndex < zones.Length; zoneIndex++)
        {
            var people = Enumerable
                .Range(0, timeZones.Count)
                .Where(person => timeZones[person] == zones[zoneIndex])
                .ToArray();
            var hasMultipleZones = zones.Length > 1;
            var angle = hasMultipleZones ? 2 * Math.PI * zoneIndex / zones.Length : 0;
            var centerX = hasMultipleZones ? 0.52 * Math.Cos(angle) : 0;
            var centerZ = hasMultipleZones ? 0.52 * Math.Sin(angle) : 0;
            var tangentX = hasMultipleZones ? -Math.Sin(angle) : 1;
            var tangentZ = hasMultipleZones ? Math.Cos(angle) : 0;

            for (var member = 0; member < people.Length; member++)
            {
                var offset = (member - (people.Length - 1) / 2.0) * 0.42;
                var x = centerX + tangentX * offset;
                var z = centerZ + tangentZ * offset;
                poses[people[member]] = new JugglerLayoutPose(x, z, RotationTowardCenter(x, z));
            }
        }

        return poses;
    }

    public static JugglerLayoutPose[] ReadPoses(JifPattern pattern)
    {
        var jugglers = pattern.Jugglers ?? [];
        var defaultLayout = CreateWvLayout(Enumerable.Repeat(0, jugglers.Count).ToArray());
        return jugglers
            .Select(
                (juggler, index) =>
                {
                    if (juggler.Position is null)
                        return defaultLayout[index];

                    var position = juggler.Position ?? [0m, 0m, 0m];
                    var lookAt = juggler.LookAt ?? [0m, 0m, 0m];
                    var x = (double)(position.Count > 0 ? position[0] : 0m) / 4.5;
                    var z = (double)(position.Count > 2 ? position[2] : 0m) / 4.5;
                    var dx =
                        (double)(lookAt.Count > 0 ? lookAt[0] : 0m)
                        - (position.Count > 0 ? (double)position[0] : 0d);
                    var dz =
                        (double)(lookAt.Count > 2 ? lookAt[2] : 0m)
                        - (position.Count > 2 ? (double)position[2] : 0d);
                    var rotation =
                        dx == 0 && dz == 0
                            ? RotationTowardCenter(x, z)
                            : NormalizeRotation(Math.Atan2(dx, -dz) * 180 / Math.PI);
                    return new JugglerLayoutPose(x, z, rotation);
                }
            )
            .ToArray();
    }

    public static JifPattern ApplyToJif(JifPattern pattern, IReadOnlyList<JugglerLayoutPose> layout)
    {
        var jugglers = pattern.Jugglers ?? [];
        if (jugglers.Count != layout.Count)
        {
            throw new ArgumentException(
                "There must be a layout pose for each juggler.",
                nameof(layout)
            );
        }

        const double layoutScale = 4.5;
        return pattern with
        {
            Jugglers = jugglers
                .Select(
                    (juggler, index) =>
                    {
                        var pose = layout[index];
                        var x = pose.X * layoutScale;
                        var z = pose.Z * layoutScale;
                        var radians = pose.Rotation * Math.PI / 180;
                        return juggler with
                        {
                            Position = [RoundLikePassist(x), 0, RoundLikePassist(z)],
                            LookAt =
                            [
                                RoundLikePassist(x + Math.Sin(radians)),
                                0,
                                RoundLikePassist(z - Math.Cos(radians)),
                            ],
                        };
                    }
                )
                .ToArray(),
        };
    }

    private static decimal RoundLikePassist(double value) =>
        decimal.Round((decimal)value, 3, MidpointRounding.AwayFromZero);

    public static double NormalizeRotation(double degrees)
    {
        degrees %= 360;
        return degrees < 0 ? degrees + 360 : degrees;
    }

    private static double RotationTowardCenter(double x, double z)
    {
        var degrees = Math.Atan2(-x, z) * 180 / Math.PI;
        degrees %= 360;
        return degrees < 0 ? degrees + 360 : degrees;
    }
}
