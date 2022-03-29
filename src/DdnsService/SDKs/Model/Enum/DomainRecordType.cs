using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.SDKs.Model
{
    public enum DomainRecordType
    {
        A,
        NS,
        MX,
        TXT,
        CNAME,
        SRV,
        AAAA,
        CAA,
        REDIRECT_URL,
        FORWARD_URL
    }
}
