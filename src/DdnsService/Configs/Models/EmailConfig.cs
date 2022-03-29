using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdnsService.Configs
{
    public class EmailConfig
    {
        public string SmtpServer { get; set; }

        public bool SmtpSsl { get; set; }

        public uint SmtpPort { get; set; }

        public string SendMailAddress { get; set; }

        public string SendMailName { get; set; }

        public string Password { get; set; }

        public string ReceiveMailAddress { get; set; }

        public static string Position { get { return "AppSettings:EmailConfig"; } }

    }
}
