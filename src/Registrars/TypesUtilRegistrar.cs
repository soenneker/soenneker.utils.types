using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Utils.Types.Abstract;

namespace Soenneker.Utils.Types.Registrars;

/// <summary>
/// A utility library for Type and Assembly related operations
/// </summary>
public static class TypesUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ITypesUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddTypesUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<ITypesUtil, TypesUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="ITypesUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddTypesUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<ITypesUtil, TypesUtil>();

        return services;
    }
}
