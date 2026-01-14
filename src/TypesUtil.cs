using Soenneker.Utils.Types.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Soenneker.Extensions.String;

namespace Soenneker.Utils.Types;

/// <summary>
/// High-performance utilities for resolving <see cref="Type"/> instances by simple (non-qualified) name
/// across solution-scoped assemblies.
/// </summary>
/// <remarks>
/// Optimized for repeated lookups:
/// <list type="bullet">
/// <item>Assemblies are cached per solution name.</item>
/// <item>Types are indexed once per solution for O(1) lookup.</item>
/// <item>Negative lookups are memoized to avoid repeated reflection scans.</item>
/// </list>
/// Cache keys are solution-scoped to prevent cross-solution type collisions.
/// </remarks>
public sealed class TypesUtil : ITypesUtil
{
    private static readonly StringComparison _ordIgnore = StringComparison.OrdinalIgnoreCase;

    // solutionName -> assemblies
    private readonly ConcurrentDictionary<string, Assembly[]> _assembliesCache = new(StringComparer.Ordinal);

    // solutionName -> (typeName -> Type)
    private readonly ConcurrentDictionary<string, Dictionary<string, Type>> _solutionTypeIndexCache = new(StringComparer.Ordinal);

    // (solutionName, typeName) -> miss marker
    private readonly ConcurrentDictionary<(string Solution, string Name), byte> _missCache = new();

    /// <summary>
    /// Retrieves a <see cref="Type"/> by its simple (non-namespace-qualified) name using cached indexes.
    /// </summary>
    /// <param name="className">The simple type name to locate.</param>
    /// <param name="solutionName">The solution or assembly name prefix used to scope the search.</param>
    /// <param name="assemblies">
    /// Optional explicit assemblies to scan. When provided, no solution-level indexing is used.
    /// </param>
    /// <returns>The matching <see cref="Type"/>, or <see langword="null"/> if not found.</returns>
    /// <remarks>
    /// Builds a per-solution type index on first use. Subsequent calls are dictionary lookups.
    /// Failed lookups are cached to avoid repeated reflection scans.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="className"/> or <paramref name="solutionName"/> is null, empty, or whitespace.
    /// </exception>
    [Pure]
    public Type? GetTypeByNameCached(string className, string solutionName, List<Assembly>? assemblies = null)
    {
        if (className.IsNullOrWhiteSpace())
            throw new ArgumentException("Type name cannot be null or whitespace.", nameof(className));

        if (solutionName.IsNullOrWhiteSpace())
            throw new ArgumentException("Solution name cannot be null or whitespace.", nameof(solutionName));

        // Explicit assemblies => direct scan, no caching assumptions
        if (assemblies is { Count: > 0 })
            return GetTypeByNameFromAssemblies(className, assemblies);

        var missKey = (solutionName, className);

        if (_missCache.ContainsKey(missKey))
            return null;

        Dictionary<string, Type> index = _solutionTypeIndexCache.GetOrAdd(
            solutionName,
            static (sn, state) =>
            {
                var self = (TypesUtil)state!;
                Assembly[] solutionAssemblies = self.GetSolutionAssembliesCachedArray(sn);
                return BuildTypeIndex(solutionAssemblies);
            },
            this);

        if (index.TryGetValue(className, out Type? type))
            return type;

        _missCache.TryAdd(missKey, 0);
        return null;
    }

    /// <summary>
    /// Returns all assemblies associated with the specified solution name using a cached lookup.
    /// </summary>
    /// <param name="solutionName">The solution or assembly name prefix.</param>
    /// <returns>
    /// A list of assemblies whose <see cref="Assembly.FullName"/> begins with the specified prefix.
    /// </returns>
    /// <remarks>
    /// Assemblies are resolved once per solution and cached.
    /// The returned list is a copy and may be modified safely.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="solutionName"/> is null, empty, or whitespace.
    /// </exception>
    [Pure]
    public List<Assembly> GetSolutionAssembliesCached(string solutionName)
    {
        Assembly[] assemblies = GetSolutionAssembliesCachedArray(solutionName);
        return assemblies.Length == 0 ? new List<Assembly>() : new List<Assembly>(assemblies);
    }

