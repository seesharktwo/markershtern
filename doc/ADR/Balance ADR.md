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
	
	listTransact {
	
		string GlobalTransactId
		
		Operation mode,
		
		data Date
	}
	
}
```


## Grpc
```proto
service BalanceService 
{
  rpc GetBalance(UserBalanceRequested) returns(UserBalanceResponced);
}
```
## Входящие события
```proto
message UserBalanceRequested
{
	string id = 1;
}

BalanceChanged
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
message UserBalanceResponced
{
	string user_id = 1;

	DecimalValue balance = 2;
}

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
	ADDITION = 1;
	// Операция вычитания  
	SUBTRACT = 2;
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
	// Операция проводки транзакции 
	PRODUCT_ORDER_ADDITION_IMMEDIATE = 1;
	PRODUCT_ORDER_SUBTRACT_IMMEDIATE = 2;
	
	BALANCE_ADDITION_IMMEDIATE = 3;
	BALANCE_SUBTRACT_IMMEDIATE = 4;
	
	PRODUCT_ORDER_ADDITION_ROLLBACK = 5;
	PRODUCT_ORDER_SUBTRACT_ROLLBACK = 6;
	
	BALANCE_ADDITION_ROLLBACK = 7;
	BALANCE_SUBTRACT_ROLLBACK = 8;
	
	PRODUCT_ORDER_ADDITION_ROLLBACK = 9;
	PRODUCT_ORDER_SUBTRACT_ROLLBACK = 10;
	
	PRODUCT_BRIEFCASE_ADDITION_IMMEDIATE = 11;
	PRODUCT_BRIEFCASE_SUBTRACT_IMMEDIATE = 12;
}
```
```

# Status

Предложено




