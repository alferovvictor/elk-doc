## Elastic
Elasticsearch - это поисковая и аналитическая система.

Возможности
* Хранение данных в виде объектов с набором полей
* Поиск и фильтрация данных
* Поддержка кластеров

Устройство
* Данные хранятся в индексах (аналог базы данных)
* **До версии 6.0.0 в одном индексе можно было хранить несколько типов, теперь только один**
* Интерфейс для взаимодействия - HTTP API
* Написана на java, входит в дистрибутив

### **Установка и запуск** (Windows + PowerShell)
* [Скачать дистрибутив (zip-архив)](https://www.elastic.co/guide/en/elasticsearch/reference/current/zip-windows.html)
* Распаковать архив
* Перейти в папку с архивов
* [Elasticsearch использует свою версию java из папки jdk, что бы использовать эту  версию нужно, что бы переменная окружения JAVA_HOME не была установлена](https://www.elastic.co/guide/en/elasticsearch/reference/current/setup.html#jvm-version)
* Проверка установлена ли переменная окружения JAVA_HOME
```powershell
$env:JAVA_HOME
```
* Если JAVA_HOME задана, то сбросить её
```powershell
$env:JAVA_HOME=""
```
* Запустить
```powershell
.\bin\elasticsearch.bat
```
* Проверить, что приложение запущено
```powershell
curl http://localhost:9200
invoke-webrequest http://localhost:9200
invoke-restmethod http://localhost:9200
```

```http
GET http://localhost:9200
```


* Подготовим данных для сохранение я elastic

>`$data=@'
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

  Появился индекс `blog`
  
* Посмотрим сохранённый объект
  ```powershell
    Invoke-WebRequest -Uri http://localhost:9200/blog/post/2?pretty -Method 'Get' -ContentType "application/json"
  ```

Полезные ссылки
* https://habr.com/ru/post/280488/
* https://www.elastic.co/downloads/elasticsearch


Полезные команды
* Просмотр всех индексов http://localhost:9200/_cat/indices?v
* Удалить индекс
```http
DELETE http://localhost:9200/myfilebeat-index2020.04.06 HTTP/1.1
content-type: application/json
```
* Добавить объект

Пример 1
```http
PUT http://localhost:9200/blog/post/4?pretty HTTP/1.1
content-type: application/json

{
  "title": "Веселые котята",
  "content": "<p>Смешная история про котят<p>",
  "tags": [
    "котята",
    "смешная история"
  ],
  "published_at": "2014-09-12T20:44:42+00:00"
}
```

Пример 2
```http
PUT http://localhost:9200/messages/message/1?pretty HTTP/1.1
content-type: application/json

{
    "Filed1" : "AAA",
    "Filed2" : "BBB BBB bbb"
}
```

Пример 3 (через _create)
```http
PUT http://localhost:9200/messages/_create/2 HTTP/1.1
content-type: application/json

{
    "Filed1" : "CCC",
    "Filed2" : "DDD ddd DDD"
}
```

Пример 4
```http
POST http://localhost:9200/messages/_doc/3 HTTP/1.1
content-type: application/json

{
    "Filed1" : "EEE-1",
    "Filed2" : "FFF fff FFFF"
}
```

* Получить объект

Пример 1
```http
GET http://localhost:9200/blog/post/4
content-type: application/json
```

Пример 2
```http
GET http://localhost:9200/messages/_doc/3 HTTP/1.1
content-type: application/json
```

Пример 3
```http
GET http://localhost:9200/myfilebeat-index2020.04.06/_doc/iufbTnEBogqjN7Dpj6NE HTTP/1.1
content-type: application/json
```

Пример 4
```http
GET http://localhost:9200/myfilebeat-index2020.04.06/_search HTTP/1.1
content-type: application/json
```
