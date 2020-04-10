# ELK

Основные коппоенты
* Elasticsearch
* Logstash
* Kibana

> Версии должны быть одинаковые

Дополнительные
* Serilog
* Filebeat

## Elastic
Elasticsearch - это поисковая и аналитическая система.

Возможности
* Хранение данных в виде объектов с набором полей
* Поиск и фильтрация данных
* Поддержка кластеров

Устройство
* Данные хранятся в индексах (аналог базы данных)
* **До версии 6.0.0 в одном индексе можно было хранть несколько типов, теперь только один**
* Интерфейс для взаимоджействия - HTTP API
* Написана на java, входит в дистрибутив

Установка (Windows + PowerShell)
* [Скачать дистрибутив (zip-архив)](https://www.elastic.co/guide/en/elasticsearch/reference/current/zip-windows.html)
* Распакавать архив
* Перейти в папку с архивов
* [Elasticsearch использует свою версию java из папки jdk, что бы использовать эту  версию нужно, что бы переменая окружения JAVA_HOME не была установлена](https://www.elastic.co/guide/en/elasticsearch/reference/current/setup.html#jvm-version)
  * Проверка установлена ли переменная окружениея JAVA_HOME
    ```powershell
    $env:JAVA_HOME
    ```
  * Если JAVA_HOME данана, то сбросить её
    ```powershell
    $env:JAVA_HOME=""
    ```
  * Выполнить
    ```powershell
    .\bin\elasticsearch.bat
    ```
  * Проверить, что приложение запущено
    ```powershell
    curl http://localhost:9200
    invoke-webrequest http://localhost:9200
    invoke-restmethod http://localhost:9200
    ```
  * Подготовим данных для сохранение я elastic
    `$data=@'
{
  "title": "Веселые котята",
  "content": "<p>Смешная история про котят<p>",
  "tags": [
    "котята",
    "смешная история"
  ],
  "published_at": "2014-09-12T20:44:42+00:00"
}
'@`

* Сохраним их в elastic
    ```powershell    
    Invoke-WebRequest -Uri http://localhost:9200/blog/post/2?pretty -Method 'Put' -Body $data -ContentType "application/json"
    ```
* Посмотрим индексы http://localhost:9200/_cat/indices?v
  * Появился индекс `blog`
* Посмотрим сохранённый объекn
  ```powershell
    Invoke-WebRequest -Uri http://localhost:9200/blog/post/2?pretty -Method 'Get' -ContentType "application/json"
  ```

Полезные ссылки
* https://habr.com/ru/post/280488/
* https://www.elastic.co/downloads/elasticsearch




## Logstash
Logstash - это конвейер обработки данных на стороне сервера, который принимает данные из нескольких источников одновременно, преобразует их и затем отправляет в «хранилище», например Elasticsearch.

Возможности
* 

## Kibana
Kibana позволяет пользователям визуализировать данные из Elasticsearch с помощью диаграмм и графиков и таблиц.

## Serilog
Serilog - библиотека для логирования в .net/.net core

## Filebeat