using DdnsService.Config;
using DdnsService.Model;
using DdnsService.Utils.Date;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdnsService.Data
{
    public class IpDataManager
    {
        public async Task<LocalIpHistory> SaveIpInfo(string ip, string lastIp)
        {
            if (string.IsNullOrEmpty(ip))
            {
                throw new Exception("保存IP信息失败，当前IP不能为空。");
            }
            LocalIpHistory ipHistory = new LocalIpHistory()
            {
                IP = ip,
                LastIP = lastIp,
                UpdateTs = TimeUtil.Timestamp()
            };
            using (CustumDbContext db = new CustumDbContext())
            {
                ipHistory = db.LocalIpHistory.Add(ipHistory).Entity;
                //自动删除24h之前的数据
                if (ConfigManager.Now.AppSettings.IsEnableAutoClearHistoryIP)
                {
                    string sql = $"DELETE FROM {nameof(LocalIpHistory)} WHERE {nameof(LocalIpHistory.UpdateTs)} < {TimeUtil.Timestamp() - 24 * 60 * 60}";
                    await db.Database.ExecuteSqlRawAsync(sql);
                }
                if (await db.SaveChangesAsync() > 0)
                {
                    return ipHistory;
                }
                else
                {
                    throw new Exception($"保存IP数据失败，IP：{ip}");
                }
            }
        }

        public async Task<LocalIpHistory> GetLastIpInfo()
        {
            using (CustumDbContext db = new CustumDbContext())
            {
                LocalIpHistory ipHistory = await db.LocalIpHistory.OrderByDescending(c => c.UpdateTs).FirstOrDefaultAsync();
                return ipHistory;
            }
        }
    }
}
