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
    public class AliyunDdnsTest : DdnsSDKBaseTest
    {
        private AliyunDdns _sdk = null;

        public AliyunDdnsTest() : base(DdnsProviderType.Aliyun)
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
            var result = await SDK.DescribeDomainRecordInfo("753500576812240896", "");
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task UpdateDomainRecordTest()
        {
            var request = new UpdateDomainRecordParam()
            {
                Domain = "geeiot.net",
                RecordId = "753500576812240896",
                RR = "wula",
                Type = DomainRecordType.A,
                Value = "192.168.26.2",
            };
            var result = await SDK.UpdateDomainRecord(request);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DescribeSubDomainRecordsTest()
        {
            var result = await SDK.DescribeSubDomainRecords("a", "leetgo.com");
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CreateDomainTest()
        {
            bool result = await SDK.CreateDomain("letgo.com");
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task DescribeDomainInfoTest()
        {
            var result = await SDK.DescribeDomainInfo("letgo.com");
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DomainIsExistTest()
        {
            bool result = await SDK.DomainIsExist("letgo1.com");
            Assert.IsTrue(result);
        }
    }
}
