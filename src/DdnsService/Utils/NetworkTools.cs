using DdnsService.Configs;
using DdnsService.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DdnsService.Utils
{
    public class NetworkTools
    {
        private static readonly List<string> _hosts = new List<string>()
        {
            "223.5.5.5",
            "114.114.114.114",
        };

        /// <summary>
        /// 通过Ping验证网络是否断开
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Ping()
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
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取外网IP
        /// </summary>
        /// <param name="ipApis"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<(bool, string)> TryGetNetworkIp(List<ApisNode> ipApis)
        {
            if (ipApis == null || ipApis.Count == 0)
            {
                throw new Exception("获取外网IP接口不存在，请先添加获取IP接口。");
            }
            StringBuilder errorsSb = new StringBuilder();
            foreach (var item in ipApis)
            {
                (bool state, string msg) result = await ReuqestApi(item);
                if (result.state)
                {
                    return result;
                }
                else
                {
                    errorsSb.AppendLine(result.msg);
                }
            }
            return (false, errorsSb.ToString());
        }

        #region private

        private static async Task<(bool, string)> ReuqestApi(ApisNode item)
        {
            try
            {
                var client = new RestClient(new RestClientOptions()
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
                });
                var request = new RestRequest(item.Url, item.Method.Equals("get", StringComparison.OrdinalIgnoreCase) ? Method.Get : Method.Post)
                {
                    Timeout = 3000,
                };
                request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.61 Safari/537.36");
                request.AddHeader("Accept", "text/html,application/json,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                var result = await client.ExecuteAsync(request);
                if (result.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(result.Content))
                {
                    throw new Exception($"Http request failed, url is {item.Url}, method is {item.Method}. http status code is {result.StatusCode}, result is {result.Content}");
                }
                string ipAddress;
                switch (item.Type)
                {
                    case GetIpApiType.Json:
                        {
                            ipAddress = TryDecodeJsonText(result.Content, item.Field);
                        }
                        break;
                    case GetIpApiType.Regex:
                        {
                            ipAddress = TryDecodeRegularText(result.Content);
                        }
                        break;
                    default:
                        throw new Exception($"Unknow get ip from content type. type value is {item.Type}");
                }
                if (string.IsNullOrEmpty(ipAddress))
                {
                    return (false, $"Http request failed, url is {item.Url}, method is {item.Method}, The parsing result does not contain any IPv4 addresses.");
                }
                return (true, ipAddress);
            }
            catch (Exception ex)
            {
                return (false, $"Http request failed, url is {item.Url}, method is {item.Method}. {ex.Message}.");
            }
        }

        private static string TryDecodeRegularText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }
            Regex rgx = new Regex(@"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)");//中括号[]
            Match match = rgx.Match(html);
            return match.Success ? match.Value : null;
        }

        private static string TryDecodeJsonText(string html, string field)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }
            if(html[0] != '{' && html[^1] != '}' 
                || html[0] != '[' && html[^1] != ']')
            {
                return null;
            }
            JObject jObject = JObject.Parse(html);
            if(jObject == null)
            {
                return null;
            }
            return TryDecodeJsonObj(jObject, field);
        }

        private static string TryDecodeJsonObj(JObject obj, string field)
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
                else if (jProperty.Value.Type == JTokenType.Array)
                {
                    foreach (var item in jProperty.Value)
                    {
                        string value = TryDecodeJsonObj(item.ToObject<JObject>(), field);
                        if (!string.IsNullOrEmpty(value) && IpAddressVal(value))
                        {
                            return value;
                        }
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

        private static bool IpAddressVal(string value)
        {
            Regex rgx = new Regex(@"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)");//中括号[]
            return rgx.IsMatch(value);
        }

        #endregion

    }
}
