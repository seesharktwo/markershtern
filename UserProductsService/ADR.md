Context

Микросервис, отвечающий за отображение не находящихся в продаже личных товаров, 
которые пользователь купил, либо добавил.
_______________________________________________________________________________
Status

Предложено
_______________________________________________________________________________
Decision

Запись В БД представляет собой:
Line_1 
{ 
	UserId, Set Prodcuts [
	Product_1 { ProductId, Name, AuthorMarker, Price, Count, Description },
	Product_2 { ProductId, Name, AuthorMarker, Price, Count, Description } 
	]
}
Line_2...

Запись в БД будет создаваться в момент, когда пользователь прошел регистрацию.
Используется метод создания записи.

Микросервис подписан на события:
    регистрации пользователя,
    получения списка продуктов,
    полечения информации о продукте,
    удаление товара, 
    добавление товара,
    покупку товара,

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
    отправить ответ с описанием ошибки. Если все в порядке, то отправить ответным сообщением
    пустой список ошибок.

Удаление товара(string userId, string productId) -
    Проверяет, существует ли запись в БД.  Если записи с таким UserID/ProductID не существует, то
    отправить ответ с описанием ошибки. Если все в порядке, то отправить ответным сообщением
    пустой список ошибок.


_______________________________________________________________________________
.proto


service LineEditor {
    rpc (UserRegistered) returns (EditLineResponse)
    rpc (EditLineRequest) returns (EditLineResponse)
    rpc (ProductsRequest) returns (ProductsResponse)
    rpc (ProductDescriptionRequest) returns (ProductDescriptionResponse)
    rpc (ProductSellRequest) returns (ProductSellResponse)
    rpc (ProductDeleteRequest) returns (ProductDeleteResponse)
}

https://docs.microsoft.com/ru-ru/dotnet/architecture/grpc-for-wcf-developers/protobuf-data-types
https://visualrecode.com/blog/csharp-decimals-in-grpc/
Так как Protocol Buffers не поддерживает Decimal, то следует использовать это(две ссылки выше поясняют):

message DecimalValue {
	int64 units = 1;
	sfixed nanos = 2;
}

и реализовать класс DecimalValue:

public partial class DecimalValue
{
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

message UserRegisteredRequest {
    string userId = 1;
} - Сообщение приходит, когда пользователь регистрируется. Оно нужно для создания записи в БД.

message Product {
	string productId = 1;
	string name = 2;
    string authorMarker = 3;
	decimalValue price = 4;
    string description = 5;
    int32 count = 6;
} 

message EditLineRequest {
    string userId = 1;
	product product = 2;
} - Сообщение приходит, когда пользователь добавляет/покупает продуктов.

message EditLineResponse {
    string userId = 1;
    repeated string errors = 2;
} - Сообщение о удачном/неудачном создании/изменении записи в БД.

message ProductsRequest {
    string userId = 1;
} - Сообщение приходит, когда пользователь запрашивает список продуктов.

message ProductsResponse {
    string userId = 1;
    product products = 2;
    repeated string errors = 3; 
} - Сообщение для ответа на запрос получения списка продуктов. 

message ProductDescriptionRequest {
    string userId = 1;
    string productId = 2;
} - Сообщение приходит, когда пользователь запрашивает информацию о продукте.

message ProductDescriptionResponse {
    string userId = 1;
    product product = 2;
    repeated string errors = 2;
} - Сообщение для ответа на запрос получения информации о продукте.

message ProductSellRequest {
    string userId = 1;
    string productId = 2;
    int32 count = 3;
} - Сообщение приходит, когда пользователь хочет продать товар.

message ProductSellResponse {
    string userId = 1;
    repeated string errors = 2;
} - Сообщение для ответа на запрос продажи товара.

message ProductDeleteRequest {
    string userId = 1;
    string productId = 2;
} - Сообщение приходит, когда пользователь хочет удалить товар.

message ProductDeleteResponse {
    string userId = 1;
    repeated string errors = 2;
} - Сообщение для ответа на запрос удаления товара.

Так как Protocol Buffers не поддерживает напрямую тип Guid, то ID различных
объектов представлены как тип string, который в будущем будем расшифровываться 
по схеме 8-4-4-4-12: 3422b448-2460-4fd2-9183-8000de6f8343
_______________________________________________________________________________
