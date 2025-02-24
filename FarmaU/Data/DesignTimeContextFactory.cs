// filepath: /workspaces/Proyecto-Final-Desarrollo-Web/FarmaU/Data/DesignTimeDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FarmaU.Data
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<FarmaUContext>
    {
        public FarmaUContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<FarmaUContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new FarmaUContext(builder.Options);
        }
    }
}