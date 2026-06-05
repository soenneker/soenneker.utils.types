using System;
using System.Collections.Generic;
using System.Reflection;

namespace Soenneker.Utils.Types.Abstract;

/// <summary>
/// A utility library for Type and Assembly related operations
/// </summary>
public interface ITypesUtil
{
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
    Type? GetTypeByNameCached(string className, string solutionName, List<Assembly>? assemblies = null);

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
    List<Assembly> GetSolutionAssembliesCached(string solutionName);
}
