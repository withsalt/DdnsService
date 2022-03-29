using DdnsService.Models;
using DdnsService.SDKs;
using System.Collections.Generic;

namespace DdnsService.Services
{
    public interface IDdnsProviderService
    {
        /// <summary>
        /// 根据Id获取DDNS服务商
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IDdnsService Get(int id);
    }
}
