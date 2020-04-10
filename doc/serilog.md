## Serilog
Библиотека для логирования для .Net/.Ne Core

Возможности
* логирование в разные хранилища (Sink)
* структурированное логирование
* форматирование записей логов
* обогащение (дополнение) данными записей логов - дополнительно к основным данным, прозрачно для разработчика, добавляются дополнительные например: текущий пользователь, id HTTP-запроса, environment, и т.д.

### Установка

```powershell
 Install-Package Serilog
 ```
Для форматирование записей логов в более читаемый json:
```powershell
 Install-Package Serilog.Formatting.Compact
 ```

### Логирование в файл
```powershell
Install-Package Serilog.Sinks.File
```

```c#
var logger = new LoggerConfiguration()
    .WriteTo.File(
        path: @"d:\LOGS\SerilogTests\FileSink\log.log",
        rollingInterval: RollingInterval.Day,
        formatter: new Serilog.Formatting.Compact.CompactJsonFormatter() 
        , buffered: true // Включаем буфер - иначе события логов будут сбрасываться на диск при каждой операции логирования
    )
    .MinimumLevel.Verbose()
    .CreateLogger();

logger.Information("Hello, world!");

logger.Dispose(); // Сброс буфера на диск

```

### Логирование в файл в фоновом потоке

События логов пишутся во внутреннюю очередь (размером 10 000 событий), а затем в файл, поэтому нужно обязательно вызывать ```logger.Dispose()```  в конце программы.

```powershell
Install-Package Serilog.Sinks.Async
```

```c#
var logger = new LoggerConfiguration()
    .WriteTo.Async(
        configure => configure.File(
            path: @"d:\LOGS\SerilogTests\FileAsyncSink\log.log",
            rollingInterval: RollingInterval.Day,
            formatter: new Serilog.Formatting.Compact.CompactJsonFormatter()
        )
        , blockWhenFull: true // Если не задать, то при очень частой записи логово - часть событий может потеряться
    )
    .MinimumLevel.Verbose()
    .CreateLogger();

logger.Information("Hello, world!");

logger.Dispose(); // Сброс очереди на диск
```

### Логирование в Elasticsearch

```powershell
Install-Package Serilog.Sinks.Elasticsearch
```

Для более подходящего форматирования (для elastic) событий логов
```powershell
Install-Package Serilog.Formatting.Elasticsearch
```

В elastic события отсылаются пачками, т.е. пишутся сначала во внутренний буфер, а затем в elastic, поэтому нужно обязательно вызывать ```logger.Dispose()``` в конце программы.

Для надёжности (чтобы не потерять события лога при крахе и перезапуске программы) - события сначала пишутся на диск в файлы по датам, а затем уже в elastic. Причём запоминается последняя успешная позиция, отправленная в elastic.

Так же события, которые не удалось записать в elastic (например ошибка маппинга) записываются в специальный sink ```FailureSink```

```c#
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

            // Файловая очередь
            BufferBaseFilename = @"d:\LOGS\SerilogTests\ElasticSink\",

            // Размер пакета событий, отправляемы в elastic за один раз
            BatchPostingLimit = 1000,

            FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                        EmitEventFailureHandling.WriteToFailureSink |
                        EmitEventFailureHandling.RaiseCallback,

            // Для событий, которые не удалось записатт в elastic 
            FailureSink = new FileSink(@"d:\LOGS\SerilogTests\ElasticSink\failures.txt", new JsonFormatter(), null)
        }
    )
    .MinimumLevel.Verbose()
    .CreateLogger();

logger.Information("Hello, world!");

logger.Dispose(); // Сброс очереди на диск
```

### Логирование в logstash

```powershell
Install-Package Serilog.Sinks.Http
```

Так же как и ```Serilog.Sinks.Elasticsearch``` события логирования отправляются пачками. События сохраняются во внутренней очереди, котрая может быть в памяти или на диске. В случае диска - она аналогична устройству очреде в ```Serilog.Sinks.Elasticsearch``

По умолчанию события в пачке формируются как вложенные в массив внутреннего поля ```events```, для более удобного представления событий - [нужно использовать ```ArrayBatchFormatter```](https://github.com/FantasticFiasco/serilog-sinks-http/wiki/Batch-formatters)

```c#
var logger = new LoggerConfiguration()
    // Ненадёжная очередь
    //.WriteTo.Http("http://localhost:31311", batchFormatter: new ArrayBatchFormatter())

    // Надёжная очередь, события сначала сохраняются на диск
    .WriteTo.DurableHttpUsingFileSizeRolledBuffers(
        "http://localhost:31311"
        , batchFormatter: new ArrayBatchFormatter()
        , bufferBaseFileName: @"d:\LOGS\SerilogTests\HttpSink\buffer"
    )

    //.WriteTo.DurableHttpUsingTimeRolledBuffers("http://localhost:31311", batchFormatter: new ArrayBatchFormatter())
    .MinimumLevel.Verbose()
    .CreateLogger();

logger.Information("Hello, world!");

logger.Dispose(); // Сброс очереди на диск
```


Полезные ссылки
* https://serilog.net/