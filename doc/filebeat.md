## Filebeat

Filebeat - это легковесный поставщик данных логирования для их пересылки в централизованное хранилище логов. Установленный в качестве агента на серверах, Filebeat отслеживает указанные файлы логов, собирает события логов и перенаправляет их либо в Elasticsearch, либо в Logstash для индексации.

Возможности
* Следит за источником логов
* Парсит логи
* Может трансформировать записи
* Отсылает в output
* Плагины input и output

### Устройство

На примере папки с логами.

Filebeat следит за файлами с логами, запоминает обработанные файлы и последнюю обработанную позицию в файле, что бы при перезапуске продолжить обработку не начиная с начала. Перед отправкой в output сохраняет события в очередь. Очередь может быт в памяти, [размер очереди настраивается](https://www.elastic.co/guide/en/beats/filebeat/7.6/configuring-internal-queue.html). Очередь может быть и на дске, но это ещё beta и не рассматриваем такой способ.

> Плохо отслеживает позицию в сетевых shared-folders

Processors дают возможность обрабатывать события логов например:
* добавить поля (например environment: development/test/production).
* скопировать поле в другое место
* переименовать поле
* удалить поле
* выполнить скрипт javascript

Т.е. с помощью processors можно привести логи из разных источников к одному виду. Но по сравнению с logstash возможностей по трансформации событий - меньше.

Балансировка поддерживается для output: для hosts указывается несколько серверов и параметр ```loadbalance: true```


### **Установка и запуск** (Windows + PowerShell)
* [Скачать дистрибутив (zip-архив)](https://www.elastic.co/downloads/beats/filebeat)
* Распаковать архив
* Перейти в папку с архивов
* Запускать filebeat под Администратором
* Конфигурация находится в файле filebeat.yml, нужно раскомментировать и дописать некоторые строки:

Пример 1: следим за логами в папке ```d:\LOGS\WebApp1``` и отсылаем в elastic

    ```yaml
    filebeat.inputs:
    - type: log
      enabled: true    
      index: "filebeat-index%{+yyyy.MM.dd}"
      json.message_key: message
      json.process_array: true
      paths:
        - d:\LOGS\WebApp1\*

    output.elasticsearch:
      hosts: ["localhost:9200"]
    ```

Пример 2: следим за логами в папке ```d:\LOGS\WebApp1``` и отсылаем в logstash

    ```yaml
    filebeat.inputs:
    - type: log
      enabled: true    
      json.message_key: message
      json.process_array: true
      paths:
        - d:\LOGS\WebApp1\*

    output.logstash:
      hosts: ["localhost:5044", "localhost:6044"] # Несколько серверов - для балансировка
      loadbalance: true # Балансировка
    ```

>Параметр ```json.message_key: message``` отвечает за преобразование сообщения лога в JSON иначе в лог попадает просто текстовая строка (```message```)

* Проверка
```powershell
.\filebeat.exe test config
./filebeat test config -e
```
* Установить KIBANA dashboard (займёт несколько минут)
```powershell
 ./filebeat setup -e - KIBANA dashboard
```
* Запуск
```powershell
.\filebeat.exe -e -c filebeat.yml -d "publish"
```
> ключ d "publish" отлаживать (вывод в консоль) сообщений типа publish
ключ d "*" отлаживать (вывод в консоль) сообщений всех типов

В результате в KIBANA появится паттерн filebeat-*

* В файле ```filebeat.reference.yml``` есть список всех доступных параметров
* Можно добавить к записям (событиям) лога дополнительные поля, например

    ```yaml
    filebeat.inputs:
    - type: log
      enabled: true    
      json.message_key: message
      json.process_array: true
      paths:
        - d:\LOGS\WebApp1\*

      fields:
        field1: aaa
        review: 2

    output.logstash:
      hosts: ["localhost:5044"]
    ```
* Или дополнительно трансформировать с помощью процессоров:

    ```yaml
    filebeat.inputs:
    - type: log
      enabled: true    
      index: "filebeat-index%{+yyyy.MM.dd}"
      json.message_key: message
      json.process_array: true
      paths:
        - d:\LOGS\WebApp1\*

    output.elasticsearch:
      hosts: ["localhost:9200"]

    processors:
      - add_host_metadata: ~
      - add_cloud_metadata: ~
      - add_docker_metadata: ~
      - add_kubernetes_metadata: ~
      - add_tags: # Добавляем теги
          tags: [web, production]
          target: "environment"  
      - copy_fields: # Копируем поля
          fields:
          - from: json.@mt
            to: messageTemplate
          - from: json.@r
            to: rr
          fail_on_error: false
          ignore_missing: true
      - drop_fields: # Удаляем поля
          fields: ["host", "json.SourceContext", "json.message"]
      - timestamp: # Датой и временем события считать поле
          field: json.@t
          layouts:
            - '2006-01-02T15:04:05.999Z'

    ```

### Дополнительно

Можно активировать модули. Например для работами с [логами IIS](https://www.elastic.co/guide/en/beats/filebeat/7.6/filebeat-module-iis.html).
```powershell
./filebeat modules enable iis - IIS
```
Его настройки можно изменить в файле ```.\modules.d\iis.yml```, а посмотреть доступные в папке ``````.\modules\iis``````

### Полезные ссылки
* https://www.elastic.co/guide/en/beats/filebeat/7.6/filebeat-getting-started.html
* https://www.elastic.co/guide/en/beats/filebeat/7.6/filebeat-modules-quickstart.html


