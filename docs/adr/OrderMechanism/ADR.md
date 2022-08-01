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
{
    "user_id" : "d28e5a67-8da4-4b76-97ce-1a648705c6e8",
    "orders" : {
      "active" : [
         {
            "order_id" : "91d6bbdf-6388-4da9-9991-9c92e8b751f7",
            "type" : 1,
            "product_id" : "b5a7c6d8-3533-4d45-97ba-500f202b9077",
            "quantity" : 3232,
            "price" : 123.00,
         },
        ],
      "inactive" : [
         {
            "users_id_to" : [
	       {
	          "user_id" : "d6eef007-549b-4f94-b5f5-5a5738070dbc",
		  "quantity" : 300,
		  "price" : 300
	       },
	       {
	          "user_id" : "xdsefdsm-zxcb-4uj4-ikm5-53r53vbb0dbc",
		  "quantity" : 33,
		  "price" : 33
	       }
	    ],
            "order_id" : "31d6bbdf-6312-4da9-9881-9cx2e8b121f7",
            "type" : 2,
            "product_id" : "43a7c6d2-3543-4d75-97ba-500f20asdr37",
            "quantity" : 333,
            "price" : 333.00,
	    // Дата закрытия заявки
            "date_completed" : 2022-02-09,
         },
        ]  
    }
  }
```

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
enum OrderTypes {
   // Тип заявок на продажу.
   Sell_order = 1;
   // Тип заявок на покупку.
   Buy_order = 2;
}
```

### Kafka

```proto
message Header {
   string key = 1;
   string value = 2;
}
```

```proto   
// Сообщение приходит от Facade. На основе полей формируется заявка и сохраняется в БД.
message OrderCreatedEventRequest {
  Header header = 1;
  string user_id = 2;
  
  // Тип заявки
  OrderTypes type = 3;
  string product_id = 4;
  DecimalValue price = 5;
  int32 quantity = 6;   
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
message OrdersClosedEventRequest {
   Header header = 1;
   Order from = 2; 
   Order to = 3; 
}
```

```proto
// Сообщение от микросервиса транзакций.
// На это подписан микросервис заявок.
// если success true, то заявки закрываются
// если false, то заявки продолжают висеть.
message OrdersClosedEventRequest {
   Header header = 1;
   Order from = 2; 
   Order to = 3; 
   bool success = 4;
}
```
