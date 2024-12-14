# efcore-queries

`efcore-queries` is a lightweight, powerful, and flexible extension for Entity Framework Core (EF Core), designed to simplify query composition and execution. It enables the construction of dynamic queries in a more readable and maintainable way, making it easier to interact with databases. This project is specifically designed to receive HTTP requests in JSON format and parse those queries to perform simple database queries.

---

## Table of Contents

1. [Features](#features)
2. [Installation](#installation)
3. [Configuration](#configuration)
4. [Initialization](#initialization)
5. [Usage](#usage)
    - [Role Mapping Extension](#role-mapping-extension)
    - [Using RoleMappingExtension in a Service](#using-rolemappingextension-in-a-service)
    - [Controller Integration](#controller-integration)
6. [Request Structure](#request-structure)
7. [Response Structure](#response-structure)
8. [Example Request](#example-request)
9. [Contributing](#contributing)
10. [License](#license)

---

## Features

- **Dynamic Query Building**: Easily build complex queries from JSON payloads in HTTP requests.
- **Filter & Sort Operations**: Supports dynamic filtering and sorting based on input in the JSON request.
- **Predicate Handling**: Allows for flexible predicate expressions to filter or project data.
- **Extensive LINQ Support**: Seamless integration with LINQ to leverage all its features.
- **Async Support**: Execute queries asynchronously with ease.

---

## Installation

To install the package, run the following command in your project:

```bash
dotnet add package efcore-queries
```

---

## Configuration

To configure the behavior of the query parser, add the following section to your `appsettings.json`:

```json
"Query": {
  "Limits": {
    "FilterCompositionDepth": 5,
    "FilterConditionDepth": 4,
    "SortLimit": 4,
    "PageSizeLimit": 100
  },
  "DefaultPageSize": 10
}
```

### Key Configuration Options

- `FilterCompositionDepth`: Maximum depth for nested filters.
- `FilterConditionDepth`: Maximum depth for filter conditions.
- `SortLimit`: Maximum number of sorting operations allowed.
- `PageSizeLimit`: Maximum page size for queries.
- `DefaultPageSize`: Default page size if not specified in the request.

---

## Initialization

To initialize `efcore-queries`, update your `Program.cs` as follows:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Initialize efcore-queries
        builder.AddEFCoreQueries();
    }
}
```

This setup ensures the library is ready to parse and handle incoming JSON queries.

---

## Usage

### Role Mapping Extension

This example demonstrates how to use `QuerySpecification` to map a `Role` entity to a `RoleDto`:

```csharp
public class RoleMappingExtension
{
    public static readonly QuerySpecification<Role, RoleDto> builder = new QuerySpecificationBuilder<Role, RoleDto>()
        .Register(e => e.RoleId, d => d.Id)
        .Register(e => e.Name, d => d.Name)
        .Register(e => e.Permissions, RolePermissionMappingExtension.Expression, d => d.Permissions)
        .Build();

    public static Expression<Func<Role, RoleDto>> Expression => builder.ConstructorExperssion;
}
```

### Using RoleMappingExtension in a Service

This example shows how to integrate the `RoleMappingExtension` with a query service in a service layer:

```csharp
public class RoleService
{
    private readonly QueryBuilder<Role, RoleDto> queryBuilder;

    public RoleService(QueryService queryService)
    {
        queryBuilder = queryService.For(RoleMappingExtension.builder);
    }

    public async Task<QueryResponse<RoleDto>> QueryRoleList(QueryState query, CancellationToken cancel)
    {
        return await db.Roles.Query(queryBuilder, query, cancel);
    }
}
```

### Controller Integration

The following example demonstrates how to use the `RoleService` in a controller to handle HTTP requests:

```csharp
public class RoleController
{
    [HttpGet("search")]
    public Task<QueryResponse<RoleDto>> GetRolesSearch([FromBody] QueryState state, CancellationToken cancel)
    {
        return service.QueryRoleList(state, cancel);
    }
}
```

---

### Request Structure

If the `TotalCount` is included in the request, the response will remain unchanged.
However, if you wish to get the updated `TotalCount`, simply omit the field in the request, and the response will include a recalculated count.
This approach is particularly useful for multiple search requests, as it eliminates the need to recalculate the `TotalCount` with each request.

```json
{
  "filter": [ "<filter>" ],
  "sorts": [
    {
      "key": "<string>",
      "direction": "<direction>"
    }
  ],
  "pagination": {
    "pageIndex": "<integer>",
    "pageSize": "<integer>",
    "totalCount": "<integer>" // !!! read the above comment !!!
  }
}
```

#### Filter

##### Filter Condition

```json
{
    "filterType": "condition",
	"key": "<string>",
	"value": "<string>|<date>|<float>|<boolean>|<null>",
	"operator": "<operator>", // see below (Condition Operators)
	"options": "<option>" // see below (Condition Operators Options)
}
```
##### Filter Composition

```json
{
    "filterType": "composition",
	"operator": "<operator>", // see below (Composition Operators)
	"conditions: [ "<filter>" ] // see above (Filter)
}
```

#### Condition Operators

- Equals
- NotEquals
- GreaterThan
- GreaterThanOrEqual
- LessThan
- LessThanOrEqual

- Contains
- NotContains
- StartsWith
- EndsWith

- Includes
- NotIncludes

#### Condition Operators Options (!!! As Bit Flag !!!)

- None
- IgnoreCase

#### Composition Operators

- And
- Or

#### Direction

- Descending
- Ascending

### Response Structure

The `TotalCount` will only be calculated if it has not already been provided in the request.
If the request includes a `TotalCount`, the response will return the same value without recalculating it.

```json
{
  "results": [
    "<your json parsed DTO>"
  ]
  "state": {
    {
      "filter": "<filter>",
      "sorts": [
        {
          "key": "<string>",
          "direction": "<direction>"
        }
      ],
      "pagination": {
        "pageIndex": "<integer>",
        "pageSize": "<integer>",
        "totalCount": "<integer>"
      }
    }
  }
}
```

### Example Request

```json
{
  "filter": {
    "filterType": "composition",
    "operator": "and",
    "": [
      {
        "filterType": "condition",
        "key": "Name",
        "value": "ngloader",
        "operator": "contains",
        "options": "ignorecase"
      },
      {
        "filterType": "condition",
        "key": "Age",
        "value": "24",
        "operator": "equals"
      }
    ]
  },
  "sorts": [
    {
      "key": "Name",
      "direction": "descending"
    }
  ],
  "pagination": {
    "pageIndex": 0,
    "pageSize": 5
  }
}
```

---

## Contributing

We welcome contributions to enhance this project! If you have ideas or fixes, feel free to:

1. Fork the repository.
2. Create a new branch.
3. Submit a pull request with detailed explanations of your changes.

---

## License

This project is licensed under the **GNU General Public License**.

---

