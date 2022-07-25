### Context

Микросервис, отвечающий за отображение личных товаров, 
которые пользователь купил, либо добавил.

Требования: Получение клиентом списка товаров, находящихся у него в портфеле, каждый из который имеет Id, Name, Quantity, QuantityForTrading

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
    	Product_1 { ProductId, Name, AuthorId, Quantity, QuantityForTrading },  
    	Product_2 { ProductId, Name, AuthorId, Quantity, QuantityForTrading }   
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

Создание Записи (string userId) -  
Проверяет, сущетсвует ли запись в БД. Если запись с таким UserID существует, то
отправить ответ с описанием ошибки с UserId. Если все в порядке, то создать новую запись и 
отправить ответное сообщение с UserId.

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
	string author_marker = 3;
	int32 quantity = 4;
	int32 quantity_for_trading = 5;
} 
```

```proto
enum Errors {
	USER_NOT_FOUND = 1;
	USER_ID_MATCHES_EXISTING = 2;
	USER_NOT_HAVE_PRODUCT = 3;
	USER_NOT_HAVE_QUANTITY_PRODUCT = 4;
	QUANTITY_SALE_EXCEEDS_AVAILABLE_QUANTITY = 5;
	PRODUCT_ON_SALE = 6;
}
```  
1. Пользователь с таким Id не был найден.  
2. Пользователь с таким Id уже существует.  
3. Пользователь не имеет продукт с таким Id.  
4. Пользователь не имеет необходимое количества продукта.  
5. Количество товара в заявке на продажу превышает доступное количество товара, доступное для торговли.  
6. Товар не может быть удален, так как находится на торговле.

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
message GetUserProductsReply {
    string user_id = 1;
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
message AddProductReply {
    string user_id = 1;
    oneof result {
    	Errors error = 2;
    	bool success = 3;
    }
}
```
\- Если в ходе выполнения операций по изменению записей возникла ошибка, то эта ошибка вписывается в error. Микросервис отправляет это сообщение обратно Facade через gRPC
Возможные ошибки:  
- Пользователя с таким UserID не существует.  


```proto
message RemoveProductRequest {
    string user_id = 1;
    string product_id = 2;
}
```
\- Микросервис получает это сообщение через gRPC от Facade. Микросервис делает поиск записи в БД по user_id,  
ищет товар по product_id, если Quantity == QuantityForTrading, то полностью удаляет указанный товар с портфеля пользователя.

```proto
message RemoveProductReply {
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
message Authorization_UserRegisteredEvent {
    string user_id = 1;
}
```
\- Микросервис портфеля активов подписан на топик события регистрации пользователя. Микросервис получает это сообщение,  
после чего создает запись в БД с user_id и пустым списком продуктов, если нет ошибок.


```proto
message Authorization_UserRegisteredEventReply {
    string user_id = 1;
    oneof result {
    	Errors error = 2;
    	bool success = 3;
    } 
}
```  
\- Микросервис портфеля активов подписан на топик события регистрации пользователя. Если в ходе выполнения создания новой записи возникла ошибка, то она включается в error. Если ошибок нет, то success true, что говорит о успешном выполнении операции.  
Возможные ошибки:  
- Запись с таким UserID уже существует.

```proto
message Order_OrderCompletedEvent {
 string user_id_from = 1;
 string user_id_to = 2;
 string product_id = 3;
 int32 quantity = 4;
}
```  
\- Микросервис портфеля активов подписан на топик события совершения сделки. Микросервис проверяет существование двух пользователей по UserID, находит нужный продукт, отнимает quantity продукта у user_id_from и прибавляет его user_id_to. 

```proto
message Briefcase_OrderCompletedEventReply {
 string user_id_from = 1;
 string user_id_to = 2;
 oneof result {
    	Errors error = 2;
    	bool success = 3;
    }
}
```  
\- Микросервис портфеля активов подписан на топик события совершения сделки. Если в ходе операций по изменению записей возникла ошибка, то мы отправляем сообщение в топик с error, иначе success true.  
Возможные ошибки:  
- Пользователь с таким user_id_from не был найден.
- Пользователь с таким user_id_to не был найден.
- У пользователя user_id_from не был найден товар product_id.
- У пользователя user_id_from не был найден товар product_id в нужном количестве.


```proto
message Order_ProductSoldEvent {
    string user_id = 1;
    string product_id = 2;
    int32 quantity = 3;
}
```
\- Микросервис портфеля активов подписан на топик события продажи товара. Микросервис делает поиск записи в БД по user_id   
, ищет товар по product_id, если проблем нет, то проверяет, что quantity <= QuantityForTrading.

```proto
message Briefcase_ProductSoldEventReply {
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
- Количество товара для продажи превышает QuantityForTrading товара в портфеле пользователя.

---