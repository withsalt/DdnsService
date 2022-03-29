using DdnsService.Configs;
using DdnsService.Database;
using DdnsService.Entity;
using DdnsService.Extensions;
using DdnsService.Models;
using DdnsService.SDKs;
using DdnsService.SDKs.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdnsService.Services
{
    public class DomainDdnsService : IDomainDdnsService
    {
        private readonly ILogger<DomainDdnsService> _logger;
        private readonly IDdnsProviderService _provider;
        private readonly IDataAccessService _dataAccess;
        private readonly IEmailNoticeService _email;
        private readonly DdnsConfigNode _ddnsConfigNode;

        public DomainDdnsService(ILogger<DomainDdnsService> logger
            , IDdnsProviderService provider
            , IDataAccessService dataAccess
            , IEmailNoticeService email
            , IOptions<DdnsConfigNode> ddnsConfigNodeOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _ddnsConfigNode = ddnsConfigNodeOptions.Value ?? throw new ArgumentNullException(nameof(ddnsConfigNodeOptions));

        }

        /// <summary>
        /// 验证域名设置
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InitDomainConfigs()
        {
            List<Configs.DomainItem> domains = _ddnsConfigNode.Domains;
            if (domains == null || !domains.Any())
            {
                _logger.ThrowLogError("没有找到需要更新的域名，请先配置需要更新的域名（配置文件：DdnsConfig->Domains）。");
            }
            foreach (var domain in domains)
            {
                if (!_ddnsConfigNode.Providers.Any(p => p.Id == domain.Provider))
                {
                    _logger.ThrowLogError($"配置文件中域名{domain.Domain}的提供厂商不正确，未找到Id为{domain.Provider}的DDNS提供商。");
                }
                if (!DomainInfoVal(domain))
                {
                    _logger.ThrowLogError($"DDNS域名格式不正确，正确的域名格式参考：xxx.xxx.com。");
                }
                if (!DomainTTLVal(domain))
                {
                    _logger.ThrowLogError($"DDNS域名TTL不正确，TTL不能小于60秒。");
                }
            }
            //检查域名是否存在
            foreach (var domain in domains)
            {
                var provider = _provider.Get(domain.Provider);
                if (!await provider.DomainIsExist(domain.Domain))
                {
                    //如果不存在，尝试新建域名
                    if (!await provider.CreateDomain(domain.Domain))
                    {
                        throw new Exception($"{provider}的域名解析列表中不存在域名（{domain.Domain}）且自动创建失败或域名被禁用，请前往{provider}控制台手动添加或启用域名。");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 更新域名记录
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>
        /// 主要干三件事
        /// 1、获取需要更新的域名的当前记录
        /// 2、判断记录是否与当前获取的外网IP相符合
        ///    -->相符合：什么都不做，return
        ///    -->不符合：更新当前域名记录
        /// 3、不管记录更新成功与否，根据配置发送邮件
        /// </remarks>
        public async Task Update(string ip)
        {
            if (!_ddnsConfigNode.IsEnableDdns)
            {
                _logger.LogInformation("Ddns config is disabled.");
            }
            //获取配置文件中的配置列表
            if (_ddnsConfigNode.Domains == null || !_ddnsConfigNode.Domains.Any())
            {
                throw new Exception("没有找到需要更新的域名，请先配置需要更新的域名（配置文件：DdnsConfig->Domains）。");
            }
            bool hasUpdate = false;
            bool hasFailed = false;
            StringBuilder resultSb = new StringBuilder();
            //判断IP是否发生变化
            foreach (var item in _ddnsConfigNode.Domains)
            {
                Stopwatch st = new Stopwatch();
                try
                {
                    IDdnsService provider = _provider.Get(item.Provider);
                    st.Reset();
                    st.Start();
                    DomainRecord updateRecord = null;
                    List<DomainRecord> records = await provider.DescribeSubDomainRecords(item.RR, item.Domain);
                    if (records != null && records.Any())
                    {
                        if (records.Count > 1)
                        {
                            for (int i = 1; i < records.Count; i++)
                            {
                                await provider.DeleteDomainRecord(new DeleteDomainRecordParam()
                                {
                                    RecordId = records[i].RecordId,
                                    Domain = records[i].Domain,
                                    RR = records[i].RR,
                                });
                            }
                        }
                        updateRecord = records[0];
                    }
                    if (updateRecord == null)
                    {
                        //新建解析记录
                        await provider.AddDomainRecord(new AddDomainRecordParam()
                        {
                            Domain = item.Domain,
                            RR = item.RR,
                            Type = DomainRecordType.A,
                            Value = ip,
                            TTL = item.TTL,
                        });
                        //保存记录
                        await _dataAccess.SaveIpHistory(new IpHistory()
                        {
                            Guid = Guid.NewGuid(),
                            Record = item.Record,
                            IP = ip,
                            UpdateTime = DateTime.Now,
                            Type = DomainRecordType.A,
                            TTL = item.TTL,
                        });
                        hasUpdate = true;
                        string msg = $"新建{provider}解析记录[{item.Record}]成功，耗时：{st.ElapsedMilliseconds}ms，解析记录为：{ip}";
                        resultSb.AppendLine(msg);
                        _logger.LogInformation(msg);
                    }
                    else
                    {
                        //检查更新解析记录
                        if (updateRecord.RR != item.RR
                            || updateRecord.TTL != item.TTL
                            || updateRecord.Value != ip)
                        {
                            //更新记录
                            await provider.UpdateDomainRecord(new UpdateDomainRecordParam()
                            {
                                Domain = item.Domain,
                                RecordId = updateRecord.RecordId,
                                RR = item.RR,
                                Type = DomainRecordType.A,
                                Value = ip,
                                TTL = item.TTL,
                            });
                            //保存记录
                            await _dataAccess.SaveIpHistory(new IpHistory()
                            {
                                Guid = Guid.NewGuid(),
                                Record = item.Record,
                                IP = ip,
                                UpdateTime = DateTime.Now,
                                Type = DomainRecordType.A,
                                TTL = item.TTL,
                            });
                            hasUpdate = true;
                            string msg = $"更新{provider}解析记录[{item.Record}]成功，耗时：{st.ElapsedMilliseconds}ms，原记录为：{(updateRecord == null ? "空" : updateRecord.Value)}，更新后解析记录为：{ip}";
                            resultSb.AppendLine(msg);
                            _logger.LogInformation(msg);
                        }
                        else
                        {
                            string msg = $"{provider}解析记录[{item.Record}]无变更，解析记录为：{ip}";
                            resultSb.AppendLine(msg);
                            _logger.LogInformation(msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = $"更新解析记录[{item.Record}]到[{ip}]失败，错误：{ex.Message}";
                    resultSb.AppendLine(msg);
                    _logger.LogWarning(ex, msg);
                }
                finally
                {
                    st.Stop();
                }
            }
            //如果有记录更新或者更新失败
            if (hasUpdate || hasFailed)
            {
                //发送邮件通知
                await SendEmailNotice(resultSb.ToString());
            }
        }

        private bool DomainInfoVal(Configs.DomainItem domain)
        {
            if (domain == null || string.IsNullOrEmpty(domain.Record))
            {
                return false;
            }
            string[] dos = domain.Record.Split('.');
            if (dos.Length < 3)
            {
                return false;
            }
            return true;
        }

        private bool DomainTTLVal(Configs.DomainItem domain)
        {
            if (domain == null)
            {
                return false;
            }
            if (domain.TTL < 60)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task SendEmailNotice(string message)
        {
            try
            {
                var result = await _email.Send("域名解析记录变更通知", message);
                if (result.Item1 != SendStatus.Success)
                {
                    throw new Exception(result.Item2);
                }
                _logger.LogInformation("通知邮件已发送！");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送邮件通知失败，错误：{ex.Message}");
            }
        }
    }
}
