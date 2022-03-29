using System.Threading.Tasks;

namespace DdnsService.Services
{
    public interface IDomainDdnsService
    {
        /// <summary>
        /// 检查域名配置是否正确
        /// </summary>
        /// <returns></returns>
        Task<bool> InitDomainConfigs();

        Task Update(string ip);
    }
}
