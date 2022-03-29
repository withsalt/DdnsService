using DdnsService.Entity;
using Microsoft.EntityFrameworkCore;

namespace DdnsService.Database
{
    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
        {

        }

        public DbSet<IpHistory> IpHistory { get; set; }
    }
}
