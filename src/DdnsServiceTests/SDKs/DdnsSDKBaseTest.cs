using DdnsService.Models;
using DdnsService.SDKs;
using DdnsService.SDKs.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdnsServiceTests.SDKs
{
    [TestClass()]
    public abstract class DdnsSDKBaseTest
    {
        public IDdnsService SDK = null;

        public DdnsSDKBaseTest(DdnsProviderType type)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
            //获取DDNS服务商配置
            List<DdnsProviderModel> providers = configuration.GetSection("DdnsConfig:DdnsProviders")?.Get<List<DdnsProviderModel>>();
            if (providers == null || !providers.Any())
            {
                throw new Exception("获取DDNS提供厂商列表失败，请配置DDNS提供厂商（配置文件：DdnsConfig->DdnsProviders）。");
            }
            string acessKey = providers.Where(p => p.Type == type).First()?.AccessKey;
            string accessSecret = providers.Where(p => p.Type == type).First()?.AccessKeySecret;
            switch (type)
            {
                case DdnsProviderType.Aliyun:
                    SDK = new AliyunDdns(acessKey, accessSecret);
                    break;
                case DdnsProviderType.QCloud:
                    SDK = new QCloudDdns(acessKey, accessSecret);
                    break;
            }
        }

        [TestMethod()]
        public virtual async Task AddDomainRecordTest()
        {
            var result = await SDK.AddDomainRecord(new AddDomainRecordParam()
            {
                Domain = "leetgo.com",
                RR = "wula",
                Type = DomainRecordType.A,
                Value = "192.168.1.1",
                TTL = 600
            });
            Assert.IsTrue(result.Status);
        }
    }
}
