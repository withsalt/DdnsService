using DdnsService.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace DdnsService
{
    public static class RegisteConfigure
    {
        public static IServiceCollection ConfigureSettings(this IServiceCollection services, HostBuilderContext hostContext)
        {
            services.Configure<AppSettingsNode>(hostContext.Configuration.GetSection(AppSettingsNode.Position));
            services.Configure<DdnsConfigNode>(hostContext.Configuration.GetSection(DdnsConfigNode.Position));
            services.Configure<List<ApisNode>>(hostContext.Configuration.GetSection(ApisNode.Position));
            return services;
        }
    }
}
