using DdnsService.Database;
using DdnsService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DdnsService
{
    public static class RegisteSqliteDatabase
    {
        public static IServiceCollection AddSqlite(this IServiceCollection services)
        {
            string connectionString = null;
            using (var provider = services.BuildServiceProvider())
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                connectionString = configuration.GetConnectionString("SqliteDbConnectionString");
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Sqlite db connect string is null.");
            }
            if (connectionString.Contains("%BASEDIRECTORY%", StringComparison.CurrentCultureIgnoreCase))
            {
                connectionString = connectionString.Replace("%BASEDIRECTORY%", AppContext.BaseDirectory, StringComparison.CurrentCultureIgnoreCase);
            }
            services.AddDbContext<SqliteDbContext>(options => options.UseSqlite(connectionString));
            services.AddScoped<IDataAccessService, DataAccessService>();
            return services;
        }
    }
}
