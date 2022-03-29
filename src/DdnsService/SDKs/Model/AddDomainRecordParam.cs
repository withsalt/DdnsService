using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.SDKs.Model
{
    public class AddDomainRecordParam
    {
        public string Domain { get; set; }

        public string RR { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DomainRecordType Type { get; set; }

        public string Value { get; set; }

        public uint TTL { get; set; } = 600;
    }
}
