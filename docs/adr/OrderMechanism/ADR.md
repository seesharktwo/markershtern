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
Facade делает первичную валидацию, что заявку выставить возможно, после чего отправляет сообщение через gRPC в Микросервис. На основе данных формируется заявка и сохраняется.

Микросервис подписан на:  
- событие ответа от Микросервиса портфеля активов.  
- событие ответа от Микросервиса баланса.  


БД микросервиса содержит записи по типу:

```
Active {
	Line_1 {
	Order_Id, Type, User_Id, Product_Id, Quantity, Price
	},
	Line_2 {
	Order_Id, Type, User_Id, Product_Id, Quantity, Price
	}
}
Inactive {
	Line_1 {
	Order_Id, User_Id_from, User_Id_to, Product_Id, Quantity, Price
	}
	Line_2 {
	Order_Id, User_Id_from, User_Id_to, Product_Id, Qnatity, Price
	}
}
```

---

### Фоновый процесс   

Микросервис заявок должен иметь фоновый процесс обработки заявок, который будет закрывать подходящие. 

Микросервис берет заявку покупки, и сравнивает с заявкой продажи по цене и количеству. Если все в порядке, отправляет Микросервису Баланса через Kafka сообщение о необходимости изменить баланс пользователей, отправляет Микросервису портфеля через Kafka сообщение о необходимости изменить портфели пользователей, если все в порядке, то закрывает эти завки. 

Доработаю этот процесс.

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
enum Errors {
	// Пользователь не найден.
	USER_NOT_FOUND = 1;
	// Пользователь с таким ID существует.
	USER_ID_MATCHES_EXISTING = 2;
	// Пользователь не имеет товара с таким ID
	USER_NOT_HAVE_PRODUCT = 3;
	// Пользователь не имеет необходимое количество товара.
	USER_NOT_HAVE_QUANTITY_PRODUCT = 4;
	// Продукт находится в продаже.
	PRODUCT_ON_SALE = 5;
	// У пользователя недостаточно денег.
	USER_NOT_HAVE_MONEY = 6;
}
```  


### Для gRPC

```proto   
// Сообщение приходит от Facade.
message SellProductOrderRequest {
  // На основе этих полей формируется заявка на продажу.
  string user_id = 1;
  string product_id = 2;
  DecimalValue price = 3;
  int32 quantity = 4;   
  google.protobuf.Timespan timelife = 5;
  bool close_b
}
```  

\- Сообщение для ответа Facade, если ошибок нет, то в сообщении будет success true. 
Возможные ошибки:  
- Пользователь с таким UserID не был найден.  
- Товар с таким ProductID не был найден.  
- Товара с таким ProductID недостаточно(количество в портфеле меньше, чем указано в заявке).


```proto
message BuyProductOrderRequest {
  string order_id = 1;
  string user_id = 2;
  string product_id = 3;
  DecimalValue price = 4;
  int32 quantity = 5;
}
```  
\- Сообщение приходит через gRPC от Facade, когда пользователь нажимает на кнопку "Купить". Микросервис отправляет другое сообщение через Kafka в микросервис баланса, чтобы узнать, есть ли у пользователя необходимая сумма.

```proto
message BuyProductOrderReply {
  string user_id = 1;
  oneof {
  	Errors error = 2;
	bool success = 3;
  }
}
```  
\- Сообщение для ответа Facade, если ошибок нет, то в сообщении будет success true.  
Возможные ошибки:  
- Пользователь с таким UserID не был найден.  

```proto
message OrderIsDone {
   string user_id = 1;
   string order_id = 2;
   string user_id_to = 3; 
   DecimalValue value = 4;
}
```   
\- Сообщение для Facade, которое указывает, что заявка была закрыта.

### Для Kafka

```proto
message Order_SaleOrderPlacedEvent {
  string order_id = 1
  string user_id = 2;
  string product_id = 3;
  int32 quantity = 4;
}
```  
\- Сообщение для Микросервиса портфеля активов, которое уточняет, имеется ли у пользователя данный товар и в нужном количестве.  

```proto
message Briefcase_SaleOrderPlacedEventReply {
  string order_id = 1;
  string user_id = 2;
  oneof {
  	Errors error = 3;
	bool success = 4;
  }
}
```
\- Сообщение от Микросервиса портфеля активов. Если success true, то у пользователя в наличии имеется нужный товар в указанном количестве. После этого заявка выставляется. 

```proto
message Briefcase_BuyOrderPlacedEvent {
  string order_id = 1;
  string user_id = 2;
  DecimalValue value = 3;
}
```  
\- Сообщение для Микросервиса баланса, которое уточняет, имеется ли у пользователя необходимое количество валюты для совершения сделки.


```proto
message Balance_BuyOrderPlacedEventReply {
  string order_id = 1;
  string user_id = 2;
  oneof = {
     Errors error = 3;
     bool success = 4;
  }
}
```  
\- Сообщение от Микросервиса Баланса. Если необходимое количество суммы есть у пользователя, то success true, иначе ошибка. Если success true, то заявка на покупку выставляется.   
Возможные ошибки:   
- Пользователь с UserID не был найден.
- У пользователя недостаточно средств.   

```proto   
message Order_ChangeBalanceEvent {
   string user_id_from = 1;
   string user_id_to = 2;
   DecimalValue value = 3;
}
```   
\- Сообщение для Микросервиса Баланса о необходимости изменения балансов пользователей.   

```proto   
message Order_ChangeBriefcaseEvent {
   string user_id_from = 1;
   string user_id_to = 2;  
   string product_id = 3;
   int32 quantity = 4;
}
```   
\- Сообщение для Микросервиса Портфеля о необходимости изменения портфелей пользователей.   
