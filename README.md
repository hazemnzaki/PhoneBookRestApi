# PhoneBook REST API

A .NET 8 REST API for managing a phonebook with CRUD operations, built with Entity Framework Core and Swagger/OpenAPI.

![Swagger UI](https://github.com/user-attachments/assets/8f84a593-4574-4cb9-b309-26527dbc59c6)

## Features

- ✅ **CRUD Operations**: Create, Read, Update, and Delete phonebook entries
- ✅ **Entity Framework Core**: Data persistence with SQL Server support
- ✅ **In-Memory Database**: Development mode support for testing without SQL Server
- ✅ **Swagger UI**: Interactive API documentation and testing interface
- ✅ **Unit Tests**: Comprehensive test coverage using xUnit and In-Memory database
- ✅ **Data Validation**: Input validation for names and phone numbers

## API Endpoints

### PhoneBook Controller (`/api/PhoneBook`)

- `GET /api/PhoneBook` - Get all phonebook entries
- `GET /api/PhoneBook/{id}` - Get a specific entry by ID
- `GET /api/PhoneBook/ByName/{name}` - Get an entry by name
- `POST /api/PhoneBook` - Create a new entry
- `PUT /api/PhoneBook/{id}` - Update an existing entry
- `DELETE /api/PhoneBook/{id}` - Delete an entry

## Project Structure

```
PhoneBookRestApi/
├── PhoneBookRestApi/              # Main API project
│   ├── Controllers/               # API controllers
│   ├── Data/                      # DbContext and database configuration
│   ├── Models/                    # Data models
│   └── Program.cs                 # Application startup
└── PhoneBookRestApi.Tests/        # Unit test project
    └── PhoneBookControllerTests.cs # Controller tests
```

## Technologies

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 9.0
- SQL Server / In-Memory Database
- Swagger/OpenAPI
- xUnit for testing

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (or use In-Memory database for development)

### Running the Application

1. Clone the repository:
```bash
git clone https://github.com/hazemnzaki/PhoneBookRestApi.git
cd PhoneBookRestApi
```

2. Build the solution:
```bash
dotnet build
```

3. Run the API:
```bash
cd PhoneBookRestApi
dotnet run
```

4. Open Swagger UI in your browser:
```
http://localhost:5246/swagger
```

### Configuration

The application can be configured to use either SQL Server or In-Memory database:

**appsettings.Development.json** (In-Memory for development):
```json
{
  "UseInMemoryDatabase": true
}
```

**appsettings.json** (SQL Server for production):
```json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PhoneBookDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Running Tests

```bash
dotnet test
```

All tests use an in-memory database and don't require SQL Server.

## Example Usage

### Create a new entry
```bash
curl -X POST http://localhost:5246/api/PhoneBook \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "phoneNumber": "+1-234-567-8900"}'
```

### Get all entries
```bash
curl http://localhost:5246/api/PhoneBook
```

### Get entry by name
```bash
curl http://localhost:5246/api/PhoneBook/ByName/John%20Doe
```

### Update an entry
```bash
curl -X PUT http://localhost:5246/api/PhoneBook/1 \
  -H "Content-Type: application/json" \
  -d '{"id": 1, "name": "John Doe", "phoneNumber": "+1-555-123-4567"}'
```

### Delete an entry
```bash
curl -X DELETE http://localhost:5246/api/PhoneBook/1
```

## Model

### PhoneBookEntry
```csharp
{
  "id": 0,              // Auto-generated
  "name": "string",     // Required, max 100 characters
  "phoneNumber": "string" // Required, valid phone format, max 20 characters
}
```

## License

This project is open source and available under the MIT License.