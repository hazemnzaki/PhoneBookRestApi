# PhoneBook REST API - Design Document

## Table of Contents
- [Overview](#overview)
- [Architecture Patterns](#architecture-patterns)
  - [CQRS Pattern](#cqrs-pattern)
  - [Mediator Pattern](#mediator-pattern)
- [Project Structure](#project-structure)
- [Class and Interface Relationships](#class-and-interface-relationships)
- [Core Components](#core-components)
  - [CQRS Infrastructure](#cqrs-infrastructure)
  - [Commands](#commands)
  - [Queries](#queries)
  - [Handlers](#handlers)
- [Database Interaction](#database-interaction)
- [Dependency Injection](#dependency-injection)
- [Request Flow](#request-flow)
- [Benefits and Design Decisions](#benefits-and-design-decisions)
- [Conclusion](#conclusion)

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

#### Request Flow Diagram

```
┌─────────────────┐
│   Controller    │  (Receives HTTP requests)
└────────┬────────┘
         │ Creates Command/Query
         ▼
┌─────────────────┐
│    IMediator    │  (Orchestration layer)
└────────┬────────┘
         │ Send(request)
         ▼
┌─────────────────┐
│ Mediator.Send() │  (Resolves handler via DI)
└────────┬────────┘
         │ Invokes Handle()
         ▼
┌─────────────────┐
│     Handler     │  (Business logic)
└────────┬────────┘
         │ Database operations
         ▼
┌─────────────────┐
│   DbContext     │  (EF Core)
└────────┬────────┘
         │ SQL Commands
         ▼
┌─────────────────┐
│    Database     │  (SQL Server / In-Memory)
└─────────────────┘
```

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

## Class and Interface Relationships

### CQRS Infrastructure Classes

```
┌──────────────────────────────────────────────────────────────┐
│                    CQRS Infrastructure                        │
└──────────────────────────────────────────────────────────────┘

┌─────────────────────────┐
│  <<interface>>          │
│  IRequest<TResponse>    │  (Marker interface)
└─────────────────────────┘
           △
           │ implements
           │
    ┌──────┴───────┬─────────────┐
    │              │             │
┌───────────┐  ┌──────────┐  ┌──────────┐
│  Command  │  │  Query   │  │  Query   │
│  Classes  │  │  Classes │  │  Classes │
└───────────┘  └──────────┘  └──────────┘


┌───────────────────────────────────────┐
│  <<interface>>                        │
│  IRequestHandler<TRequest, TResponse> │
│  + Handle(request, token): Task<T>   │
└───────────────────────────────────────┘
           △
           │ implements
           │
    ┌──────┴───────┬─────────────┐
    │              │             │
┌─────────────┐ ┌──────────────┐ ┌──────────────┐
│  Command    │ │  Query       │ │  Query       │
│  Handlers   │ │  Handlers    │ │  Handlers    │
└─────────────┘ └──────────────┘ └──────────────┘


┌────────────────────────────────────┐
│  <<interface>>                     │
│  IMediator                         │
│  + Send<TResponse>(request): Task │
└────────────────────────────────────┘
           △
           │ implements
           │
┌────────────────────────────────────┐
│  Mediator                          │
│  - _serviceProvider: IServiceProvider │
│  + Send<TResponse>(request): Task  │
│  (Uses reflection to resolve)      │
└────────────────────────────────────┘
```

### Command Classes and Handlers

```
┌──────────────────────────────────────────────────────────────┐
│                    Commands (Write Operations)                │
└──────────────────────────────────────────────────────────────┘

┌─────────────────────────────────┐
│ CreatePhoneBookEntryCommand     │
│ : IRequest<PhoneBookEntry>      │
│ + Entry: PhoneBookEntry         │
└─────────────────────────────────┘
         │
         │ handled by
         ▼
┌─────────────────────────────────────────────────┐
│ CreatePhoneBookEntryCommandHandler              │
│ : IRequestHandler<CreateCommand, PhoneBookEntry>│
│ - _context: PhoneBookContext                    │
│ + Handle(): Task<PhoneBookEntry>                │
└─────────────────────────────────────────────────┘


┌─────────────────────────────────┐
│ UpdatePhoneBookEntryCommand     │
│ : IRequest<bool>                │
│ + Id: int                       │
│ + Entry: PhoneBookEntry         │
└─────────────────────────────────┘
         │
         │ handled by
         ▼
┌─────────────────────────────────────────────────┐
│ UpdatePhoneBookEntryCommandHandler              │
│ : IRequestHandler<UpdateCommand, bool>          │
│ - _context: PhoneBookContext                    │
│ + Handle(): Task<bool>                          │
└─────────────────────────────────────────────────┘


┌─────────────────────────────────┐
│ DeletePhoneBookEntryCommand     │
│ : IRequest<bool>                │
│ + Id: int                       │
└─────────────────────────────────┘
         │
         │ handled by
         ▼
┌─────────────────────────────────────────────────┐
│ DeletePhoneBookEntryCommandHandler              │
│ : IRequestHandler<DeleteCommand, bool>          │
│ - _context: PhoneBookContext                    │
│ + Handle(): Task<bool>                          │
└─────────────────────────────────────────────────┘
```

### Query Classes and Handlers

```
┌──────────────────────────────────────────────────────────────┐
│                    Queries (Read Operations)                  │
└──────────────────────────────────────────────────────────────┘

┌────────────────────────────────────┐
│ GetAllPhoneBookEntriesQuery        │
│ : IRequest<IEnumerable<Entry>>     │
│ (No properties - gets all)         │
└────────────────────────────────────┘
         │
         │ handled by
         ▼
┌────────────────────────────────────────────────────────┐
│ GetAllPhoneBookEntriesQueryHandler                     │
│ : IRequestHandler<Query, IEnumerable<Entry>>           │
│ - _context: PhoneBookContext                           │
│ + Handle(): Task<IEnumerable<PhoneBookEntry>>          │
└────────────────────────────────────────────────────────┘


┌────────────────────────────────────┐
│ GetPhoneBookEntryByIdQuery         │
│ : IRequest<PhoneBookEntry?>        │
│ + Id: int                          │
└────────────────────────────────────┘
         │
         │ handled by
         ▼
┌────────────────────────────────────────────────────────┐
│ GetPhoneBookEntryByIdQueryHandler                      │
│ : IRequestHandler<Query, PhoneBookEntry?>              │
│ - _context: PhoneBookContext                           │
│ + Handle(): Task<PhoneBookEntry?>                      │
└────────────────────────────────────────────────────────┘


┌────────────────────────────────────┐
│ GetPhoneBookEntryByNameQuery       │
│ : IRequest<PhoneBookEntry?>        │
│ + Name: string                     │
└────────────────────────────────────┘
         │
         │ handled by
         ▼
┌────────────────────────────────────────────────────────┐
│ GetPhoneBookEntryByNameQueryHandler                    │
│ : IRequestHandler<Query, PhoneBookEntry?>              │
│ - _context: PhoneBookContext                           │
│ + Handle(): Task<PhoneBookEntry?>                      │
└────────────────────────────────────────────────────────┘
```

### Controller Integration

```
┌────────────────────────────────────────────────────────────┐
│                   PhoneBookController                       │
│                   (API Controller)                          │
│                                                             │
│  - _mediator: IMediator                                    │
│                                                             │
│  + GetPhoneBookEntries(): Task<ActionResult>               │
│  + GetPhoneBookEntry(id): Task<ActionResult>               │
│  + GetPhoneBookEntryByName(name): Task<ActionResult>       │
│  + PostPhoneBookEntry(entry): Task<ActionResult>           │
│  + PutPhoneBookEntry(id, entry): Task<ActionResult>        │
│  + DeletePhoneBookEntry(id): Task<ActionResult>            │
└────────────┬───────────────────────────────────────────────┘
             │ uses
             │
             ▼
┌────────────────────────────────────┐
│         IMediator                  │
│  (Injected via DI)                 │
│  + Send<TResponse>(request): Task  │
└────────────────────────────────────┘
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

The custom mediator implementation uses dependency injection and reflection to dynamically resolve handlers at runtime.

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
        _context.PhoneBookEntries.Add(request.Entry);
        await _context.SaveChangesAsync(cancellationToken);
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

### Database Layer Diagram

```
┌─────────────────────────────────────────────────────┐
│              PhoneBookContext                        │
│              : DbContext                             │
│                                                      │
│  + PhoneBookEntries: DbSet<PhoneBookEntry>          │
│  + OnModelCreating(modelBuilder): void              │
└──────────────────┬──────────────────────────────────┘
                   │ manages
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│              PhoneBookEntry                          │
│              (Entity Model)                          │
│                                                      │
│  + Id: int                                          │
│  + Name: string [Required, MaxLength(100)]          │
│  + PhoneNumber: string [Required, Phone, MaxLength] │
└─────────────────────────────────────────────────────┘
                   │
                   │ persisted to
                   ▼
┌─────────────────────────────────────────────────────┐
│              Database                                │
│              (SQL Server / In-Memory)                │
│                                                      │
│  Table: PhoneBookEntries                            │
│  - Id (PK, Identity)                                │
│  - Name (nvarchar(100), Index)                      │
│  - PhoneNumber (nvarchar(20))                       │
└─────────────────────────────────────────────────────┘
```

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

### Dependency Injection Diagram

```
┌─────────────────────────────────────────────────────────────┐
│              Dependency Injection Container                  │
│              (Program.cs Configuration)                      │
└─────────────────────────────────────────────────────────────┘

    IMediator  ──────────────────▶  Mediator
                                    (Scoped)

    IRequestHandler<CreateCommand, Entry>  ──▶  CreateHandler
    IRequestHandler<UpdateCommand, bool>   ──▶  UpdateHandler
    IRequestHandler<DeleteCommand, bool>   ──▶  DeleteHandler
                                                 (All Scoped)

    IRequestHandler<GetAllQuery, IEnum<E>>  ──▶  GetAllHandler
    IRequestHandler<GetByIdQuery, Entry?>   ──▶  GetByIdHandler
    IRequestHandler<GetByNameQuery, Entry?> ──▶  GetByNameHandler
                                                 (All Scoped)

    PhoneBookContext  ──────────────────▶  DbContext Instance
                                           (Scoped - one per request)
```

**Scoped Lifetime:**
- New instances per HTTP request
- Handlers share the same DbContext within a request
- Automatic disposal after request completion

## Request Flow

### Complete Request Flow Example

#### Creating a Phone Book Entry

```
 1. HTTP Request
    POST /api/PhoneBook
    { "name": "John Doe", "phoneNumber": "+1-234-567-8900" }
         │
         ▼
 2. PhoneBookController
    - Receives HTTP request
    - Validates model
    - Creates: new CreatePhoneBookEntryCommand(entry)
         │
         ▼
 3. IMediator.Send()
    - Controller calls: await _mediator.Send(command)
         │
         ▼
 4. Mediator Implementation
    - Gets request type: CreatePhoneBookEntryCommand
    - Gets response type: PhoneBookEntry
    - Constructs handler type using reflection
    - Resolves from DI: CreatePhoneBookEntryCommandHandler
         │
         ▼
 5. CreatePhoneBookEntryCommandHandler
    - Receives command
    - Calls: _context.PhoneBookEntries.Add(entry)
    - Calls: await _context.SaveChangesAsync()
         │
         ▼
 6. PhoneBookContext (EF Core)
    - Tracks entity changes
    - Generates SQL: INSERT INTO PhoneBookEntries...
    - Executes against database
         │
         ▼
 7. Database
    - Inserts record
    - Returns generated Id
         │
         ▼
 8. Response Flow (back up the chain)
    Handler returns PhoneBookEntry
    → Mediator returns to Controller
    → Controller returns HTTP 201 Created
```

#### Example: Retrieving All Entries

```
 1. HTTP Request
    GET /api/PhoneBook
         │
         ▼
 2. PhoneBookController
    - Receives HTTP request
    - Creates: new GetAllPhoneBookEntriesQuery()
         │
         ▼
 3. IMediator.Send()
    - Controller calls: await _mediator.Send(query)
         │
         ▼
 4. Mediator Implementation
    - Resolves: GetAllPhoneBookEntriesQueryHandler
         │
         ▼
 5. GetAllPhoneBookEntriesQueryHandler
    - Calls: _context.PhoneBookEntries.ToListAsync()
         │
         ▼
 6. Database Query
    - Executes: SELECT * FROM PhoneBookEntries
         │
         ▼
 7. Response Flow
    Handler returns IEnumerable<PhoneBookEntry>
    → Mediator returns to Controller
    → Controller returns HTTP 200 OK with data
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

---

*This document serves as both architectural documentation and an educational resource for understanding CQRS and Mediator patterns in a .NET application context.*
