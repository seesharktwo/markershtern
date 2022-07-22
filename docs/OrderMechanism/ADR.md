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
Микросервис выставления заявки от Facade через gRPC получает сообщение с Id пользователя, который собирается выставить заявку,   
наименование продукта, количество продукта, цену. Далее Микросервис через Kafka обращается к Микросервису портфеля активов,  
запрашивает, имеется ли необходимый товар и в нужном количестве. Если проблем нет, то формирует заявку и отправляет её через  
Kafka в Микросервис товаров, доступных для торговли(общий список всех заявок на покупку/продажу).

Микросервис подписан на:  
- событие продажи товара/выставление заявки(Когда пользователь вводит данные в форме и нажимает "Продать").  
- ответ от Микросервиса портфеля активов.
- ответ от Микросервиса товаров, доступных для торговли.

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

и класс:
```
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
```  

### Для gRPC

```proto
message SellProductOrderRequest {
  string user_id = 1;
  string product_name = 2;
  DecimalValue price = 3;
  int32 count = 4;
}
```  
- Сообщение приходит через gRPC от Facade, когда пользователь нажимает на кнопку "продать".

```proto
message SellProductOrderReply {
  string user_id = 1;
  repeated string errors = 2;
}
```  
- Сообщение для ответа Facade, если ошибок нет, то errors будет пуст, а значит заявка успешно выставлена.

### Для Kafka

```proto
message SaleOrderPlacedEvent {
  string user_id = 1;
  string product_name = 2;
  int32 count = 3;
}
```  
- Сообщение для Микросервиса портфеля активов, которое уточняет, имеется ли у пользователя данный товар и в нужном количестве.

```proto
message SaleOrderPlacedEventReply {
  string user_id = 1;
  repeated string errors = 2;
}
```
- Сообщение от Микросервиса портфеля активов. Если список errors пуст, то у пользователя в наличии имеется нужный товар в указанном количестве.  

```proto
message SellOrderCreatedEvent {
  string user_id = 1;
  string product_name = 2;
  DecimalValue price = 3;
  int32 count = 4;
}
```  
- Сообщение для Микросервиса просмотра товаров, доступных для торговли. Микросервис выставления заявки отправляет это сообщение м/c товаров для торговли  
, это является готовой заявкой.

```proto
message SellOrderCreatedEventReply {
  string user_id = 1;
  repeated string errors = 2;
}
```  
- Сообщение от Микросервиса просмотра товаров, доступных для торговли. Если список errors пуст, то это сигнализирует о том,  
что заявка успешно выставлена.
