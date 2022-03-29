using DdnsService.Configs;
using DdnsService.Database;
using DdnsService.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace DdnsService.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly ILogger<DataAccessService> _logger;
        private readonly SqliteDbContext _dbContext;
        private readonly AppSettingsNode _appSettings;

        public DataAccessService(ILogger<DataAccessService> logger
            , SqliteDbContext dbContext
            , IOptions<AppSettingsNode> appSettingsOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _appSettings = appSettingsOptions.Value ?? throw new ArgumentNullException(nameof(appSettingsOptions));
        }

        public async Task InitDatabase()
        {
            try
            {
                if (await _dbContext.Database.EnsureCreatedAsync())
                {
                    _logger.LogInformation("Sqlite database ensure created.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sqlite database ensure failed，{ex.Message}");
                throw;
            }
        }

        public async Task SaveIpHistory(IpHistory history)
        {
            if (!_appSettings.IsSaveIpHistory)
            {
                return;
            }
            try
            {
                if (history == null)
                {
                    return;
                }
                history.Guid = Guid.NewGuid();
                history.UpdateTime = DateTime.Now;
                await _dbContext.AddAsync(history);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存变更记录失败，错误：{ex.Message}");
            }
        }
    }
}
