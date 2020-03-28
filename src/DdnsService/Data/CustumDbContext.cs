using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DdnsService.Config;
using DdnsService.Model;
using Microsoft.EntityFrameworkCore;

namespace DdnsService.Data
{
    public class CustumDbContext : DbContext
    {
        private readonly string connectString = null;

        public CustumDbContext()
        {
            string conStr = ConfigManager.Now.ConnectionStrings.SqliteDbConnectionString;
            if (conStr.Contains("%BASEDIRECTORY%", StringComparison.CurrentCultureIgnoreCase))
            {
                connectString = conStr.Replace("%BASEDIRECTORY%", AppContext.BaseDirectory, StringComparison.CurrentCultureIgnoreCase);
            }
            if (string.IsNullOrEmpty(connectString))
            {
                throw new Exception("Sqlite db connect string is null.");
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectString);
        }

        public DbSet<LocalIpHistory> LocalIpHistory { get; set; }

        public DbSet<SystemLog> SystemLog { get; set; }
    }
}
