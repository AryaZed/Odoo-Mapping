# Odoo Data Mapping Tool

A .NET application for mapping and synchronizing data between SQL Server databases and Odoo/PostgreSQL databases. This tool supports dynamic mapping of tables and fields, allowing for flexible data integration between systems.

## Features

- Map data from SQL Server to Odoo PostgreSQL databases
- Support for dynamic table and field mappings
- Data transformation capabilities during mapping
- Multiple interfaces: Web API, Console, and Blazor UI
- Validation of mappings before execution
- Tracking of mapping execution history
- Scheduled mappings for automated synchronization

## Architecture

The solution follows Clean Architecture principles with these layers:

- **Domain**: Core business models and entities
- **Application**: Business logic and interfaces
- **Infrastructure**: Implementation of data access and external services
- **API**: RESTful Web API for managing and executing mappings
- **ConsoleApp**: Command-line interface for executing mappings
- **UI**: Blazor-based web interface for managing mappings and monitoring executions

## Getting Started

### Prerequisites

- .NET 9.0 or higher
- SQL Server (for application database and source data)
- PostgreSQL (for Odoo database)

### Setup

1. Clone the repository
2. Update connection strings in `appsettings.json` files for each project
3. Run the database migrations:
   ```
   dotnet ef database update --project OdooMapping.Infrastructure --startup-project OdooMapping.Api
   ```
4. Build the solution:
   ```
   dotnet build
   ```

### Using the Console App

```
dotnet run --project OdooMapping.ConsoleApp <mapping-guid>
```

Example:
```
dotnet run --project OdooMapping.ConsoleApp 00000000-0000-0000-0000-000000000000
```

### Using the API

The API provides endpoints for managing mappings:

- `GET /api/mappings` - List all mappings
- `GET /api/mappings/{guid}` - Get a specific mapping
- `POST /api/mappings` - Create a new mapping
- `PUT /api/mappings/{guid}` - Update a mapping
- `DELETE /api/mappings/{guid}` - Delete a mapping
- `POST /api/mappings/{guid}/execute` - Execute a mapping
- `POST /api/mappings/{guid}/validate` - Validate a mapping

### Using the UI

Run the Blazor UI project:

```
dotnet run --project OdooMapping.UI
```

The UI provides a user-friendly interface for:
- Creating and managing mapping templates
- Configuring and executing mappings
- Setting up scheduled mappings
- Validating data mappings
- Browsing Odoo models and fields

## Troubleshooting

If you encounter any issues with the UI components, ensure that the API project is running, as the UI depends on the API for backend services.

## License

This project is licensed under the MIT License 