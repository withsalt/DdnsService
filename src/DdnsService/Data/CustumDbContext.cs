using System;
using System.Collections.Generic;
using System.Text;
using DdnsService.Config;
using DdnsService.Model;
using Microsoft.EntityFrameworkCore;

namespace DdnsService.Data
{
    public class CustumDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConfigManager.Now.ConnectionStrings.SqliteDbConnectionString);
        }

        public DbSet<LocalIpHistory> LocalIpHistory { get; set; }

        public DbSet<SystemLog> SystemLog { get; set; }
    }
}
