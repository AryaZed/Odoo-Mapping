# Odoo Data Mapping Tool

A .NET application for mapping and synchronizing data between SQL Server databases and Odoo/PostgreSQL databases. This tool supports dynamic mapping of tables and fields, allowing for flexible data integration between systems.

## Features

- Map data from SQL Server to Odoo PostgreSQL databases
- Support for dynamic table and field mappings
- Data transformation capabilities during mapping
- Both API and Console interfaces
- Validation of mappings before execution
- Tracking of mapping execution history

## Architecture

The solution follows Clean Architecture principles with these layers:

- **Domain**: Core business models and entities
- **Application**: Business logic and interfaces
- **Infrastructure**: Implementation of data access and external services
- **API**: RESTful Web API for managing and executing mappings
- **ConsoleApp**: Command-line interface for executing mappings

## Getting Started

### Prerequisites

- .NET 8.0 or higher
- SQL Server (for application database and source data)
- PostgreSQL (for Odoo database)

### Setup

1. Clone the repository
2. Update connection strings in `appsettings.json` files
3. Run the database migrations:
   ```
   dotnet ef database update --project OdooMapping.Infrastructure --startup-project OdooMapping.Api
   ```
4. Run the API or Console application

### Using the Console App

```
dotnet run --project OdooMapping.ConsoleApp <mapping-id>
```

### Using the API

The API provides endpoints for managing mappings:

- `GET /api/mappings` - List all mappings
- `GET /api/mappings/{id}` - Get a specific mapping
- `POST /api/mappings` - Create a new mapping
- `PUT /api/mappings/{id}` - Update a mapping
- `DELETE /api/mappings/{id}` - Delete a mapping
- `POST /api/mappings/{id}/execute` - Execute a mapping
- `POST /api/mappings/{id}/validate` - Validate a mapping

## License

This project is licensed under the MIT License 