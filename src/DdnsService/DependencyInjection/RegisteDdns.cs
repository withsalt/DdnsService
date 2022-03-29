using DdnsService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DdnsService
{
    public static class RegisteDdns
    {
        public static IServiceCollection AddDdns(this IServiceCollection services)
        {
            services.TryAddTransient<IEmailNoticeService, EmailNoticeService>();
            services.TryAddSingleton<IDdnsProviderService, DdnsProviderService>();
            services.TryAddTransient<IDomainDdnsService, DomainDdnsService>();
            return services;
        }
    }
}
