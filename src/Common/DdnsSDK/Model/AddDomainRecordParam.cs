using DdnsSDK.Model.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsSDK.Model
{
    public class AddDomainRecordParam
    {
        public string DomainName { get; set; }

        public string RR { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DomainRecordType Type { get; set; }

        public string Value { get; set; }

        public int TTL { get; set; } = 600;
    }
}
