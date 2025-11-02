using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneBookRestApi.Controllers;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Tests
{
    public class PhoneBookControllerTests
    {
        private PhoneBookContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PhoneBookContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PhoneBookContext(options);
        }

        [Fact]
        public async Task GetPhoneBookEntries_ReturnsEmptyList_WhenNoEntries()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new PhoneBookController(context);

            // Act
            var result = await controller.GetPhoneBookEntries();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<PhoneBookEntry>>>(result);
            var entries = Assert.IsAssignableFrom<IEnumerable<PhoneBookEntry>>(actionResult.Value);
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

            var controller = new PhoneBookController(context);

            // Act
            var result = await controller.GetPhoneBookEntries();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<PhoneBookEntry>>>(result);
            var entries = Assert.IsAssignableFrom<IEnumerable<PhoneBookEntry>>(actionResult.Value);
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

            var controller = new PhoneBookController(context);

            // Act
            var result = await controller.GetPhoneBookEntry(entry.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PhoneBookEntry>>(result);
            var returnedEntry = Assert.IsType<PhoneBookEntry>(actionResult.Value);
            Assert.Equal("John Doe", returnedEntry.Name);
            Assert.Equal("123-456-7890", returnedEntry.PhoneNumber);
        }

        [Fact]
        public async Task GetPhoneBookEntry_ReturnsNotFound_WhenEntryDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new PhoneBookController(context);

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

            var controller = new PhoneBookController(context);

            // Act
            var result = await controller.GetPhoneBookEntryByName("John Doe");

            // Assert
            var actionResult = Assert.IsType<ActionResult<PhoneBookEntry>>(result);
            var returnedEntry = Assert.IsType<PhoneBookEntry>(actionResult.Value);
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

            var controller = new PhoneBookController(context);

            // Act
            var result = await controller.GetPhoneBookEntryByName("john doe");

            // Assert
            var actionResult = Assert.IsType<ActionResult<PhoneBookEntry>>(result);
            var returnedEntry = Assert.IsType<PhoneBookEntry>(actionResult.Value);
            Assert.Equal("John Doe", returnedEntry.Name);
        }

        [Fact]
        public async Task GetPhoneBookEntryByName_ReturnsNotFound_WhenNameDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new PhoneBookController(context);

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
            var controller = new PhoneBookController(context);
            var newEntry = new PhoneBookEntry { Name = "John Doe", PhoneNumber = "123-456-7890" };

            // Act
            var result = await controller.PostPhoneBookEntry(newEntry);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PhoneBookEntry>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdEntry = Assert.IsType<PhoneBookEntry>(createdResult.Value);
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

            var controller = new PhoneBookController(context);
            entry.PhoneNumber = "999-888-7777";

            // Act
            var result = await controller.PutPhoneBookEntry(entry.Id, entry);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedEntry = await context.PhoneBookEntries.FindAsync(entry.Id);
            Assert.NotNull(updatedEntry);
            Assert.Equal("999-888-7777", updatedEntry.PhoneNumber);
        }

        [Fact]
        public async Task PutPhoneBookEntry_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new PhoneBookController(context);
            var entry = new PhoneBookEntry { Id = 1, Name = "John Doe", PhoneNumber = "123-456-7890" };

            // Act
            var result = await controller.PutPhoneBookEntry(999, entry);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutPhoneBookEntry_ReturnsNotFound_WhenEntryDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new PhoneBookController(context);
            var entry = new PhoneBookEntry { Id = 999, Name = "John Doe", PhoneNumber = "123-456-7890" };

            // Act
            var result = await controller.PutPhoneBookEntry(999, entry);

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

            var controller = new PhoneBookController(context);

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
            var controller = new PhoneBookController(context);

            // Act
            var result = await controller.DeletePhoneBookEntry(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
