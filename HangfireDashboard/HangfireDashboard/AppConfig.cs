namespace HangfireDashboard
{
    public class AppConfig
    {
        public static readonly string HFLogin = "";
        public static readonly string HFPassword = "";
        public static readonly string ProjectMonitorApiKey = "";

#if DEBUG
        public static readonly string HFConnectionString = "";

#else
        public static readonly string HFConnectionString = "";
#endif
    }
}