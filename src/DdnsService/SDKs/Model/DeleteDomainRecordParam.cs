using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.SDKs.Model
{
    public class DeleteDomainRecordParam
    {
        public string RecordId { get; set; }

        public string Domain { get; set; }

        public string RR { get; set; }
    }
}
