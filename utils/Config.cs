using HandHistoryParser.parser;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HandHistoryParser.utils
{
    internal class Config
    {
        internal static ILogger<Parser> CreateDefaultLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/app.log",
                              rollingInterval: RollingInterval.Day,
                              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            return LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(dispose: true);
            }).CreateLogger<Parser>();
        }

        internal static string GetOutputFilePath(string zipFilePath)
        {
            string archiveName = Path.GetFileNameWithoutExtension(zipFilePath);
            string dateString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputDirectory = Path.Combine("resources", "parsed");
            string outputFileName = $"{dateString}_{archiveName}.txt";
            string outputFilePath = Path.Combine(outputDirectory, outputFileName);
            Directory.CreateDirectory(outputDirectory);
            return outputFilePath;
        }
    }
}
