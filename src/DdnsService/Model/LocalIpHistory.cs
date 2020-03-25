using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.Model
{
    class LocalIpHistory
    {
        public string IP { get; set; }

        public string LastIP { get; set; }

        public long UpdateTime { get; set; }
    }
}
