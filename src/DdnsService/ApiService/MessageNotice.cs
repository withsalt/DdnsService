using DdnsService.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WithSalt.Common.Api.Msg;

namespace DdnsService.ApiService
{
    class MessageNotice
    {
        public async Task<(bool, string)> Send(string ip, string lastIp)
        {
            SendMsg msg = new SendMsg(ConfigManager.Now.AppSettings.MessageApiConfig.URL, ConfigManager.Now.AppSettings.MessageApiConfig.AppKey);
            string msgContent = string.Format(ConfigManager.Now.AppSettings.MessageApiConfig.MessageTemplate, ip, lastIp);
            return await msg.Send(msgContent, ConfigManager.Now.AppSettings.MessageApiConfig.Mobile);
        }
    }
}
