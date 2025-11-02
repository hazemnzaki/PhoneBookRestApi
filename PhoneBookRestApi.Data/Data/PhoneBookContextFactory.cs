using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PhoneBookRestApi.Data
{
    public class PhoneBookContextFactory : IDesignTimeDbContextFactory<PhoneBookContext>
    {
        public PhoneBookContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PhoneBookContext>();

            // Build configuration - look for appsettings.json in the startup project
            var basePath = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(basePath, "../PhoneBookRestApi");
            
            // If we're already in the startup project directory, use current directory
            if (!Directory.Exists(configPath))
            {
                configPath = basePath;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(configPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get the connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in appsettings.json. " +
                    "Please ensure the connection string is configured in your appsettings.json file.");
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new PhoneBookContext(optionsBuilder.Options);
        }
    }
}
