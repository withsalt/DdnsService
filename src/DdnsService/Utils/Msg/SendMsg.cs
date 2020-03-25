using DdnsService.Utils;
using DdnsService.Utils.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace WithSalt.Common.Api.Msg
{
    public class SendMsg
    {
        public string MessageApi { get; set; }

        public string MessageCert { get; set; }

        public SendMsg(string msgApi, string msgCert)
        {
            this.MessageApi = msgApi;
            this.MessageCert = msgCert;
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="phone">电话</param>
        /// <param name="msg">要发送的消息</param>
        /// <param name="err">out err</param>
        /// <returns>
        /// 发送成功 返回0 ，并返回ok
        /// 发送失败返回错误码，并返回错误消息
        /// </returns>
        public bool Send(string msg, string phone, out string err)
        {
            err = "Program error";
            try
            {
                if (string.IsNullOrEmpty(msg))
                {
                    err = "Send message is null";
                    return false;
                }
                if (string.IsNullOrEmpty(phone))
                {

                }
                HttpUtil httpHelper = new HttpUtil();
                HttpItem requestItem = new HttpItem()
                {
                    Method = Method.GET,
                    URL = CreateRequestApi(MessageCert, phone, CreateMsgContent(msg))
                };

                HttpResult result = httpHelper.Request(requestItem);
                if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(result.Html))
                {
                    err = "Msg result is null";
                    return false;
                }
                else
                {
                    try
                    {
                        JObject jo = JObject.Parse(result.Html);
                        if (jo != null)
                        {
                            string resultCode = jo["status"].ToString();
                            string resultMsg = jo["msg"].ToString();
                            if (resultCode == "0")
                            {
                                return true;
                            }
                            else
                            {
                                err = string.Format("Result code is {0},result msg is {1}", resultCode, resultMsg);
                                return false;
                            }
                        }
                        else
                        {
                            err = "Can not parse result msg";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        err = ex.Message;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }

        public bool Send(string msg, string[] phone, out string err)
        {
            err = string.Empty;
            if (phone.Length == 0)
            {
                err = "Phones count is zero";
                return false;
            }
            if (string.IsNullOrEmpty(msg))
            {
                err = "Send msg is null";
                return false;
            }
            foreach (var item in phone)
            {
                Send(msg, phone, out err);
            }
            return true;
        }

        private string CreateRequestApi(string cert, string phone, string content)
        {
            try
            {
                if (string.IsNullOrEmpty(cert) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(content))
                {
                    return null;
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(MessageApi);
                sb.AppendFormat("?mobile={0}&content={1}&appkey={2}", phone, HttpUtil.UrlEncode(content), cert);
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string CreateMsgContent(string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(msg))
                {
                    return null;
                }
                return msg;
            }
            catch
            {
                return null;
            }
        }
    }
}
