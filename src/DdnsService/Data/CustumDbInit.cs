using DdnsService.Model;
using DdnsService.Utils.Date;
using Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.Data
{
    public class CustumDbInit
    {
        public static bool Initialize()
        {
            try
            {
                using (CustumDbContext context = new CustumDbContext())
                {
                    if (context.Database.EnsureCreated())
                    {
                        context.SystemLog.Add(new SystemLog()
                        {
                            Log = "数据库初始化。",
                            Ts = TimeUtil.Timestamp(),
                            LogType = Enum.LogType.Info
                        });
                    }
                    else
                    {
                        context.SystemLog.Add(new SystemLog()
                        {
                            Log = "服务启动。",
                            Ts = TimeUtil.Timestamp(),
                            LogType = Enum.LogType.Info
                        });
                    }
                    context.SaveChanges();
                    return true;
                }
            }
            catch(Exception ex)
            {
                Log.Error($"初始化数据库出错，{ex.Message}", ex);
                return false;
            }
        }
    }
}
