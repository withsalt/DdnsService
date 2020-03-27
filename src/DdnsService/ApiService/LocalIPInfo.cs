using DdnsService.Config;
using DdnsService.Utils;
using DdnsService.Utils.Http;
using Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DdnsService.ApiService
{
    /// <summary>
    /// 获取本机的外网IP
    /// </summary>
    class LocalIPInfo
    {
        public async Task<string> GetLocalIp()
        {
            Log.Info("开始获取当前网络环境外网IP...");
            List<LocalIpApiListItem> localIpApiList = ConfigManager.Now.LocalIpApiList;
            if (localIpApiList == null || localIpApiList.Count == 0)
            {
                throw new Exception("获取外网IP接口不存在，请先添加获取IP接口。");
            }
            if (!await NetworkTools.NetworkCheck())
            {
                throw new Exception("当前网络故障，无法连接互联网。");
            }
            string ipAddress = null;
            foreach (var item in localIpApiList)
            {
                (bool, string) result = await ReuqestApi(item);
                if (result.Item1)
                {
                    ipAddress = result.Item2;
                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        break;
                    }
                }
                else
                {
                    Log.Warn(result.Item2);
                }
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new Exception("获取当前网络外网IP失败。");
            }
            return ipAddress;
        }

        private async Task<(bool, string)> ReuqestApi(LocalIpApiListItem item)
        {
            try
            {
                HttpUtil http = new HttpUtil();

                HttpItem httpItem = new HttpItem()
                {
                    URL = item.Url,
                    Method = item.Method.ToLower() == "get" ? Method.GET : Method.POST,

                };

                HttpResult result = await http.Request(httpItem);
                if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return (false, $"接口{item.Url}请求失败，Http status code:{result.StatusCode}");
                }
                if (string.IsNullOrEmpty(result.Html))
                {
                    return (false, $"接口{item.Url}请求失败，没有返回任何结果。");
                }
                string ipAddress;
                if (item.Type.ToLower() == "json" && result.Html[0] == '{' && result.Html[result.Html.Length - 1] == '}')
                {
                    ipAddress = TryDecodeJsonText(result.Html, item.Field);
                }
                else
                {
                    ipAddress = TryDecodeRegularText(result.Html);
                }
                if (string.IsNullOrEmpty(ipAddress))
                {
                    return (false, $"接口{item.Url}请求失败，解析结果未包含任何IP v4地址。");
                }
                return (true, ipAddress);
            }
            catch (Exception ex)
            {
                return (false, $"接口{item.Url}请求失败，{ex.Message}。");
            }
        }

        private string TryDecodeRegularText(string html)
        {
            Regex rgx = new Regex(@"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)");//中括号[]
            Match match = rgx.Match(html);
            return match.Success ? match.Value : null;
        }

        private string TryDecodeJsonText(string html, string field)
        {
            JObject jObject = JObject.Parse(html);
            return TryDecodeJsonObj(jObject, field);
        }

        private string TryDecodeJsonObj(JObject obj, string field)
        {
            foreach (JProperty jProperty in obj.Properties())
            {
                if (jProperty.Value.Type == JTokenType.Object)
                {
                    string value = TryDecodeJsonObj(jProperty.Value.ToObject<JObject>(), field);
                    if (!string.IsNullOrEmpty(value) && IpAddressVal(value))
                    {
                        return value;
                    }
                }
                else if (jProperty.Value.Type == JTokenType.String)
                {
                    if (jProperty.Name.ToLower() == field.ToLower())
                    {
                        return jProperty.Value.ToString();
                    }
                }
            }
            return null;
        }

        private bool IpAddressVal(string value)
        {
            Regex rgx = new Regex(@"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)");//中括号[]
            return rgx.IsMatch(value);
        }
    }
}
