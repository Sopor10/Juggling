using System.Threading.Tasks;

namespace FeatureManagement.Abstractions;

public interface IFeatureManager
{
    Task<bool> IsEnabledAsync(string feature);
}