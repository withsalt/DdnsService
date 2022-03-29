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
    public class QCloudDdnsTest : DdnsSDKBaseTest
    {
        public QCloudDdnsTest() : base(DdnsProviderType.QCloud)
        {

        }

        [TestMethod()]
        public override void AddDomainRecordTest()
        {
            base.AddDomainRecordTest();
        }

        /// <summary>
        /// 根据RecoardId获取解析记录信息
        /// </summary>
        [TestMethod()]
        public async Task DescribeDomainRecordInfoTest()
        {
            var result = await SDK.DescribeDomainRecordInfo("1089543189", "geeiot.net");
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task UpdateDomainRecordTest()
        {
            var request = new UpdateDomainRecordParam()
            {
                Domain = "geeiot.net",
                RecordId = "1089543182",
                RR = "10086",
                Type = DomainRecordType.A,
                Value = "192.168.26.7",
            };
            var result = await SDK.UpdateDomainRecord(request);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DescribeSubDomainRecordsTest()
        {
            var result = await SDK.DescribeSubDomainRecords("wula", "geeiot.net");
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CreateDomainTest()
        {
            bool result = await SDK.CreateDomain("leetgo.com");
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task DescribeDomainInfoTest()
        {
            var result = await SDK.DescribeDomainInfo("leetgo.com");
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DomainIsExistTest()
        {
            bool result = await SDK.DomainIsExist("leetgo.com");
            Assert.IsTrue(result);
        }
    }
}
