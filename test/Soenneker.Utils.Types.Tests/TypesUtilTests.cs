using Soenneker.Utils.Types.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Utils.Types.Tests;

[Collection("Collection")]
public class TypesUtilTests : FixturedUnitTest
{
    private readonly ITypesUtil _util;

    public TypesUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ITypesUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
