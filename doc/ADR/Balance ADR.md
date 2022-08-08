# Context: 

Микросервис отвечающий за баланс пользователя

# Decision

Микросервис хранит данные о балансе пользователя. 

Обрабатывает транзакции. 

одна транзакция на 1 пользователя. Они обрабатываются отдельно, даже если идет покупка и передача денег от одного к другому. 

## база данных
```proto
users{

	string id
	
	Decimal money
	
	string LastTransactId
}

userTransacts {
	
		string GlobalTransactId
		
		Operation mode,
		
		data Date
	}
```


## Grpc
```proto
service BalanceService 
{
  rpc GetBalance(GetBalanceRequest) returns(GetBalanceResponse);
}

message GetBalanceRequest
{
	string id = 1;
}

message GetBalanceResponse
{
	string user_id = 1;

	DecimalValue balance = 2;
}

```
## Входящие события
```proto

TransactionCommitted
{

	string id_global_transact,

	string id_order,
	
	string id_user,
	
	DecimalValue sum,
	
	Operation MODE,
	
	TransactionType TYPE
	
}

UserCreated {
	
	Тут будет событие когда создается пользователь в системе. Его пока нет.
	
}
```
## Исходящие события 

```proto


TransactionCanceled
{

	string id_global_transact
	
	SourceEventTransaction SOURCE
	
}

TransactionCompleted
{

	string id_global_transact
	
	SourceEventTransaction SOURCE
	
	string id_object
	
	DecimalValue quanity
	
}
```
##

```proto
enum Operation {
	// Операция добавления 
	ADDITION = 0;
	// Операция вычитания  
	SUBTRACT = 1;
}
```
```proto
enum TransactionType {
	// Операция проводки транзакции 
	ACTION  = 0;
	// Операция отката транзакции 
	ROLLBACK = 1;
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
```proto
enum SourceEventTransaction {
	PRODUCT_ORDER_ADDITION_ACTION = 0;
	PRODUCT_ORDER_SUBTRACT_ACTION = 1;
	
	BALANCE_ADDITION_IMMEDIATE = 2;
	BALANCE_SUBTRACT_IMMEDIATE = 3;
	
	PRODUCT_ORDER_ADDITION_ROLLBACK = 4;
	PRODUCT_ORDER_SUBTRACT_ROLLBACK = 5;
	
	BALANCE_ADDITION_ROLLBACK = 6;
	BALANCE_SUBTRACT_ROLLBACK = 7;
	
	PRODUCT_BRIEFCASE_ADDITION_ROLLBACK = 8;
	PRODUCT_BRIEFCASE_SUBTRACT_ROLLBACK = 9;
	
	PRODUCT_BRIEFCASE_ADDITION_IMMEDIATE = 10;
	PRODUCT_BRIEFCASE_SUBTRACT_IMMEDIATE = 11;
}
```
```

# Status

Предложено




