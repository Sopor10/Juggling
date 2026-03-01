using Dunet;

namespace Siteswap.Details;

[Union]
public partial record Result<T>
{
    public partial record Success(T Value);

    public partial record Failure(string Error);
}
