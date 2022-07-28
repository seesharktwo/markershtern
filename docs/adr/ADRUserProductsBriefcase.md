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

    Line_1 {  
    	UserId, Set Prodcuts [  
    	Product_1 { ProductId, Name, AuthorId, Quantity },  
    	Product_2 { ProductId, Name, AuthorId, Quantity }   
    	]   
    },  
    Line_2...

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

Для решения проблемы с обработкой дубликатов сообщений следует использовать дополнительную коллекцию в MongoDB.

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

### для фасада через gRPC:

С Фасада через gRPC приходит сообщение GetUserProductsRequest, содержащее в себе user_id, по которому будет проходить  
поиск записи в БД, после чего берется список продуктов вместе с user_id, сериализуетя в GetUserProductsReply и отправляется Фасаду.

```proto
service SenderProductsService {
    rpc GetUserProducts (GetUserProductsRequest) returns (GetUserProductsReply)
}
```

```proto
message GetUserProductsRequest {
    string user_id = 1;
}
```
\- Сообщение приходит, когда пользователь запрашивает список продуктов. Микросервис использует user_id для поиска записи, после чего берет список товаров.

```proto
message GetUserProductsResponse {
    oneof reply {
    	repeated Product products = 2;
    	Errors error = 3;
    }
}
```
\- Сообщение для ответа на запрос получения списка продуктов.  
Возможные ошибки:  
- Пользователь с таким UserID не был найден.

```proto
message AddProductRequest {
    string user_id = 1;
    Product product = 2;
}  
```
\- Микросервис получает это сообщение от Facade через gRPC, по user_id ищет запись в БД, и, если проблем нет, то добавляет product в запись.


```proto
message RemoveProductRequest {
    string user_id = 1;
    string product_id = 2;
}
```
\- Микросервис получает это сообщение через gRPC от Facade. Микросервис делает поиск записи в БД по user_id,  
ищет товар по product_id. Если такой товар имеется, то Микросервис через Kafka отправляет сообщение, чтобы убедиться, что товар не находится в продаже. Если нет, то полностью удаляет указанный товар с портфеля пользователя.

```proto
message RemoveProductResponse {
    string user_id = 1;
    oneof result {
    	Errors error = 2;
    	bool success = 3;
    }
}
```
\- Если в ходе выполнения операции по удалению товара возникла ошибка, то включает эту ошибку в error.  
Возможные ошибки:  
- Пользователь с таким UserID не был найден.
- Продукт с таким ProductID не был найден.
- Товар находится в продаже и не может быть удален.


### Для кафки:

```proto
message UserRegisteredEvent {
    string user_id = 1;
}
```
\- Микросервис портфеля активов подписан на топик события регистрации пользователя. Микросервис получает это сообщение,  
после чего создает запись в БД с user_id и пустым списком продуктов, если нет ошибок.

```proto
message OrderCompletedEvent {  
 string message_id = 1;
 string user_id_from = 2;
 string user_id_to = 3;
 string product_id = 4;
 int32 quantity = 5;
}
```  
\- Микросервис портфеля активов подписан на топик события совершения сделки. Микросервис проверяет существование двух пользователей по UserID, находит нужный продукт, отнимает quantity продукта у user_id_from и прибавляет его user_id_to. Поле message_id нужно, чтобы избежать повторной обработки одного и того же сообщения. 

```proto
message Order_ProductOrderSoldCreatedEvent {
    string user_id = 1;
    string product_id = 2;
    int32 quantity = 3;
}
```
\- Микросервис портфеля активов подписан на топик события продажи товара. Микросервис делает поиск записи в БД по user_id   
, ищет товар по product_id, проверяет, имеется ли товар в необходимом количестве.

```proto
message Briefcase_ProductOrderSoldCreatedEventResponse {
    string user_id = 1;
    oneof result {
    	Errors error = 2;
    	bool success = 3;
    }
}
```
\- Микросервис портфеля активов подписан на топик события продажи товара. Если все в порядке, то отправляем success ture. Если в ходе проверки возникли ошибки, то отправляет это сообщение с error.  
Возможные ошибки:
- Пользователя с таким UserID не существует в БД.  
- Продукта с таким ProductID не существует в портфеле пользователя.  
- Количество товара недостаточно.

```proto   
message Briefcase_ProductQuantityDecreaseEvent {   
   string user_id = 1;
   string product_id = 2;
   int32 quantity = 3;
}
```   
\- Это сообщение отправляется, когда у пользователя отнимается некоторое количество товара после факта продажи. На это событие должен быть подписан Микросервис заявок. Он ищет заявку в БД по Type = "sell_order" по user_id, а далее по product_id. Если в заявке указанное количество больше, чем quantity, то заявку необходимо отменить, так как она недействительна.

```proto   
message Briefcase_ProductRemovedEvent {
   string user_id = 1;
   string product_id = 2;
}
```   
\- Это сообщение отправляется, когда пользователь удаляет свой товар из портфеля активов. На это событие подписан Микросервис Заявок. Он ищет заявки с Type = "sell_order", по user_id, по product_id. Далее закрывает все заявки, подходящие под эти условия. 

---
