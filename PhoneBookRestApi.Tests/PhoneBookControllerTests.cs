using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhoneBookRestApi.Commands;
using PhoneBookRestApi.Controllers;
using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Dtos;
using PhoneBookRestApi.Handlers;
using PhoneBookRestApi.Mappings;
using PhoneBookRestApi.Queries;

namespace PhoneBookRestApi.Tests
{
    public class PhoneBookControllerTests
    {
        private static readonly PhoneBookContextFactory _contextFactory = new PhoneBookContextFactory();

        private PhoneBookContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PhoneBookContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return _contextFactory.CreateDbContext(options);
        }

        private IMediator GetMediator(PhoneBookContext context)
        {
            var services = new ServiceCollection();
            services.AddSingleton(context);
            
            // Register AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));
            
            // Register custom CQRS infrastructure
            services.AddScoped<IMediator, Mediator>();
            
            // Register command handlers
            services.AddScoped<IRequestHandler<CreatePhoneBookEntryCommand, PhoneBookEntry>, CreatePhoneBookEntryCommandHandler>();
            services.AddScoped<IRequestHandler<UpdatePhoneBookEntryCommand, bool>, UpdatePhoneBookEntryCommandHandler>();
            services.AddScoped<IRequestHandler<DeletePhoneBookEntryCommand, bool>, DeletePhoneBookEntryCommandHandler>();
            
            // Register query handlers
            services.AddScoped<IRequestHandler<GetAllPhoneBookEntriesQuery, IEnumerable<PhoneBookEntry>>, GetAllPhoneBookEntriesQueryHandler>();
            services.AddScoped<IRequestHandler<GetPhoneBookEntryByIdQuery, PhoneBookEntry?>, GetPhoneBookEntryByIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetPhoneBookEntryByNameQuery, PhoneBookEntry?>, GetPhoneBookEntryByNameQueryHandler>();
            
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IMediator>();
        }

        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            return config.CreateMapper();
        }

        [Fact]
        public async Task GetPhoneBookEntries_ReturnsEmptyList_WhenNoEntries()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.GetPhoneBookEntries();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<PhoneBookEntryDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var entries = Assert.IsAssignableFrom<IEnumerable<PhoneBookEntryDto>>(okResult.Value);
            Assert.Empty(entries);
        }

        [Fact]
        public async Task GetPhoneBookEntries_ReturnsAllEntries()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.PhoneBookEntries.AddRange(
                new PhoneBookEntry { Name = "John Doe", PhoneNumber = "123-456-7890" },
                new PhoneBookEntry { Name = "Jane Smith", PhoneNumber = "987-654-3210" }
            );
            await context.SaveChangesAsync();

            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.GetPhoneBookEntries();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<PhoneBookEntryDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var entries = Assert.IsAssignableFrom<IEnumerable<PhoneBookEntryDto>>(okResult.Value);
            Assert.Equal(2, entries.Count());
        }

        [Fact]
        public async Task GetPhoneBookEntry_ReturnsEntry_WhenEntryExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var entry = new PhoneBookEntry { Name = "John Doe", PhoneNumber = "123-456-7890" };
            context.PhoneBookEntries.Add(entry);
            await context.SaveChangesAsync();

            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.GetPhoneBookEntry(entry.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PhoneBookEntryDto>>(result);
            var returnedEntry = Assert.IsType<PhoneBookEntryDto>(actionResult.Value);
            Assert.Equal("John Doe", returnedEntry.Name);
            Assert.Equal("123-456-7890", returnedEntry.PhoneNumber);
        }

        [Fact]
        public async Task GetPhoneBookEntry_ReturnsNotFound_WhenEntryDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.GetPhoneBookEntry(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPhoneBookEntryByName_ReturnsEntry_WhenNameExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var entry = new PhoneBookEntry { Name = "John Doe", PhoneNumber = "123-456-7890" };
            context.PhoneBookEntries.Add(entry);
            await context.SaveChangesAsync();

            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.GetPhoneBookEntryByName("John Doe");

            // Assert
            var actionResult = Assert.IsType<ActionResult<PhoneBookEntryDto>>(result);
            var returnedEntry = Assert.IsType<PhoneBookEntryDto>(actionResult.Value);
            Assert.Equal("John Doe", returnedEntry.Name);
            Assert.Equal("123-456-7890", returnedEntry.PhoneNumber);
        }

        [Fact]
        public async Task GetPhoneBookEntryByName_IsCaseInsensitive()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var entry = new PhoneBookEntry { Name = "John Doe", PhoneNumber = "123-456-7890" };
            context.PhoneBookEntries.Add(entry);
            await context.SaveChangesAsync();

            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.GetPhoneBookEntryByName("john doe");

            // Assert
            var actionResult = Assert.IsType<ActionResult<PhoneBookEntryDto>>(result);
            var returnedEntry = Assert.IsType<PhoneBookEntryDto>(actionResult.Value);
            Assert.Equal("John Doe", returnedEntry.Name);
        }

        [Fact]
        public async Task GetPhoneBookEntryByName_ReturnsNotFound_WhenNameDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.GetPhoneBookEntryByName("NonExistent");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostPhoneBookEntry_CreatesNewEntry()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);
            var newEntry = new CreatePhoneBookEntryDto { Name = "John Doe", PhoneNumber = "123-456-7890" };

            // Act
            var result = await controller.PostPhoneBookEntry(newEntry);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PhoneBookEntryDto>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdEntry = Assert.IsType<PhoneBookEntryDto>(createdResult.Value);
            Assert.Equal("John Doe", createdEntry.Name);
            Assert.Equal("123-456-7890", createdEntry.PhoneNumber);
            Assert.True(createdEntry.Id > 0);
        }

        [Fact]
        public async Task PutPhoneBookEntry_UpdatesEntry_WhenEntryExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var entry = new PhoneBookEntry { Name = "John Doe", PhoneNumber = "123-456-7890" };
            context.PhoneBookEntries.Add(entry);
            await context.SaveChangesAsync();

            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);
            var updateDto = new UpdatePhoneBookEntryDto { Name = "John Doe", PhoneNumber = "999-888-7777" };

            // Act
            var result = await controller.PutPhoneBookEntry(entry.Id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedEntry = await context.PhoneBookEntries.FindAsync(entry.Id);
            Assert.NotNull(updatedEntry);
            Assert.Equal("999-888-7777", updatedEntry.PhoneNumber);
        }

        [Fact]
        public async Task PutPhoneBookEntry_ReturnsNotFound_WhenEntryDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);
            var updateDto = new UpdatePhoneBookEntryDto { Name = "John Doe", PhoneNumber = "123-456-7890" };

            // Act
            var result = await controller.PutPhoneBookEntry(999, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeletePhoneBookEntry_DeletesEntry_WhenEntryExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var entry = new PhoneBookEntry { Name = "John Doe", PhoneNumber = "123-456-7890" };
            context.PhoneBookEntries.Add(entry);
            await context.SaveChangesAsync();

            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.DeletePhoneBookEntry(entry.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var deletedEntry = await context.PhoneBookEntries.FindAsync(entry.Id);
            Assert.Null(deletedEntry);
        }

        [Fact]
        public async Task DeletePhoneBookEntry_ReturnsNotFound_WhenEntryDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mediator = GetMediator(context);
            var mapper = GetMapper();
            var controller = new PhoneBookController(mediator, mapper);

            // Act
            var result = await controller.DeletePhoneBookEntry(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
