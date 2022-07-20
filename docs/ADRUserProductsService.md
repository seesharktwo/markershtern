### Context

Микросервис, отвечающий за отображение не находящихся в продаже личных товаров, 
которые пользователь купил, либо добавил.

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
    	Product_1 { ProductId, Name, AuthorMarker, Price, Count, Description },  
    	Product_2 { ProductId, Name, AuthorMarker, Price, Count, Description }   
    	]   
    },  
    Line_2...

Запись в БД будет создаваться, когда произошло событие регистрации пользователя.

Микросервис подписан на:
- событие регистрации пользователя,
- событие запроса списка продуктов,
- событие удаления товара, 
- событие добавления товара,
- событие покупки товара,

В каждом методе, где используется ID, нужно использовать метод перевода из string в
Guid.

Создание Записи (string userId) -  
Проверяет, сущетсвует ли запись в БД. Если запись с таким UserID существует, то
отправить ответ с описанием ошибки. Если все в порядке, то создать новую запись и 
отправить ответное сообщение бещ ошибок.

Редактирование Записи(string userId, product product) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID не существует, то  
отправить ответ с описанием ошибки. Если все в порядке, то изменить запись и  
отправить ответное сообщение без ошибок.

Получение Списка(string userId) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID не существует, то  
отправить ответ с описанием ошибки. Если все в порядке, то отправить ответным сообщением  
список продуктов и пустой список ошибок.

Получение информации о продукте(string userId, string productId) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID/ProductID не существует, то  
отправить ответ с описанием ошибки. Если все в порядке, то отправить ответным сообщением  
продукт и пустой список ошибок.

Продажа товара(string userId, string productId, string count) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID/ProductID не существует, то  
отправить ответ с описанием ошибки. Если все в порядке, изменить запись, отправить ответным сообщением  
пустой список ошибок.

Удаление товара(string userId, string productId) -  
Проверяет, существует ли запись в БД.  Если записи с таким UserID/ProductID не существует, то  
отправить ответ с описанием ошибки. Если все в порядке, удалить товар, отправить ответным сообщением  
пустой список ошибок.


---

### .proto

```proto
	message Product {
		string product_id = 1;
		string name = 2;
		string author_marker = 3;
		string description = 4;
		int32 count = 5;
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

и реализовать класс DecimalValue:

	public partial class DecimalValue {
	
		private const decimal NanoFactor = 1_000_000_000;
		
		public DecimalValue(long units, int nanos) 
		{
			Units = units;
			Nanos = nanos;
		}

		public static implicit operator decimal(CustomTypes.DecimalValue grpcDecimal)
		{
			return grpcDecimal.Units + grpcDecimal.Nanos / NanoFactor;
		}

		public static implicit operator CustomTypes.DecimalValue(decimal value) 
		{
			var units = decimal.ToInt64(value);
			var nanos = decimal.ToInt32((value - units) * NanoFactor);
			return new CustomTypes.DecimalValue(units, nanos);
		}
    }
    

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
- Сообщение приходит, когда пользователь запрашивает список продуктов.

```proto
	message GetUserProductsReply {
	    string user_id = 1;
	    repeated product products = 2;
	    repeated string errors = 3; 
	}
```
- Сообщение для ответа на запрос получения списка продуктов.

### Для кафки:

```proto
	message UserRegisteredEvent {
	    string user_id = 1;
	}
```
- Микросервис портфеля активов подписан на топик события регистрации пользователя. Микросервис получает это сообщение,  
после чего создает запись в БД с user_id и пустым списком продуктов.

```proto
	message ProductAddedEvent {
	    string user_id = 1;
	    product product = 2;
	}  
```
- Микросервис портфеля активов подписан на топик события добавления/покупки товара пользователем.  
Микросервис получает это сообщение, по user_id ищет запись в БД, и, если проблем нет,то добавляет product в запись.

```proto
	message ResultChangeRecordEventReply {
	    string user_id = 1;
	    repeated string errors = 2;
	}
```
- Микросервис портфеля активов подписан на топики событий регистрации пользователя, добавления/покупки товара.  
Это сообщение нужно, если в ходе выполнения операций по созданию/изменению записей возникли ошибки. Микросервис  
отправляет эти ошибки в другой топик, на который подписаны другие микросервисы.

```proto
	message ProductSoldEvent {
	    string user_id = 1;
	    string product_id = 2;
	    int32 count = 3;
	}
```
- Микросервис портфеля активов подписан на топик события продажи товара. Микросервис делает поиск записи в БД по user_id   
, ищет товар по product_id, если проблем нет, то удаляет указанное количество(count) товара из портфеля пользователя.

```proto
	message ProductSoldEventReply {
	    string user_id = 1;
	    repeated string errors = 2;
	}
```
- Микросервис портфеля активов подписан на событие продажи товара. Если в ходе выполнения операции по удалению  
товара возникла ошибка, то отправляет это сообщение со списком ошибок.

```proto
	message ProductRemovedEvent {
	    string user_id = 1;
	    string product_id = 2;
	}
```
- Микросервис портфеля активов подписан на событие удаления товара. Микросервис делает поиск записи в БД по user_id,  
ищет товар по product_id, если проблем нет, то полностью удаляет указанный товар с портфеля пользователя

```proto
	message ProductRemovedEventReply {
	    string user_id = 1;
	    repeated string errors = 2;
	}
```
- Микросервис портфеля активов подписан на событие удаления товара. Если в ходе выполнения операции по удалению  
товара возникла ошибка, то отправляет это сообщение со списком ошибок.

---
