using Microsoft.EntityFrameworkCore;
using FarmaU.Models;

namespace FarmaU.Data
{
    public class FarmaUContext : DbContext
    {
        public FarmaUContext(DbContextOptions<FarmaUContext> options)
            : base(options)
        {
        }

        public DbSet<MyEntity> MyEntities { get; set; }
    }
}