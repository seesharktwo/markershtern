### Context

Микросервис, отвечающий за отображение личных товаров, 
которые пользователь купил, либо добавил.

Требования: Получение клиентом списка товаров, находящихся у него в портфеле, каждый из который имеет Id, Name, Quantity.

ссылка на требования:
https://docs.google.com/document/d/1NvxJDdTIB7qBqGpAQsgQmtSa3DbxsR0sPqAFgcczsjY/edit#heading=h.ex6x3ie43ne2

---

### Status

Предложено

---

### Decision

Запись в БД будет создаваться, когда произошло событие регистрации пользователя.

Запись В БД представляет собой:   


```
// В качестве индекса id пользователя.
[ef48ad6c-be94-4ad6-acd6-f54952e06d26] 
{
   "products" : [ 
   "product_id" : "",
   "quantity" : 257
   ]
}
```

Коллекция продуктов
```
// Индекс, он же id продукта.
[113fc7b2-7ca7-4e6e-8692-fdfa2a0a582f]
{
   "name" : "iron",
   "author_id" : "376379ed-6d8a-4274-b951-e8a811439b91",
}
```

---

Создание Записи (string userId) - Создает новую запись в БД с userId. Если такая запись с таким userId уже есть,   
то новая запись не создается. 

Получение Списка Товаров(string userId) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID не существует, то  
отправить ответ с описанием ошибки с UserId. Если все в порядке, то отправить ответным сообщением  
список продуктов с UserId.

Добавление Товара(user_id, product) -    
Проверяет, существует ли запись в БД с таким UserID. Происходит поиск в БД по user_id, если запись с таким user_id существует, то добавить товар в коллекцию с товарами, а также в коллекцию с портфелями пользователей в количестве product.quantity. Если запись с таким product.name уже существует в общей коллекции с товарами, то добавить product.quantity в коллекцию пользователю в портфель.
Микросервис транзакций гарантирует корректное выполнение.

Уменьшение товара(user_id, product_id, quantity) - 
Проверяет, существует ли запись в БД с таким UserID. Происходит поиск в БД по user_id, product_id, от quantity в БД отнимается quantity из параметра. Если оба quantity равны, то товар полностью удаляется из порфтеля пользователя. 
Микросервис транзакций гарантирует корректное выполнение.

Удаление товара(string userId, string productId, string authorId) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID/ProductID не существует, то  
отправить error. Необходимо сравнить authorId и userId, если они равны, то удалить товар возможно. Пользователь может удалить товар только в своем портфеле, но не из коллекции всех товаров.


---

### .proto

```proto
message Product {
	string name = 1;
	string author_id = 2;
	int32 quantity = 3;
} 
```

```proto
enum Errors {
	// Пользователь с таким Id не был найден. 
	USER_NOT_FOUND = 1;
	// Пользователь не имеет продукт с таким Id.  
	USER_NOT_HAVE_PRODUCT = 2;
	// Пользователь не имеет необходимое количества продукта. 
	USER_NOT_HAVE_QUANTITY_PRODUCT = 3;
}
```  

```proto
message SuccessResponse {
   
}
```

Так как Protocol Buffers не поддерживает напрямую тип Guid, то ID различных
объектов представлены как тип string, который в будущем будем расшифровываться 
по схеме 8-4-4-4-12: 3422b448-2460-4fd2-9183-8000de6f8343

Так как Protocol Buffers не поддерживает Decimal, то следует использовать это(две ссылки ниже поясняют):

https://docs.microsoft.com/ru-ru/dotnet/architecture/grpc-for-wcf-developers/protobuf-data-types

https://visualrecode.com/blog/csharp-decimals-in-grpc/

```proto
message DecimalValue {
	int64 units = 1;
	sfixed32 nanos = 2;
}
```

```proto
// Так как oneof не поддерживает repeated, то приходится использовать обертку.
message ProductsList {
	repeated Product products = 1;
}
```

### Получение списка продуктов пользователя

```proto
service UserBriefcase {
   rpc GetUserProducts (GetUserProductsRequest) returns (GetUserProductsResponse)
}
```

```proto
// Сообщение от Facade через gRPC. Запрос списка товаров.
message GetUserProductsRequest {
    // user_id это ID пользователя, чьи товары необходимо предоставить Facade.
    string user_id = 1;
}
```


```proto
// Сообщение для ответа на запрос получения списка продуктов.
message GetUserProductsResponse {
    // Если в ходе выполнения возникли ошибки, то отправляется ошибка, если нет, то wrapper
    oneof response {
        // Представляет собой обертку для repeated product 
    	ProductsList list = 1;
    	Errors error = 2;
    }
}
```
Возможные ошибки:  
- Пользователь с таким UserID не был найден.

### Добавление товара

```proto
service UserBriefcase {
   rpc AddProduct (AddProductRequest) returns (AddProductResponse)
}
```

```proto
// Микросервис получает это сообщение от Facade через gRPC, сигнализирующее о том, что пользователь хочет добавить товар. 
message AddProductRequest {
    // ID пользователя, которому нужно добавить товар.
    string user_id = 1;
    // Продукт, который нужно добавить
    Product product = 2;
}  
```

```proto
message AddProductResponse {
   oneof result {
      Errors error = 1;
      SuccessResponse success = 2;
   }
}
```
Возможные ошики:   
\- Данный продукт уже существует.

