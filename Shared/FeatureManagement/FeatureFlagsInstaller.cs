using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;


namespace FeatureManagement;

public static class FeatureFlagsInstaller
{
    public static IServiceCollection InstallFeatureFlags(this IServiceCollection services)
    {
        services.AddFeatureManagement();
        services.AddSingleton<FeatureManagement.Abstractions.IFeatureManager, FeatureManager>();
        return services;
    }
}