using Microsoft.EntityFrameworkCore;
using PhoneBookRestApi.Commands;
using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Handlers;
using PhoneBookRestApi.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register custom CQRS infrastructure
builder.Services.AddScoped<IMediator, Mediator>();

// Register command handlers
builder.Services.AddScoped<IRequestHandler<CreatePhoneBookEntryCommand, PhoneBookEntry>, CreatePhoneBookEntryCommandHandler>();
builder.Services.AddScoped<IRequestHandler<UpdatePhoneBookEntryCommand, bool>, UpdatePhoneBookEntryCommandHandler>();
builder.Services.AddScoped<IRequestHandler<DeletePhoneBookEntryCommand, bool>, DeletePhoneBookEntryCommandHandler>();

// Register query handlers
builder.Services.AddScoped<IRequestHandler<GetAllPhoneBookEntriesQuery, IEnumerable<PhoneBookEntry>>, GetAllPhoneBookEntriesQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetPhoneBookEntryByIdQuery, PhoneBookEntry?>, GetPhoneBookEntryByIdQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetPhoneBookEntryByNameQuery, PhoneBookEntry?>, GetPhoneBookEntryByNameQueryHandler>();

// Configure DbContext with SQL Server or InMemory based on configuration
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
