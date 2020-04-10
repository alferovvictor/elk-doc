using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using Serilog.Sinks.Http.BatchFormatters;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TestSerilogSinks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var t = new Test();
            t.FileSinkFromConfig();
            t.FileSink();
            t.FileAsyncSink();
            t.HttpSink();
            t.ElasticSink();

            Console.WriteLine("==========================================================================");
            Console.Write("CloseAndFlush ...");
            Log.CloseAndFlush();
            Console.WriteLine("DONE");
            Console.ReadLine();
        }
    }

    public class Test
    {
        public void FileSinkFromConfig()
        {
            Console.Write("File (conf) ... ");
            try
            {
                if (Directory.Exists(@"d:\LOGS\SerilogTests\FileSinkFromConfig"))
                    Directory.Delete(@"d:\LOGS\SerilogTests\FileSinkFromConfig", true);

                var configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

                var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                WriteLogs(logger);
                sw.Stop();
                Console.Write($" {sw.Elapsed.TotalMilliseconds:N0} ms ");

                Console.Write(" done, disposing ... ");
                sw.Restart();
                logger.Dispose();
                sw.Stop();
                Console.WriteLine($" {sw.Elapsed.TotalMilliseconds:N0} ms disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void FileSink()
        {
            Console.Write("File (prog) ... ");
            try
            {
                if (Directory.Exists(@"d:\LOGS\SerilogTests\FileSink"))
                    Directory.Delete(@"d:\LOGS\SerilogTests\FileSink", true);

                var logger = new LoggerConfiguration()
                    .WriteTo.File(
                        path: @"d:\LOGS\SerilogTests\FileSink\log.log",
                        rollingInterval: RollingInterval.Day,
                        formatter: new Serilog.Formatting.Compact.CompactJsonFormatter()
                        , buffered: true
                    )
                    .MinimumLevel.Verbose()
                    .CreateLogger();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                WriteLogs(logger);
                sw.Stop();
                Console.Write($" {sw.Elapsed.TotalMilliseconds:N0} ms ");

                Console.Write(" done, disposing ... ");
                sw.Restart();
                logger.Dispose();
                sw.Stop();
                Console.WriteLine($" {sw.Elapsed.TotalMilliseconds:N0} ms disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void FileAsyncSink()
        {
            Console.Write("File (asyn) ... ");
            try
            {
                if (Directory.Exists(@"d:\LOGS\SerilogTests\FileAsyncSink"))
                    Directory.Delete(@"d:\LOGS\SerilogTests\FileAsyncSink", true);

                var logger = new LoggerConfiguration()
                    .WriteTo.Async(
                        configure => configure.File(
                            path: @"d:\LOGS\SerilogTests\FileAsyncSink\log.log",
                            rollingInterval: RollingInterval.Day,
                            formatter: new Serilog.Formatting.Compact.CompactJsonFormatter()
                            , buffered: true
                        )
                        , blockWhenFull: true
                    )
                    .MinimumLevel.Verbose()
                    .CreateLogger();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                WriteLogs(logger);
                sw.Stop();
                Console.Write($" {sw.Elapsed.TotalMilliseconds:N0} ms ");

                Console.Write(" done, disposing ... ");
                sw.Restart();
                logger.Dispose();
                sw.Stop();
                Console.WriteLine($" {sw.Elapsed.TotalMilliseconds:N0} ms disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ElasticSink()
        {
            Console.Write("Elastic     ... ");

            try
            {
                var logger = new LoggerConfiguration()
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                        {
                            AutoRegisterTemplate = true,
                            OverwriteTemplate = true,
                            DetectElasticsearchVersion = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,

                            CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                            IndexFormat = "app-elastic-{0:yyyy.MM.dd}",

                            BufferBaseFilename = @"d:\LOGS\SerilogTests\ElasticSink\",
                            BatchPostingLimit = 1000,

                            FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                            FailureSink = new FileSink(@"d:\LOGS\SerilogTests\ElasticSink\failures.txt", new JsonFormatter(), null)
                        }
                    )
                    .MinimumLevel.Verbose()
                    .CreateLogger();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                WriteLogs(logger);
                sw.Stop();
                Console.Write($" {sw.Elapsed.TotalMilliseconds:N0} ms ");

                Console.Write(" done, disposing ... ");
                sw.Restart();
                logger.Dispose();
                sw.Stop();
                Console.WriteLine($" {sw.Elapsed.TotalMilliseconds:N0} ms disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void HttpSink()
        {
            Console.Write("Logstash    ... ");

            try
            {
                var logger = new LoggerConfiguration()
                    //.WriteTo.Http("http://localhost:31311", batchFormatter: new ArrayBatchFormatter())
                    .WriteTo.DurableHttpUsingFileSizeRolledBuffers(
                        "http://localhost:31311"
                        , batchFormatter: new ArrayBatchFormatter()
                        , bufferBaseFileName: @"d:\LOGS\SerilogTests\HttpSink\buffer"
                    )
                    //.WriteTo.DurableHttpUsingTimeRolledBuffers("http://localhost:31311", batchFormatter: new ArrayBatchFormatter())
                    .MinimumLevel.Verbose()
                    .CreateLogger();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                WriteLogs(logger);
                sw.Stop();
                Console.Write($" {sw.Elapsed.TotalMilliseconds:N0} ms ");

                Console.Write(" done, disposing ... ");
                sw.Restart();
                logger.Dispose();
                sw.Stop();
                Console.WriteLine($" {sw.Elapsed.TotalMilliseconds:N0} ms disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        private void WriteLogs(ILogger logger)
        {
            logger.Warning("START");

            logger.Verbose("Hello, world!");
            logger.Debug("Hello, world!");
            logger.Information("Hello, world!");
            logger.Warning("Hello, world!");

            logger.Debug("+");
            for (int i = 0; i < 100000; i++)
            {
                logger.Information("iteration: {i}", i);
                Thread.Sleep(10);
            }
            logger.Debug("-");

            logger.Warning("test");
            logger.Verbose("test");
            logger.Information("test");
        }
    }
}
