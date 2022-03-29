using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DdnsService.Services
{
    public class ConfigureService : IHostedService
    {
        private readonly ILogger<ConfigureService> _logger;
        private readonly IJobSchedulerService _scheduler;
        private readonly IDataAccessService _dataAccess;
        private readonly IDomainDdnsService _ddnsService;

        public ConfigureService(ILogger<ConfigureService> logger
            , IJobSchedulerService schedulerService
            , IDataAccessService dataAccess
            , IDomainDdnsService ddnsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scheduler = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
            _ddnsService = ddnsService ?? throw new ArgumentNullException(nameof(ddnsService));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //初始化ServiceProviders

            //验证Domains和DdnsServiceProviders的关系是否正确
            await _ddnsService.InitDomainConfigs();
            //sqlite init
            await _dataAccess.InitDatabase();
            //start work task
            await _scheduler.Start();

            _logger.LogInformation("DDNS service has started.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
