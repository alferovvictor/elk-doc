using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using Serilog.Sinks.Http.BatchFormatters;

namespace TestSerilogWebAppNetCore
{
    public class Program
    {
        public static int Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();

            var logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .CreateLogger();

            try
            {
                logger.Information("Starting web host");
                CreateWebHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                logger.Information("Stopping web host");
                logger.Dispose();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()

                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    //.ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: @"D:\LOGS\WebNetCoreSerilogToFile\log.log",
                        rollingInterval: RollingInterval.Day,
                        formatter: new Serilog.Formatting.Compact.CompactJsonFormatter()
                        , buffered: true
                    )
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                        {
                            AutoRegisterTemplate = true,
                            OverwriteTemplate = true,
                            DetectElasticsearchVersion = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,

                            CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                            IndexFormat = "web-index-{0:yyyy.MM}",

                            BufferBaseFilename = @"d:\LOGS\serilog-elastic-buffer\",
                            //BatchPostingLimit = 500,

                            FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                            FailureSink = new FileSink(@"d:\LOGS\WebNetCoreSerilogToFile\failures.txt", new JsonFormatter(), null)
                        }
                    )
                    .WriteTo.DurableHttpUsingFileSizeRolledBuffers(
                        "http://localhost:31311"
                        , batchFormatter: new ArrayBatchFormatter()
                        , bufferBaseFileName: @"d:\LOGS\serilog-http-buffer-web\Buffer_"
                    )
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("System", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                )

            ;
    }
}
