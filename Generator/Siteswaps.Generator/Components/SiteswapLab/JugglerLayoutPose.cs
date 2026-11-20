namespace Siteswaps.Generator.Components.SiteswapLab;

public sealed record JugglerLayoutPose(double X, double Z, double Rotation);

public sealed record JugglerLayoutChange(int Person, JugglerLayoutPose Pose);
