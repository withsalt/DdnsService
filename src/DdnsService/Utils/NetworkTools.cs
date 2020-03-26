﻿using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DdnsService.Utils
{
    public class NetworkTools
    {
        private static readonly List<string> _hosts = new List<string>()
        {
            "180.76.76.76",
            "114.114.114.114",
            "223.5.5.5",
            "1.2.4.8"
        };

        /// <summary>
        /// 通过Ping验证网络是否断开
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> NetworkCheck()
        {
            try
            {
                Ping ping = new Ping();
                PingOptions options = new PingOptions
                {
                    DontFragment = true
                };
                foreach (var item in _hosts)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        PingReply reply = await ping.SendPingAsync(item, 1024);
                        if (reply.Status == IPStatus.Success)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
