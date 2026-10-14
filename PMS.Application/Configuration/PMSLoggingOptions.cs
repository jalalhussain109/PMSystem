namespace PMS.Application.Configuration
{

    public class PMSLoggingOptions
    {
        public const string SectionName = "Logging";
        public string ConnectionStringName { get; set; } = "DefaultConnection";
        public bool HttpRequestLogging { get; set; } = true;
        public bool HttpResponseLogging { get; set; } = true;
        public int MaxBodyLength { get; set; } = 10000;
        public bool EnableFileLogging { get; set; } = true;
        public string LogRootPath { get; set; } = "Logs";
    }
}
