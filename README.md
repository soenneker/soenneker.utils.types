[![](https://img.shields.io/nuget/v/soenneker.utils.types.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.types/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.types/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.types/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.types.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.types/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.types/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.types/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.Types
A utility library for Type and Assembly related operations.

## Installation

```bash
dotnet add package Soenneker.Utils.Types
```

## Quick start

```csharp
using Soenneker.Utils.Types.Registrars;

services.AddTypesUtilAsSingleton();
```

Then inject `ITypesUtil` wherever you need it.

## Common operations

- `GetTypeByNameCached()` - Retrieves a `Type` by its simple (non-namespace-qualified) name using cached indexes. Returns the matching `Type`, or `null` if not found.
- `GetSolutionAssembliesCached()` - Returns all assemblies associated with the specified solution name using a cached lookup.
- `GetTypeByName()` - Retrieves a `Type` by its simple (non-namespace-qualified) name using a direct scan. Returns the first matching `Type`, or `null`.
- `GetSolutionAssemblies()` - Retrieves all loaded assemblies belonging to the specified solution. Returns a list of assemblies whose `Assembly.FullName` starts with the specified prefix.
