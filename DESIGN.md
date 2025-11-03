# PhoneBook REST API - Design Document

## Table of Contents
- [Overview](#overview)
- [Architecture Patterns](#architecture-patterns)
  - [CQRS Pattern](#cqrs-pattern)
  - [Mediator Pattern](#mediator-pattern)
- [Project Structure](#project-structure)
- [Core Components](#core-components)
  - [CQRS Infrastructure](#cqrs-infrastructure)
  - [Commands](#commands)
  - [Queries](#queries)
  - [Handlers](#handlers)
- [Database Interaction](#database-interaction)
- [Dependency Injection](#dependency-injection)
- [Request Flow](#request-flow)
- [Benefits and Design Decisions](#benefits-and-design-decisions)
- [Code Examples](#code-examples)

## Overview

The PhoneBook REST API is built using .NET 8 and follows modern architectural patterns to ensure maintainability, testability, and scalability. The application implements **CQRS (Command Query Responsibility Segregation)** and the **Mediator pattern** to handle database operations and decouple the application layers.

### Technology Stack
- **.NET 8.0** - Modern web framework
- **ASP.NET Core Web API** - RESTful API development
- **Entity Framework Core 9.0** - Object-relational mapping (ORM)
- **SQL Server / In-Memory Database** - Data persistence
- **Custom CQRS/Mediator Implementation** - Request handling pattern

## Architecture Patterns

### CQRS Pattern

**CQRS (Command Query Responsibility Segregation)** is a pattern that separates read operations (queries) from write operations (commands). This separation provides several benefits:

#### Key Concepts

1. **Commands**: Represent intent to change the system state
   - Create, Update, Delete operations
   - May return the created/updated entity or a simple success indicator
   - Can have side effects and modify data

2. **Queries**: Represent intent to retrieve data
   - Read operations only
   - No side effects
   - Cannot modify the system state

#### Why CQRS?

- **Separation of Concerns**: Read and write logic are completely separated
- **Scalability**: Queries and commands can be optimized independently
- **Clarity**: Clear distinction between operations that change state vs. retrieve data
- **Flexibility**: Different models can be used for reading and writing

### Mediator Pattern

The **Mediator pattern** is a behavioral design pattern that reduces coupling between components by having them communicate through a mediator object instead of directly with each other.

#### How It Works

```
Controller → Mediator → Handler → Database
```

1. **Controller** sends a request (command/query) to the mediator
2. **Mediator** finds the appropriate handler for the request type
3. **Handler** processes the request and interacts with the database
4. **Response** flows back through the mediator to the controller

#### Benefits

- **Decoupling**: Controllers don't know about specific handlers
- **Testability**: Easy to mock the mediator for testing
- **Maintainability**: Changes to handlers don't affect controllers
- **Single Responsibility**: Each handler has one specific job

## Project Structure

```
PhoneBookRestApi/
├── PhoneBookRestApi/              # Main API Project
│   ├── CQRS/                      # CQRS Infrastructure
│   │   ├── IMediator.cs           # Mediator interface
│   │   ├── Mediator.cs            # Mediator implementation
│   │   ├── IRequest.cs            # Request marker interface
│   │   └── IRequestHandler.cs     # Handler interface
│   ├── Commands/                  # Write operations
│   │   ├── CreatePhoneBookEntryCommand.cs
│   │   ├── UpdatePhoneBookEntryCommand.cs
│   │   └── DeletePhoneBookEntryCommand.cs
│   ├── Queries/                   # Read operations
│   │   ├── GetAllPhoneBookEntriesQuery.cs
│   │   ├── GetPhoneBookEntryByIdQuery.cs
│   │   └── GetPhoneBookEntryByNameQuery.cs
│   ├── Handlers/                  # Command & Query handlers
│   │   ├── CreatePhoneBookEntryCommandHandler.cs
│   │   ├── UpdatePhoneBookEntryCommandHandler.cs
│   │   ├── DeletePhoneBookEntryCommandHandler.cs
│   │   ├── GetAllPhoneBookEntriesQueryHandler.cs
│   │   ├── GetPhoneBookEntryByIdQueryHandler.cs
│   │   └── GetPhoneBookEntryByNameQueryHandler.cs
│   ├── Controllers/               # API Controllers
│   │   └── PhoneBookController.cs
│   └── Program.cs                 # Application startup
├── PhoneBookRestApi.Data/         # Data Layer
│   ├── Data/
│   │   ├── PhoneBookContext.cs    # EF Core DbContext
│   │   └── PhoneBookContextFactory.cs
│   └── Models/
│       └── PhoneBookEntry.cs      # Data model
└── PhoneBookRestApi.Tests/        # Unit Tests
    └── PhoneBookControllerTests.cs
```

## Core Components

### CQRS Infrastructure

The CQRS infrastructure is implemented through four core interfaces and classes:

#### IRequest<TResponse>

A marker interface that represents a request with a specific response type:

```csharp
public interface IRequest<out TResponse>
{
}
```

- Generic interface parameterized with the response type
- Implemented by both commands and queries
- Covariant (`out`) to allow flexibility in response types

#### IRequestHandler<TRequest, TResponse>

Defines the contract for request handlers:

```csharp
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
```

- Generic interface with request and response types
- Contravariant (`in`) for the request type
- Enforces that TRequest implements IRequest<TResponse>
- Returns Task<TResponse> for async operations
- Supports cancellation tokens

#### IMediator

Defines the mediator contract:

```csharp
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, 
                                      CancellationToken cancellationToken = default);
}
```

- Single method for sending requests
- Generic response type
- Async by design
- Optional cancellation support

#### Mediator Implementation

The custom mediator implementation uses dependency injection and reflection:

```csharp
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
    {
        // 1. Determine request and response types
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        
        // 2. Build the handler type
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, responseType);
        
        // 3. Resolve handler from DI container
        var handler = _serviceProvider.GetService(handlerType);
        
        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler registered for {requestType.Name}");
        }

        // 4. Invoke the Handle method using reflection
        var handleMethod = handlerType.GetMethod("Handle");
        var result = handleMethod.Invoke(handler, 
            new object[] { request, cancellationToken });
        
        // 5. Await and return the result
        if (result is Task<TResponse> task)
        {
            return await task;
        }

        throw new InvalidOperationException(
            $"Handler for {requestType.Name} did not return a Task<{responseType.Name}>");
    }
}
```

**Key Features:**
- Uses reflection to dynamically resolve handlers
- Leverages ASP.NET Core's built-in DI container
- Provides clear error messages for missing handlers
- Fully asynchronous operation

### Commands

Commands represent operations that modify the system state (Create, Update, Delete).

#### Example: CreatePhoneBookEntryCommand

```csharp
public class CreatePhoneBookEntryCommand : IRequest<PhoneBookEntry>
{
    public PhoneBookEntry Entry { get; }

    public CreatePhoneBookEntryCommand(PhoneBookEntry entry)
    {
        Entry = entry;
    }
}
```

**Characteristics:**
- Immutable by design (readonly property)
- Clear intent through naming
- Encapsulates all data needed for the operation
- Specifies return type (PhoneBookEntry)

#### Command Types

1. **CreatePhoneBookEntryCommand**
   - Returns: `PhoneBookEntry` (the created entity)
   - Contains: `PhoneBookEntry` to create

2. **UpdatePhoneBookEntryCommand**
   - Returns: `bool` (success indicator)
   - Contains: `Id` and updated `PhoneBookEntry`

3. **DeletePhoneBookEntryCommand**
   - Returns: `bool` (success indicator)
   - Contains: `Id` of entry to delete

### Queries

Queries represent read-only operations that retrieve data without side effects.

#### Example: GetAllPhoneBookEntriesQuery

```csharp
public class GetAllPhoneBookEntriesQuery : IRequest<IEnumerable<PhoneBookEntry>>
{
}
```

**Characteristics:**
- No parameters needed (gets all entries)
- Read-only by nature
- Returns a collection

#### Example: GetPhoneBookEntryByIdQuery

```csharp
public class GetPhoneBookEntryByIdQuery : IRequest<PhoneBookEntry?>
{
    public int Id { get; }

    public GetPhoneBookEntryByIdQuery(int id)
    {
        Id = id;
    }
}
```

**Characteristics:**
- Contains filter criteria (Id)
- Returns nullable type (may not find entry)
- Immutable query parameters

#### Query Types

1. **GetAllPhoneBookEntriesQuery**
   - Returns: `IEnumerable<PhoneBookEntry>`
   - Retrieves all phone book entries

2. **GetPhoneBookEntryByIdQuery**
   - Returns: `PhoneBookEntry?` (nullable)
   - Filters by: `Id`

3. **GetPhoneBookEntryByNameQuery**
   - Returns: `PhoneBookEntry?` (nullable)
   - Filters by: `Name`

### Handlers

Handlers contain the actual business logic and database interaction code.

#### Command Handler Example

```csharp
public class CreatePhoneBookEntryCommandHandler 
    : IRequestHandler<CreatePhoneBookEntryCommand, PhoneBookEntry>
{
    private readonly PhoneBookContext _context;

    public CreatePhoneBookEntryCommandHandler(PhoneBookContext context)
    {
        _context = context;
    }

    public async Task<PhoneBookEntry> Handle(
        CreatePhoneBookEntryCommand request, 
        CancellationToken cancellationToken)
    {
        // Add entry to DbContext
        _context.PhoneBookEntries.Add(request.Entry);
        
        // Save changes to database
        await _context.SaveChangesAsync(cancellationToken);
        
        // Return the created entry (now with generated Id)
        return request.Entry;
    }
}
```

#### Query Handler Example

```csharp
public class GetAllPhoneBookEntriesQueryHandler 
    : IRequestHandler<GetAllPhoneBookEntriesQuery, IEnumerable<PhoneBookEntry>>
{
    private readonly PhoneBookContext _context;

    public GetAllPhoneBookEntriesQueryHandler(PhoneBookContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PhoneBookEntry>> Handle(
        GetAllPhoneBookEntriesQuery request, 
        CancellationToken cancellationToken)
    {
        // Query database and return results
        return await _context.PhoneBookEntries.ToListAsync(cancellationToken);
    }
}
```

**Handler Characteristics:**
- Receive DbContext via constructor injection
- Implement single responsibility (one operation per handler)
- Use async/await for database operations
- Support cancellation tokens
- Encapsulate all database access logic

## Database Interaction

### Entity Framework Core DbContext

The `PhoneBookContext` class manages the database connection and entity sets:

```csharp
public class PhoneBookContext : DbContext
{
    public PhoneBookContext(DbContextOptions<PhoneBookContext> options)
        : base(options)
    {
    }

    public DbSet<PhoneBookEntry> PhoneBookEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PhoneBookEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);  // Index for faster name lookups
        });
    }
}
```

### Data Model

```csharp
public class PhoneBookEntry
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
}
```

**Features:**
- Data annotations for validation
- Auto-incrementing Id (database-generated)
- String length limits
- Phone number format validation

### Database Configuration

The application supports both SQL Server and In-Memory databases:

```csharp
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

if (useInMemoryDatabase)
{
    builder.Services.AddDbContext<PhoneBookContext>(options =>
        options.UseInMemoryDatabase("PhoneBookDb"));
}
else
{
    builder.Services.AddDbContext<PhoneBookContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
```

**Benefits:**
- Development: Fast testing with in-memory database
- Production: Persistent storage with SQL Server
- Testing: Isolated test environments

## Dependency Injection

All components are registered in the DI container in `Program.cs`:

### CQRS Infrastructure Registration

```csharp
// Register the mediator
builder.Services.AddScoped<IMediator, Mediator>();
```

### Command Handlers Registration

```csharp
builder.Services.AddScoped<IRequestHandler<CreatePhoneBookEntryCommand, PhoneBookEntry>, 
    CreatePhoneBookEntryCommandHandler>();

builder.Services.AddScoped<IRequestHandler<UpdatePhoneBookEntryCommand, bool>, 
    UpdatePhoneBookEntryCommandHandler>();

builder.Services.AddScoped<IRequestHandler<DeletePhoneBookEntryCommand, bool>, 
    DeletePhoneBookEntryCommandHandler>();
```

### Query Handlers Registration

```csharp
builder.Services.AddScoped<IRequestHandler<GetAllPhoneBookEntriesQuery, IEnumerable<PhoneBookEntry>>, 
    GetAllPhoneBookEntriesQueryHandler>();

builder.Services.AddScoped<IRequestHandler<GetPhoneBookEntryByIdQuery, PhoneBookEntry?>, 
    GetPhoneBookEntryByIdQueryHandler>();

builder.Services.AddScoped<IRequestHandler<GetPhoneBookEntryByNameQuery, PhoneBookEntry?>, 
    GetPhoneBookEntryByNameQueryHandler>();
```

**Scoped Lifetime:**
- New instances per HTTP request
- Handlers share the same DbContext within a request
- Automatic disposal after request completion

## Request Flow

### Complete Request Flow Diagram

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ HTTP Request
       ▼
┌─────────────────────┐
│  PhoneBookController│
│   (API Layer)       │
└──────┬──────────────┘
       │ Create Command/Query
       ▼
┌─────────────────────┐
│     IMediator       │
│  (Orchestration)    │
└──────┬──────────────┘
       │ Send Request
       ▼
┌─────────────────────┐
│ Mediator.Send()     │
│ - Resolve Handler   │
│ - Invoke Handle()   │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│   Handler           │
│ - Validate          │
│ - Business Logic    │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│  PhoneBookContext   │
│  (EF Core DbContext)│
└──────┬──────────────┘
       │ SQL Query/Command
       ▼
┌─────────────────────┐
│     Database        │
│  (SQL Server / RAM) │
└─────────────────────┘
```

### Example: Creating a Phone Book Entry

#### 1. Client Request
```http
POST /api/PhoneBook
Content-Type: application/json

{
  "name": "John Doe",
  "phoneNumber": "+1-234-567-8900"
}
```

#### 2. Controller Action
```csharp
[HttpPost]
public async Task<ActionResult<PhoneBookEntry>> PostPhoneBookEntry(
    PhoneBookEntry phoneBookEntry)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Create command
    var createdEntry = await _mediator.Send(
        new CreatePhoneBookEntryCommand(phoneBookEntry));

    return CreatedAtAction(nameof(GetPhoneBookEntry), 
        new { id = createdEntry.Id }, createdEntry);
}
```

#### 3. Mediator Processing
```csharp
// Mediator.Send() internally:
// 1. Gets request type: CreatePhoneBookEntryCommand
// 2. Gets response type: PhoneBookEntry
// 3. Constructs handler type: IRequestHandler<CreatePhoneBookEntryCommand, PhoneBookEntry>
// 4. Resolves from DI: CreatePhoneBookEntryCommandHandler
// 5. Invokes: handler.Handle(command, cancellationToken)
```

#### 4. Handler Execution
```csharp
public async Task<PhoneBookEntry> Handle(
    CreatePhoneBookEntryCommand request, 
    CancellationToken cancellationToken)
{
    _context.PhoneBookEntries.Add(request.Entry);
    await _context.SaveChangesAsync(cancellationToken);
    return request.Entry;
}
```

#### 5. Database Operation
```sql
-- EF Core generates and executes:
INSERT INTO PhoneBookEntries (Name, PhoneNumber)
VALUES ('John Doe', '+1-234-567-8900');

SELECT SCOPE_IDENTITY(); -- Get generated Id
```

#### 6. Response
```json
HTTP/1.1 201 Created
Location: /api/PhoneBook/1
Content-Type: application/json

{
  "id": 1,
  "name": "John Doe",
  "phoneNumber": "+1-234-567-8900"
}
```

### Example: Retrieving All Entries

#### 1. Client Request
```http
GET /api/PhoneBook
```

#### 2. Controller Action
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<PhoneBookEntry>>> GetPhoneBookEntries()
{
    var entries = await _mediator.Send(new GetAllPhoneBookEntriesQuery());
    return Ok(entries);
}
```

#### 3. Handler Execution
```csharp
public async Task<IEnumerable<PhoneBookEntry>> Handle(
    GetAllPhoneBookEntriesQuery request, 
    CancellationToken cancellationToken)
{
    return await _context.PhoneBookEntries.ToListAsync(cancellationToken);
}
```

#### 4. Database Query
```sql
SELECT Id, Name, PhoneNumber
FROM PhoneBookEntries;
```

## Benefits and Design Decisions

### Why Custom CQRS/Mediator Implementation?

Instead of using a third-party library like MediatR, this project implements a custom solution:

#### Advantages

1. **Educational Value**
   - Clear understanding of the pattern
   - Full control over implementation
   - No hidden "magic"

2. **No External Dependencies**
   - Smaller application footprint
   - No version conflicts
   - Faster startup time

3. **Customization**
   - Can be tailored to specific needs
   - Easy to extend with custom features
   - Full debugging capability

4. **Simplicity**
   - Only 4 simple interfaces/classes
   - Easy to understand and maintain
   - Minimal learning curve

### Design Benefits

#### 1. Separation of Concerns
- Controllers focus on HTTP concerns
- Handlers focus on business logic
- Clear boundaries between layers

#### 2. Testability
```csharp
// Easy to mock the mediator in tests
var mockMediator = new Mock<IMediator>();
mockMediator.Setup(m => m.Send(It.IsAny<GetAllPhoneBookEntriesQuery>(), default))
    .ReturnsAsync(new List<PhoneBookEntry> { /* test data */ });

var controller = new PhoneBookController(mockMediator.Object);
```

#### 3. Maintainability
- Adding new features requires:
  - Create command/query class
  - Create handler class
  - Register in DI
  - No changes to existing code

#### 4. Single Responsibility
- Each handler does exactly one thing
- Easy to understand and modify
- Reduced risk of bugs

#### 5. Scalability
- Commands and queries can be optimized independently
- Easy to add caching to query handlers
- Can evolve to separate read/write databases (Event Sourcing)

### Trade-offs

#### Reflection Performance
- **Trade-off**: Slight performance overhead from reflection
- **Mitigation**: Negligible for typical web API scenarios
- **Alternative**: Could implement source generators for better performance

#### Registration Boilerplate
- **Trade-off**: Each handler must be registered manually
- **Mitigation**: Clear and explicit; prevents surprises
- **Alternative**: Could implement assembly scanning

#### Learning Curve
- **Trade-off**: Team must understand CQRS pattern
- **Mitigation**: Well-documented code and design doc
- **Benefit**: Improves architectural knowledge

## Code Examples

### Adding a New Feature

Let's add a feature to search phone book entries by partial name match.

#### Step 1: Create the Query

```csharp
// Queries/SearchPhoneBookEntriesByNameQuery.cs
using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Queries
{
    public class SearchPhoneBookEntriesByNameQuery : IRequest<IEnumerable<PhoneBookEntry>>
    {
        public string SearchTerm { get; }

        public SearchPhoneBookEntriesByNameQuery(string searchTerm)
        {
            SearchTerm = searchTerm;
        }
    }
}
```

#### Step 2: Create the Handler

```csharp
// Handlers/SearchPhoneBookEntriesByNameQueryHandler.cs
using Microsoft.EntityFrameworkCore;
using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Queries;

namespace PhoneBookRestApi.Handlers
{
    public class SearchPhoneBookEntriesByNameQueryHandler 
        : IRequestHandler<SearchPhoneBookEntriesByNameQuery, IEnumerable<PhoneBookEntry>>
    {
        private readonly PhoneBookContext _context;

        public SearchPhoneBookEntriesByNameQueryHandler(PhoneBookContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhoneBookEntry>> Handle(
            SearchPhoneBookEntriesByNameQuery request, 
            CancellationToken cancellationToken)
        {
            return await _context.PhoneBookEntries
                .Where(e => e.Name.Contains(request.SearchTerm))
                .ToListAsync(cancellationToken);
        }
    }
}
```

#### Step 3: Register the Handler

```csharp
// Program.cs
builder.Services.AddScoped<
    IRequestHandler<SearchPhoneBookEntriesByNameQuery, IEnumerable<PhoneBookEntry>>, 
    SearchPhoneBookEntriesByNameQueryHandler>();
```

#### Step 4: Add Controller Endpoint

```csharp
// Controllers/PhoneBookController.cs
[HttpGet("Search/{searchTerm}")]
public async Task<ActionResult<IEnumerable<PhoneBookEntry>>> SearchPhoneBookEntries(
    string searchTerm)
{
    var entries = await _mediator.Send(new SearchPhoneBookEntriesByNameQuery(searchTerm));
    return Ok(entries);
}
```

### Testing Example

```csharp
[Fact]
public async Task PostPhoneBookEntry_ReturnsCreatedEntry()
{
    // Arrange
    var mockMediator = new Mock<IMediator>();
    var expectedEntry = new PhoneBookEntry 
    { 
        Id = 1, 
        Name = "Test User", 
        PhoneNumber = "+1-234-567-8900" 
    };
    
    mockMediator
        .Setup(m => m.Send(It.IsAny<CreatePhoneBookEntryCommand>(), default))
        .ReturnsAsync(expectedEntry);

    var controller = new PhoneBookController(mockMediator.Object);
    var newEntry = new PhoneBookEntry 
    { 
        Name = "Test User", 
        PhoneNumber = "+1-234-567-8900" 
    };

    // Act
    var result = await controller.PostPhoneBookEntry(newEntry);

    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    var returnedEntry = Assert.IsType<PhoneBookEntry>(createdResult.Value);
    Assert.Equal(expectedEntry.Id, returnedEntry.Id);
}
```

## Conclusion

The PhoneBook REST API demonstrates a clean, maintainable architecture using CQRS and the Mediator pattern. This design:

- ✅ Separates read and write operations
- ✅ Decouples controllers from business logic
- ✅ Makes the code highly testable
- ✅ Provides clear structure for new features
- ✅ Uses dependency injection effectively
- ✅ Handles database operations through EF Core
- ✅ Supports both development and production scenarios

The custom implementation provides full transparency and control while maintaining simplicity and educational value. The pattern is extensible and can grow with the application's needs, such as adding validation pipelines, logging, caching, or even evolving towards event sourcing and eventual consistency models.
