using DdnsService.Entity;
using System.Threading.Tasks;

namespace DdnsService.Services
{
    public interface IDataAccessService
    {
        /// <summary>
        /// 数据库初始化
        /// </summary>
        /// <returns></returns>
        Task InitDatabase();

        /// <summary>
        /// 保存IP变更历史记录
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        Task SaveIpHistory(IpHistory history);
    }
}
