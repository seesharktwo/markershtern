
adr by ted7007

Статус:  
 * Предложено

Проблема:  

Мироксервис с товарами доступных для торговли. 
Метод в микросервисе на получение списка товаров доступных для торговли.

Требования:  
Получение клиентом списка продуктов для торговли, каждый из которых будет содержать  id, название, bid and ask  
[Секция с требованиями](https://docs.google.com/document/d/1NvxJDdTIB7qBqGpAQsgQmtSa3DbxsR0sPqAFgcczsjY/edit#heading=h.q61z6p80nw0e)


Решение:
 Основная задача, модель товара.  
Небходимо рассмотреть взаимодействие микросервиса с кафкой - сообщения, негативные сценарии, ошибки между сервисами
Также стоит вопрос о модели товара, взаимодействии сервиса товара с mongoDB


Схема взаимодействия:  

```mermaid
flowchart TD  
	F["Facade"] ==GetProductsRequest==> P["Product MicroService"]
	P ==GetProductsResponce==> F
	subgraph kafka
		BuyOrderCreated
		SellOrderCreated
		BuyOrderDeleted
		SellOrderDeleted
	end
	O["Order MicroService"]
	P ==consume==>kafka
	O ==produce==>kafka

```

Модель товара:  
```csharp
{
	[BSON.ObjectID]
	string Id
	string Name 
	decimal Bid
	decimal Ask
}
```
	
Наш микросервис подписан на топик заявок, пусть будет BuyOrderCreated, SellOrderCreated, BuyOrderDeleted, SellOrderDeleted(смотря как это реализуется в микросервисе заявок) куда будут приходить события на создание новой заявки.  
Из этих событий будет формироваться база текущих заявок, из которой будут извлекаться данные о текущих ценах товаров.


Мною предлагается следующий процесс общения с кафкой используя proto:  
Микросервис товаров опрашивает топик на новые сообщения.  
Каждое полученное сообщение будет десериализовываться из байтовых значений в c#-objs используя десериализатор на основе proto событий заявок:  
 >Это предположительное определение сообщений, окончательный вариант должен быть предложен микросервисом заявок  
```proto
// Сообщение с данными созданной заявки на покупку
message BuyOrderCreated
{
	string id = 1
	string name = 2;
	int quanity = 3;
	DecimalValue price = 4;
	string user_id = 5;
}

// Сообщение с данными созданной заявки на продажу
message SellOrderCreated
{
	string id = 1
	string name = 2;
	int quanity = 3;
	DecimalValue price = 4;
	string user_id = 5;
}

// Сообщение с данными удаленной заявки на покупку
message BuyOrderDeleted
{
	string id = 1
}


// Сообщение с данными удаленной заявки на продажу
message SellOrderDeleted
{
	string id = 1
}
```  

```proto
message DecimalValue
{
	// The whole units of the amount.
	int64 units = 1;

	// Number of nano (10^-9) units of the amount.
	// The value must be between -999,999,999 and +999,999,999 inclusive.
	// If `units` is positive, `nanos` must be positive or zero.
	// If `units` is zero, `nanos` can be positive, zero, or negative.
	// If `units` is negative, `nanos` must be negative or zero.
	// For example $-1.75 is represented as `units`=-1 and `nanos`=-750,000,000.
	int32 nanos = 2;
}
```
	
для DecimalValue нужно создать расширение для простого конвертирования в decimal, подробнее [тут](https://visualrecode.com/blog/csharp-decimals-in-grpc/)  
Сообщения для общения с кафкой хранятся в общей библиотеке .proto файлов и подключаются по необходимости.  

Из вышеупомянутых событий будет формироваться база данных текущих заявок, из которой уже будет извлекаться необходимая информация.  
> В данном случае необходимая нам информация это bid and ask для каждого продукта,  
> но в будущем это можно расширить например до формирования списка активных/завершенных заявок 

Есть два варианта хранения заявок в виде сущностей:  
1. Хранить две разных коллекции по сущностям заявок покупки и продажи  
```csharp
BuyOrder
{
	Guid id;
	string name;
	int quanity;
	decimal price;
	Guid user_id;
 }
SellOrder
{
	Guid id;
	string name;
	int quanity;
	decimal price;
	Guid user_id;
 }
 ```
 2. Хранить одну коллекцию, а заявку покупки от продажи отличать по множеству(enum):  
 ```csharp
 enum OrderType
 {
 SellOrder,
 BuyOrder
 }
 
 Order
 {
 	Guid id;
	string name;
	int quanity;
	decimal price;
	Guid user_id;
	OrderType type;
 }
```
> Итоговое решение о сущности также стоит за микросервисом заявок, а также от задач которые будет решать микросервис товаров.

Также из решения выше формируется проблема обновления текущих данных после удаления заявки.
Мною предлагаются следующие решения:  
 Добавить в модель товара информацию о заявках, которые формируют bid and ask:
```csharp
{
	[BSON.ObjectID]
	string Id
	string Name 
	decimal Bid
	decimal Ask
	// id заявки, которая является лучшим предложением покупателя
	string BidOrderId
	// id заявки, которая является лучшим предложением продавца
	string AskOrderId
}
```  
Тогда при удалении заявки мы будем проверять, является ли она лучшим предложением  
 > найдем продукт с названием из заявки и соотнесем id удаляемой заявки и id заявки лучшего предложения
 * Если не является - тогда ничего не происходит  
 * Если является - тогда мы обновляем информацию о лучшем предложении товара по сформированной базе из событий

--- 

Проблема:  
Метод получения списка товаров для торговли в микросервисе товаров  

Решение:
Этот метод будет вызываться другим сервисом по grpc.  
Метод будет реализован в микросервисе товаров. Реализация будет происходить за счет данных о текущих заявках

```proto
service ProductService {
  rpc GetProducts(GetProductsRequest) returns(GetProductsResponse);
}

message GetProductsRequest
{

}

message GetProductsResponse
{
	repeated Product products = 1;
}

message Product
{
	string id = 1;

	string name = 2;

	DecimalValue bid = 3;

	DecimalValue ask = 4;
}
```  
В будущем нужно будет использовать метод с пагинацией.  
Мною предлагается использовать следующее решение ([источник](https://itnan.ru/post.php?c=1&p=419083)):  
Пагинация с курсором. В запросе будет приходить курсор запрашиваемой страницы,  
который будет представлять из себя закодированную посредством Base64 информацию о поле (если курсор нулевой - значит идет вызов первой порции).  
Далее в ответе будет приходить информация о следующем и предыдущем курсоре.  
Альтернативные вариант ответного сообщения с пагинацией:  
```proto
message GetProductsRequest
{
	// Максимальное кол-во объектов на странице
	int pages_size = 1;
	
	// Курсор запрашиваемой страницы
	string cursor = 2;
}

message GetProductsResponse
{
	repeated Product products = 1;

	// Курсор следующей страницы
	string next_crusor = 2;
	
	// Курсор предыдущей страницы
	string previous_cursor = 3;
}
```  

Контракт на общение service to service будет находиться в обрабатывающем микросервисе, его будет импортировать вызывающий сервис (в нашем случае facade)

---  

Статус:  
 * Предложено

Проблема:  
Получение списка товаров доступных для торговли фасадом

Требования:
Система выводит список товаров для продажи. Если система не может вывести список - система выводит сообщение об ошибке.
[Секция с требованиями](https://docs.google.com/document/d/1NvxJDdTIB7qBqGpAQsgQmtSa3DbxsR0sPqAFgcczsjY/edit#heading=h.q61z6p80nw0e)

Решение:  
Проблема решается с помощью микросервиса фасада.
Метод получения списка будет использовать контракт микросервиса заявок GetProductsWithBidAndAsk, делая на него запрос GetProductsRequest 
и ожидая ответ GetProductsResponce.  
Метод будет также отлавливать ошибку RpcException, если такая будет - фасад должен оповестить клиента о ошибке и попросить обновить страницу.
