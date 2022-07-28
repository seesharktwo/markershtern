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

Запись В БД представляет собой:   

```
    Line_1 {  
    	UserId, Set Prodcuts [  
    	Product_1 { ProductId, Name, AuthorId, Quantity },  
    	Product_2 { ProductId, Name, AuthorId, Quantity }   
    	]   
    },  
    Line_2...
```

Также у нас будет коллекция для сообщений, чтобы избежать повторной обработки дубликата.

```
[
   {
      "message_id" : "a2d85e36-dd8d-4bf0-a3a0-7349ecbcf32d"
   },
   {
      "message_id" : "7833b11b-786b-483a-bdf2-1b6b5e9ddf41"
   }
]
```
Запись в БД будет создаваться, когда произошло событие регистрации пользователя.

Микросервис подписан на:
- событие регистрации пользователя,  
- событие совершения сделки,
- событие продажи товара.

В каждом методе, где используется ID, нужно использовать метод перевода из string в
Guid.

Создание Записи (string userId) - Создает новую запись в БД с userId. Если такая запись с таким userId уже есть,   
то новая запись не создается. 

Редактирование Записи(string userId, product product) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID не существует, то  
отправить ответ с описанием ошибки с UserId. Если все в порядке, то изменить запись и  
отправить ответное сообщение UserId.

Получение Списка Товаров(string userId) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID не существует, то  
отправить ответ с описанием ошибки с UserId. Если все в порядке, то отправить ответным сообщением  
список продуктов с UserId.

Передача товара(string userId_from, string userId_to, string productId, int32 quantity) -  
Проверяет, существуют ли записи в БД.  Если записей с такими UserID/ProductID не существует, то  
отправить ответ с описанием ошибки с UserId. Если все в порядке, изменить запись, отправить ответным сообщением  
UserId.

Удаление товара(string userId, string productId) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID/ProductID не существует, то  
отправить ответ с описанием ошибки с UserId. Если все в порядке, удалить товар, отправить ответным сообщением  
UserId.


---

### .proto

```proto
message Product {
	string product_id = 1;
	string name = 2;
	string author_id = 3;
	int32 quantity = 4;
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
	// Товар не может быть удален, так как находится на торговле.
	PRODUCT_ON_SALE = 4;
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
message ToWrapper {
	repeated Product products = 1;
}
```


### для фасада через gRPC:

С Фасада через gRPC приходит сообщение GetUserProductsRequest, содержащее в себе user_id, по которому будет проходить  
поиск записи в БД, после чего берется список продуктов, сериализуетя в GetUserProductsReply и отправляется Фасаду.

```proto
service SenderProductsService {
    rpc GetUserProducts (GetUserProductsRequest) returns (GetUserProductsReply)
}
```

```proto
// Сообщение, которое указывает, что Facade запросил список продуктов пользователя.
message GetUserProductsRequest {
    // user_id это ID пользователя, чьи товары необходимо предоставить Facade.
    string user_id = 1;
}
```


```proto
// Сообщение для ответа на запрос получения списка продуктов.
message GetUserProductsResponse {
    // Если в ходе выполнения возникли ошибки, то отправляется ошибка, если нет, то wrapper
    oneof reply {
        // Представляет собой обертку для repeated product 
    	ToWrapper wrapper = 1;
    	Errors error = 2;
    }
}
```
Возможные ошибки:  
- Пользователь с таким UserID не был найден.

```proto
// Микросервис получает это сообщение от Facade, сигнализирующее о том, что пользователь хочет добавить товар. 
message AddProductRequest {
    // ID пользователя, которому нужно добавить товар.
    string user_id = 1;
    // Продукт, который нужно добавить
    Product product = 2;
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
}
``` 


```proto
message RemoveProductResponse {
    string user_id = 1;
    oneof result {
    	Errors error = 2;
    	bool success = 3;
    }
}
```
Возможные ошибки:  
- Пользователь с таким UserID не был найден.
- Продукт с таким ProductID не был найден.
- Товар находится в продаже и не может быть удален.


### Для кафки:

```proto
// Микросервис подписан на топик события регистрации пользователя.
message UserRegisteredEvent {
    // Микросервис создает новый документ в своей БД используя это поле.
    string user_id = 1;
}
```


```proto
// Микросервис подписан на топик события совершения сделки. 
message OrderCompletedEvent {  
 // Нужно, чтобы избежать повторной обработки дубликата.
 string message_id = 1;
 // ID пользователя, у которого мы собираемся списать товар.
 string user_id_from = 2;
 // ID пользователя, которому мы хотим зачислить товар.
 string user_id_to = 3;
 // ID товара, который мы хотим добавить.
 string product_id = 4;
 // Количество товара для передачи.
 int32 quantity = 5;
}
```  


```proto
// Микросервис подписан на события продажи товара.
message Order_ProductOrderSoldCreatedEvent {
    // ID пользователя, по которому ведется поиск.
    string user_id = 1;
    // ID товара, по которому ведется поиск.
    string product_id = 2;
    // Проверяет, есть ли необходимое количество товара.
    int32 quantity = 3;
}
```


```proto
message Briefcase_ProductOrderSoldCreatedEventResponse {
    string user_id = 1;
    oneof result {
    	Errors error = 2;
    	bool success = 3;
    }
}
```
Возможные ошибки:
- Пользователя с таким UserID не существует в БД.  
- Продукта с таким ProductID не существует в портфеле пользователя.  
- Количество товара недостаточно.

```proto   
// Это сообщение отправляется, когда у пользователя отнимается некоторое количество товара после факта продажи. На это событие должен быть подписан Микросервис заявок. // Он ищет заявку в БД по Type = "sell_order" по user_id, а далее по product_id. Если в заявке указанное количество больше, чем quantity, то заявку необходимо  
//отменить, так как она недействительна.
message Briefcase_ProductQuantityDecreaseEvent {   
   string user_id = 1;
   string product_id = 2;
   int32 quantity = 3;
}
```   

```proto   
// Это сообщение отправляется, когда пользователь удаляет свой товар из портфеля активов. На это событие подписан Микросервис Заявок. Он ищет заявки с Type   
// "sell_order", по user_id, по product_id. Далее закрывает все заявки, подходящие под эти условия. 
message Briefcase_ProductRemovedEvent {
   string user_id = 1;
   string product_id = 2;
}
```   

---
