using System;

namespace Soenneker.Utils.Types.Tests;

public sealed class RegisteredTypesTests
{
    [Test]
    public void Registration_is_scoped_case_insensitive_and_replaced_atomically()
    {
        var util = new TypesUtil();
        util.RegisterTypes("first", [typeof(string), typeof(int)]);
        util.RegisterTypes("second", [typeof(decimal)]);
        if (util.GetRegisteredTypeByName("STRING", "first") != typeof(string) ||
            util.GetRegisteredTypeByName("String", "second") != null)
            throw new Exception("Registration scopes or name matching are incorrect.");
        util.RegisterTypes("first", [typeof(bool)]);
        if (util.GetRegisteredTypeByName("String", "first") != null ||
            util.GetRegisteredTypeByName("Boolean", "first") != typeof(bool))
            throw new Exception("Replacing registration retained stale types.");
    }
}