    /// <summary>
    /// Retrieves a <see cref="Type"/> by its simple (non-namespace-qualified) name using a direct scan.
    /// </summary>
    /// <param name="className">The simple type name to locate.</param>
    /// <param name="solutionName">
    /// The solution or assembly name prefix used when <paramref name="assemblies"/> is null.
    /// </param>
    /// <param name="assemblies">
    /// Optional assemblies to search. If null, solution assemblies are scanned.
    /// </param>
    /// <returns>The first matching <see cref="Type"/>, or <see langword="null"/>.</returns>
    /// <remarks>
    /// Performs a reflection scan on each call.
    /// Does not populate or consult internal caches.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="className"/> or <paramref name="solutionName"/> is null, empty, or whitespace.
    /// </exception>
    [Pure]
    public static Type? GetTypeByName(string className, string solutionName, List<Assembly>? assemblies = null)
    {
        if (className.IsNullOrWhiteSpace())
            throw new ArgumentException("Type name cannot be null or whitespace.", nameof(className));

        if (solutionName.IsNullOrWhiteSpace())
            throw new ArgumentException("Solution name cannot be null or whitespace.", nameof(solutionName));

        assemblies ??= GetSolutionAssemblies(solutionName);
        return GetTypeByNameFromAssemblies(className, assemblies);
    }

    /// <summary>
    /// Retrieves all loaded assemblies belonging to the specified solution.
    /// </summary>
    /// <param name="solutionName">The solution or assembly name prefix.</param>
    /// <returns>
    /// A list of assemblies whose <see cref="Assembly.FullName"/> starts with the specified prefix.
    /// </returns>
    /// <remarks>
    /// Performs a snapshot scan of the current <see cref="AppDomain"/>.
    /// Results are not cached.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="solutionName"/> is null, empty, or whitespace.
    /// </exception>
    [Pure]
    public static List<Assembly> GetSolutionAssemblies(string solutionName)
    {
        if (solutionName.IsNullOrWhiteSpace())
            throw new ArgumentException("Solution name cannot be null or whitespace.", nameof(solutionName));

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var result = new List<Assembly>(assemblies.Length);

        for (var i = 0; i < assemblies.Length; i++)
        {
            Assembly asm = assemblies[i];
            string? fullName = asm.FullName;

            if (fullName != null && fullName.StartsWith(solutionName, StringComparison.Ordinal))
                result.Add(asm);
        }

        return result;
    }

    private Assembly[] GetSolutionAssembliesCachedArray(string solutionName)
    {
        if (solutionName.IsNullOrWhiteSpace())
            throw new ArgumentException("Solution name cannot be null or whitespace.", nameof(solutionName));

        return _assembliesCache.GetOrAdd(solutionName, static sn =>
        {
            List<Assembly> list = GetSolutionAssemblies(sn);
            return list.Count == 0 ? Array.Empty<Assembly>() : list.ToArray();
        });
    }

    private static Type? GetTypeByNameFromAssemblies(string className, List<Assembly> assemblies)
    {
        for (var i = 0; i < assemblies.Count; i++)
        {
            foreach (Type type in GetTypesSafely(assemblies[i]))
            {
                if (string.Equals(type.Name, className, _ordIgnore))
                    return type;
            }
        }

        return null;
    }

    private static Dictionary<string, Type> BuildTypeIndex(Assembly[] assemblies)
    {
        var index = new Dictionary<string, Type>(2048, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < assemblies.Length; i++)
        {
            foreach (Type type in GetTypesSafely(assemblies[i]))
            {
                if (!index.ContainsKey(type.Name))
                    index.Add(type.Name, type);
            }
        }

        return index;
    }

    /// <summary>
    /// Safely retrieves all loadable types from an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>All successfully loaded <see cref="Type"/> instances.</returns>
    /// <remarks>
    /// Handles <see cref="ReflectionTypeLoadException"/> by returning only non-null types.
    /// Avoids LINQ to minimize allocations.
    /// </remarks>
    private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            Type?[]? types = ex.Types;
            if (types is null || types.Length == 0)
                return Array.Empty<Type>();

            var list = new List<Type>(types.Length);
            for (var i = 0; i < types.Length; i++)
            {
                Type? type = types[i];
                if (type != null)
                    list.Add(type);
            }

            return list;
        }
    }
}
