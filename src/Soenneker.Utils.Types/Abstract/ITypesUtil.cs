using System;
using System.Collections.Generic;
using System.Reflection;

namespace Soenneker.Utils.Types.Abstract;

/// <summary>
/// A utility library for Type and Assembly related operations
/// </summary>
public interface ITypesUtil
{
    Type? GetTypeByNameCached(string className, string solutionName, List<Assembly>? assemblies = null);

    List<Assembly> GetSolutionAssembliesCached(string solutionName);
}
