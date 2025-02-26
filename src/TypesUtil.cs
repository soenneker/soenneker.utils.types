using Soenneker.Utils.Types.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Soenneker.Extensions.String;

namespace Soenneker.Utils.Types;

/// <inheritdoc cref="ITypesUtil"/>
public class TypesUtil: ITypesUtil
{
    private readonly ConcurrentDictionary<string, Type> _typeCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, List<Assembly>> _assembliesCache = new(StringComparer.Ordinal);

    [Pure]
    public Type? GetTypeByNameCached(string className, string solutionName, List<Assembly>? assemblies = null)
    {
        if (_typeCache.TryGetValue(className, out Type? cachedType))
            return cachedType;

        assemblies ??= GetSolutionAssembliesCached(solutionName);

        for (var i = 0; i < assemblies.Count; i++)
        {
            Type[] types = assemblies[i].GetTypes();

            for (var j = 0; j < types.Length; j++)
            {
                if (types[j].Name.EqualsIgnoreCase(className))
                {
                    _typeCache[className] = types[j];
                    return types[j];
                }
            }
        }

        return null;
    }

    [Pure]
    public List<Assembly> GetSolutionAssembliesCached(string solutionName)
    {
        if (_assembliesCache.TryGetValue(solutionName, out List<Assembly>? cachedAssemblies))
            return cachedAssemblies;

        List<Assembly> assemblies = GetSolutionAssemblies(solutionName);

        _assembliesCache.TryAdd(solutionName, assemblies);

        return assemblies;
    }

    /// <summary>
    /// Gets a Type instance matching the specified class name (not namespace-qualified).
    /// </summary>
    /// <param name="className">Name of the class sought.</param>
    /// <param name="solutionName"></param>
    /// <param name="assemblies">Optional list of assemblies to search in. If null, it searches solution assemblies.</param>
    /// <returns>Matching Type, or null if not found.</returns>
    [Pure]
    public static Type? GetTypeByName(string className, string solutionName, List<Assembly>? assemblies = null)
    {
        assemblies ??= GetSolutionAssemblies(solutionName);

        for (var i = 0; i < assemblies.Count; i++)
        {
            Type[] types = assemblies[i].GetTypes();

            for (var j = 0; j < types.Length; j++)
            {
                if (types[j].Name.EqualsIgnoreCase(className))
                {
                    return types[j];
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves all assemblies belonging to the current solution.
    /// </summary>
    /// <returns>List of assemblies belonging to the solution.</returns>
    [Pure]
    public static List<Assembly> GetSolutionAssemblies(string solutionName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Assembly> returnAssemblies = new(assemblies.Length); // Preallocate list size

        for (var i = 0; i < assemblies.Length; i++)
        {
            string? fullName = assemblies[i].FullName;

            if (fullName != null && fullName.AsSpan().StartsWith(solutionName.AsSpan(), StringComparison.Ordinal))
            {
                returnAssemblies.Add(assemblies[i]);
            }
        }

        return returnAssemblies;
    }
}
