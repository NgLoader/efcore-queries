
# efcore-queries

`efcore-queries` is a lightweight, powerful, and flexible extension for Entity Framework Core (EF Core), designed to simplify query composition and execution. It enables the construction of dynamic queries in a more readable and maintainable way, making it easier to interact with databases. This project is specifically designed to receive HTTP requests in JSON format and parse those queries to perform simple database queries.

## Features

- **Dynamic Query Building**: Easily build complex queries from JSON payloads in HTTP requests.
- **Filter & Sort Operations**: Supports dynamic filtering and sorting based on input in the JSON request.
- **Predicate Handling**: Allows for flexible predicate expressions to filter or project data.
- **Extensive LINQ Support**: Seamless integration with LINQ to leverage all its features.
- **Async Support**: Execute queries asynchronously with ease.

## Installation

To install the package, run the following command in your project:

```bash
dotnet add package NgLoader.EFCore.Queries
```

## Usage

### Example: Role Mapping Extension

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

### Example: Using `RoleMappingExtension` in a Service

This example shows how you can integrate the `RoleMappingExtension` with a query service in a service layer:

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

### Example: RoleController Handling HTTP Requests

The following example demonstrates how the `RoleService` can be used in a controller to handle HTTP requests and return data:

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

## Contributing

We welcome contributions to enhance this project! Please fork the repository and submit your pull requests.

## License

This project is licensed under the GNU GENERAL PUBLIC LICENSE.
