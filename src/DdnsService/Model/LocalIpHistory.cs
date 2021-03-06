﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DdnsService.Model
{
    public class LocalIpHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }

        [MaxLength(30)]
        public string IP { get; set; }

        [MaxLength(30)]
        public string LastIP { get; set; }

        public long UpdateTs { get; set; }
    }
}