### Удаление товара

```proto
service UserBriefcase {
   RemoveProduct (RemoveProductRequest) returns (RemoveProductResponse) 
}
```

```proto
// Микросервис получает это сообщение от Facade, сигнализирующее о том, что пользователь хочет удалить товар.   
// Прежде чем удалить товар, необходимо убедиться, что товар не находится в продаже.
message RemoveProductRequest {
    // ID пользователя, которому нужно удалить товар.
    string user_id = 1;
    // ID товара, который необходимо удалить.
    string product_id = 2;
    // Пользователь может удалить только тот товар, который он создал сам.
    string author_id = 3;
}
``` 

```proto
message RemoveProductResponse {
   oneof result {
      Errors error = 1;
      SuccessResponse success = 2;
   }
}
```

### Проверка необходимых данных для микросервиса заявок.  | gRPC

```proto
service UserBriefcase {
   rpc ValidateOrder (ValidateOrderRequest) returns (ValidateOrderResponse)
}
```

```proto
// Сообщение от Facade через gRPC. Проверяет, имеется ли необходимое количество товара у пользователя 
// для создания заявки.
message ValidateOrderRequest {
    // ID пользователя, по которому ведется поиск.
    string user_id = 1;
    // ID товара, по которому ведется поиск.
    string product_id = 2;
    // Проверяет, есть ли необходимое количество товара.
    int32 quantity = 3;
    }
```


```proto
// Сообщение для Facade. Ответ для Facade, который подтверждает, что у пользователя имеется необходимое количество продукта.
message ValidateOrderResponse {
    oneof result {
    	Errors error = 1;
    	SuccessResponse success = 2;
        }
    }
```
Возможные ошибки:
- Пользователя с таким UserID не существует в БД.  
- Продукта с таким ProductID не существует в портфеле пользователя.  
- Количество товара недостаточно.

### Создание новой записи в БД. | Kafka

```proto
// Микросервис подписан на топик события регистрации пользователя.
message UserRegisteredEvent {
    // Микросервис создает новый документ в своей БД используя это поле.
    string user_id = 1;
}
```

```proto
message UserRegisteredSuccess {
   SuccessResponse result = 1;
}
```

###  Передача товара между пользователями. | Kafka

Микросервис транзакций гарантирует, что товар спишется у одного пользователя и зачислиться другому.

```proto
// Сообщение от микросервиса транзакций на списание или добавление товара.
message ProductChanged {
   string id_global_transact = 1;
   string id_product = 2;
   string id_user = 3;
   string id_order = 4;
   int count = 5;
   Operation mode = 6;
   TransactionType type = 7;
}
```

```proto
enum TransactionType {
   // Операция проводки транзакции 
   IMMEDIATE = 1;
   // Операция отката транзакции 
   ROLLBACK = 2;
}
```

```proto
enum Operation {
   // Операция добавления 
   ADDITION = 1;
   // Операция вычитания  
   SUBTRACT = 2;
}
```

```proto
// Ответ для микросервиса транзакций для случаев с ошибкой операции.
message TransactionCanceled {
	string id_global_transact = 1;
	SourceEventTransaction source = 2;
}
```

```proto
// Ответ для микросервиса транзакций для случаев, если операция прошла успешно.
message TransactionCompleted {
	string id_global_transact = 1;
	SourceEventTransaction source = 2;
	string id_object = 3;
	DecimalValue quanity = 4;
}
```

```proto
enum SourceEventTransaction {
   // Операция проведения транзакции 
   PRODUCT_ORDER_ADDITION_ACTION = 1;
   PRODUCT_ORDER_SUBTRACT_ACTION = 2;
   
   BALANCE_ADDITION_ACTION = 3;
   BALANCE_SUBTRACT_ACTION = 4;
   
   PRODUCT_BRIEFCASE_ADDITION_ACTION = 5;
   PRODUCT_BRIEFCASE_SUBTRACT_ACTION = 6;
   
   PRODUCT_BRIEFCASE_ADDITION_ROLLBACK = 7;
   PRODUCT_BRIEFCASE_SUBTRACT_ROLLBACK = 8;
   
   PRODUCT_ADDITION_ROLLBACK = 9;
   PRODUCT_SUBTRACT_ROLLBACK = 10;
   
   BALANCE_ADDITION_ROLLBACK = 11;
   BALANCE_SUBTRACT_ROLLBACK = 12;
}
```


### Уведомление, что количество товара у пользователя изменилось. | Kafka

Это нужно, чтобы микросервис заявок мог удалить недействительные.

```proto   
// Это сообщение отправляется, когда у пользователя отнимается некоторое количество товара после факта продажи. На это событие должен быть подписан Микросервис заявок. // Он ищет заявку в БД по Type = "sell_order" по user_id, а далее по product_id. Если в заявке указанное количество больше, чем quantity, то заявку необходимо  
//отменить, так как она недействительна.
message ProductSoldEvent {   
   // Для защиты от дубликатов.
   string order_id = 1;   
   string user_id = 2;
   string product_id = 3;
   int32 quantity = 4;
}
```   

```proto   
// Это сообщение отправляется, когда пользователь удаляет свой товар из портфеля активов. На это событие подписан Микросервис Заявок. 
message ProductRemovedEvent {
   string user_id = 1;
   string product_id = 2;
}
```   

---
