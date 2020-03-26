using DdnsService.Config;
using System;
using System.Collections.Generic;
using System.Text;
using WithSalt.Common.Api.Msg;

namespace DdnsService.ApiService
{
    class MessageNotice
    {
        public (bool, string) Send(string ip, string lastIp)
        {
            try
            {
                SendMsg msg = new SendMsg(ConfigManager.Now.AppSettings.MessageApiConfig.URL, ConfigManager.Now.AppSettings.MessageApiConfig.AppKey);
                string msgContent = string.Format(ConfigManager.Now.AppSettings.MessageApiConfig.MessageTemplate, ip, lastIp);
                if (msg.Send(msgContent, ConfigManager.Now.AppSettings.MessageApiConfig.Mobile, out string err))
                {
                    return (true, "success");
                }
                else
                {
                    return (false, err);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
