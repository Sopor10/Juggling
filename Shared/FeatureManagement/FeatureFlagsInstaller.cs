using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;


namespace FeatureManagement;

public static class FeatureFlagsInstaller
{
    public static void InstallFeatureFlags(this IServiceCollection services)
    {
        services.AddFeatureManagement();
        services.AddSingleton<FeatureManagement.Abstractions.IFeatureManager, FeatureManager>();
    }
}