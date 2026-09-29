using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;

namespace Soenneker.Utils.Types.Abstract;

/// <summary>
/// A utility library for Type and Assembly related operations
/// </summary>
public interface ITypesUtil
{
    /// <summary>Replaces a solution's explicitly registered types without scanning assemblies.</summary>
    /// <param name="solutionName">The scope used for subsequent lookups.</param>
    /// <param name="types">Known types, normally supplied using typeof expressions. The first duplicate simple name wins.</param>
    /// <remarks>Registration preserves type identity, not arbitrary reflected members. Register before querying; each update replaces the complete scope atomically.</remarks>
    void RegisterTypes(string solutionName, IEnumerable<Type> types);

    /// <summary>Finds a registered type by its case-insensitive simple name without reflection scans.</summary>
    /// <param name="className">The simple type name.</param>
    /// <param name="solutionName">The registered scope.</param>
    /// <returns>The registered type, or null if the scope or name is not registered.</returns>
    Type? GetRegisteredTypeByName(string className, string solutionName);

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
    [RequiresUnreferencedCode("Assembly scanning cannot discover types removed by trimming. Use RegisterTypes and GetRegisteredTypeByName in trimmed applications.")]
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
