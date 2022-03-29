using DdnsService.SDKs.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DdnsService.Entity
{
    public class IpHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        [MaxLength(200)]
        public string Record { get; set; }

        /// <summary>
        /// 当前的Ip
        /// </summary>
        [MaxLength(30)]
        public string IP { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public DomainRecordType Type { get; set; }

        /// <summary>
        /// TTL
        /// </summary>
        public uint TTL { get; set; }
    }
}
