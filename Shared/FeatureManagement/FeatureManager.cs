
using System.Threading.Tasks;
using FeatureManagement.Abstractions;

namespace FeatureManagement;

public class FeatureManager : IFeatureManager
{
    private readonly Microsoft.FeatureManagement.IFeatureManager featureManager;

    public FeatureManager(Microsoft.FeatureManagement.IFeatureManager featureManager)
    {
        this.featureManager = featureManager;
    }

    public Task<bool> IsEnabledAsync(string feature) => featureManager.IsEnabledAsync(feature);
}