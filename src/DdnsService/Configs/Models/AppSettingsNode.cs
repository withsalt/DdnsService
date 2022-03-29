namespace DdnsService.Configs
{
    public class AppSettingsNode
    {
        public int IntervalTime { get; set; }

        public bool IsSaveIpHistory { get; set; }

        public bool IsEnableEmailNotice { get; set; }

        public EmailConfig EmailConfig { get; set; }

        public static string Position { get { return "AppSettings"; } }
    }
}
