using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using PMS.Application.Configuration;
using Serilog;
using Serilog.Events;

namespace LMKR.Shared.Logging.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    ///
    /// var builder = WebApplication.CreateBuilder(args);
    ///
    /// builder.AddSharedSerilog();
    public static WebApplicationBuilder AddSerilogExtention(this WebApplicationBuilder builder)
    {
        Console.WriteLine("========== AddSerilogExtention CALLED ==========");
        //--------------------------------------------------
        // logging (configuration)
        //--------------------------------------------------

        var loggingOptions = builder.Configuration.GetSection(PMSLoggingOptions.SectionName).Get<PMSLoggingOptions>() ?? new PMSLoggingOptions();


        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {
            //-------------------------------------
            // Console
            //-------------------------------------

            loggerConfig.WriteTo.Console( outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}");

            //--------------------------------------------------
            // Logs Files 
            //--------------------------------------------------
            if (loggingOptions.EnableFileLogging)
            {
                var logsFolder = Path.Combine(loggingOptions.LogRootPath);
                var errorFolder = Path.Combine(logsFolder, "Errors");
                Directory.CreateDirectory(logsFolder);
                Directory.CreateDirectory(errorFolder);

                loggerConfig.WriteTo.File(Path.Combine(logsFolder, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    shared: true,
                    rollOnFileSizeLimit: true);

                loggerConfig.WriteTo.File(Path.Combine(errorFolder, "error-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 90,
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    shared: true,
                    rollOnFileSizeLimit: true);
            }
        });

        return builder;
    }
}