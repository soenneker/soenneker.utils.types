[![](https://img.shields.io/nuget/v/soenneker.utils.types.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.types/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.types/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.types/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.types.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.types/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.types/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.types/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.Types
Simple-name type lookup across loaded, solution-prefixed assemblies, with cached and snapshot-based APIs.

## Installation

```bash
dotnet add package Soenneker.Utils.Types
```

## Cached lookup

```csharp
using Soenneker.Utils.Types.Registrars;

services.AddTypesUtilAsSingleton();
```

Singleton registration shares assembly snapshots, type indexes, and negative lookups across the application. Scoped registration is also available when each scope should build independent caches.

```csharp
using Soenneker.Utils.Types.Abstract;

Type? handlerType = typesUtil.GetTypeByNameCached(
    className: "CreateOrderHandler",
    solutionName: "Acme.Orders");
```

The solution name is an ordinal, case-sensitive prefix applied to `Assembly.FullName`. Only assemblies already loaded in the current `AppDomain` are considered; this library does not load assemblies from disk.

Type names are simple names such as `CreateOrderHandler`, not namespace-qualified or assembly-qualified names. Matching is ordinal and case-insensitive. If multiple types share the same simple name, the first type discovered wins, so use an explicit assembly list when ambiguity matters.

## Explicit assemblies

```csharp
using System.Reflection;

var assemblies = new List<Assembly>
{
    typeof(CreateOrderHandler).Assembly
};

Type? handlerType = typesUtil.GetTypeByNameCached(
    "CreateOrderHandler",
    "Acme.Orders",
    assemblies);
```

A non-empty explicit assembly list is scanned directly and bypasses the internal solution caches. The `solutionName` argument is still required by the API but does not scope that explicit scan.

## Cached assembly results

```csharp
List<Assembly> assemblies = typesUtil.GetSolutionAssembliesCached("Acme.Orders");
```

The returned list is a copy and can be modified safely. Its contents come from the first snapshot cached for that exact solution prefix. Assemblies loaded afterward are not added automatically. Likewise, failed cached type lookups are remembered. Use a new scoped utility or the static non-cached APIs when late-loaded assemblies must be visible.

## Non-cached APIs

```csharp
Type? type = TypesUtil.GetTypeByName("CreateOrderHandler", "Acme.Orders");
List<Assembly> current = TypesUtil.GetSolutionAssemblies("Acme.Orders");
```

These methods scan the current loaded-assembly snapshot on every call and do not populate the instance caches. `ReflectionTypeLoadException` is handled by searching the types that did load; other reflection failures propagate.

Blank type or solution names throw `ArgumentException`. Lookup returns `null` when no type matches.
