using HandHistoryParser.parser;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/app.log",
                  rollingInterval: RollingInterval.Day,
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();

var serviceCollection = new ServiceCollection()
    .AddLogging(builder =>
        builder.AddSerilog(dispose: true))
    .AddTransient<Parser>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var handParser = serviceProvider.GetService<Parser>();

handParser?.ParseZipAndWriteOutput(Path.GetFullPath(@"resources\Cash._Holdem._NL25._2013._angrypaca._88k._107MB.zip"));


    