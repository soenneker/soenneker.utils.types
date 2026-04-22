using Soenneker.Utils.Types.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Utils.Types.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class TypesUtilTests : HostedUnitTest
{
    private readonly ITypesUtil _util;

    public TypesUtilTests(Host host) : base(host)
    {
        _util = Resolve<ITypesUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
