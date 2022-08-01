### Context

Микросервис, реализующий механизм выставления заявок. 

ссылка на требования:  
https://docs.google.com/document/d/1NvxJDdTIB7qBqGpAQsgQmtSa3DbxsR0sPqAFgcczsjY/edit#heading=h.h3k0b09pdfj1   

---

### Status 

Предложено

---

### Decision

Приблизительный принцип работы микросервиса:  
\- Создание заявки. Facade делает первичную валидацию, что заявку выставить возможно, после чего отправляет сообщение в Микросервис. На основе данных формируется заявка и сохраняется. Далее микросервис ищет подходящие заявки, находит, отправляет данные в микросервис транзакций. Если не находит, то заявка остается лежать в БД. Микросервис транзакций проделывает необходимые операции, после чего заявки либо закрываются, либо обновляются( если закрыт не весь объем ), либо остаются нетронутыми.


БД микросервиса содержит записи по типу:

```
// В качестве индекса UserID
[6bf280fe-fa36-4fde-a69e-434cc76d45ae] 
{
   "order_id" : "91d6bbdf-6388-4da9-9991-9c92e8b751f7",
   "type" : 1,
   "product_id" : "b5a7c6d8-3533-4d45-97ba-500f202b9077",
   "quantity" : 3232,
   "price" : 123.00,
}
```

Также будет коллекция для завершенных заявок.

---



### .proto

Так как Proto не поддерживает Decimal, определяем свой:  
https://docs.microsoft.com/ru-ru/dotnet/architecture/grpc-for-wcf-developers/protobuf-data-types  
https://visualrecode.com/blog/csharp-decimals-in-grpc/

```proto
message DecimalValue {
  int64 units = 1;
  sfixed32 nanos = 2;
}
```

```proto
// Типы заявок
enum OrderType {
   // Тип заявок на продажу.
   SELL_ORDER = 1;
   // Тип заявок на покупку.
   BUY_ORDER = 2;
}
```

### Kafka

```proto   
// Сообщение приходит от Facade. На основе полей формируется заявка и сохраняется в БД.
message OrderCreatedEvent {
  string user_id = 1;
  
  // Тип заявки
  OrderTypes type = 2;
  string product_id = 3;
  DecimalValue price = 4;
  int32 quantity = 5;   
}
```  

```proto
message Order {
   string order_id = 1;
   string user_id = 2;
   OrdersTypes type = 3;
   string product_id = 4;
   int32 quantity = 5;
   DecimalValue price = 6;
}
```


```proto  
// Сообщение для микросервиса транзакций.
// На это событие подписан микросервис транзакций.
message OrderCandidateOccurredEvent {
   string message_id = 1;
   Order from = 2; 
   Order to = 3; 
}
```

```proto
// Сообщение от микросервиса транзакций.
// На это подписан микросервис заявок.
// если success true, то заявки закрываются
// если false, то заявки продолжают висеть.
message OrderCandidateOccurredEventResponse {
   string message_id = 1; 
   bool success = 2;
}
```
