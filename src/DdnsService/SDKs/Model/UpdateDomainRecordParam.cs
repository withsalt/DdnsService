using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace DdnsService.SDKs.Model
{
    public class UpdateDomainRecordParam
    {
        public string Domain { get; set; }

        public string RecordId { get; set; }

        public string RR { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DomainRecordType Type { get; set; }

        public string Value { get; set; }

        public uint TTL { get; set; } = 600;
    }
}
