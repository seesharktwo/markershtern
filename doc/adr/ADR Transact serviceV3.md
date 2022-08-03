
# Context: 
Миксервис контролирующий целостность транзакций. 

# Decision

решение третье. Добавлен виртуальный счет, последовательность проводок. 
блокировки на основе виртуального счета. 



```mermaid
sequenceDiagram

МикроСервисТранзакций ->> МикроСервисТранзакций:s
    СчетА ->> СчетА:Баланс 100 - Виртуальный 100 
    СчетБ ->> СчетБ:Баланс 100 - Виртуальный 100 

    СкладА ->> СкладА:Персики 0 - Виртуальный 0 
    СкладБ ->> СкладБ:Персики 10 - Виртуальный 10  

    МикроСервисТранзакций ->> СчетА:Транзакция А Списать 100   
    СчетА ->> СчетА:Баланс 100 - Виртуальный 0

    МикроСервисТранзакций ->> СкладБ:Транзакция А Снять 10 персиков 
    СкладБ ->> СкладБ:Персики 10 - Виртуальный 0 

    МикроСервисТранзакций ->> СкладА:Транзакция А Добавить 10 персиков
    СкладА ->> СкладА:Персики 0 - Виртуальный 10

    МикроСервисТранзакций ->> СчетБ:Транзакция А добавить 100
    СчетБ ->> СчетБ:Баланс 100 - Виртуальный 200 

    МикроСервисТранзакций ->> СчетА:Транзакция Б Списать 100
    СчетА ->> МикроСервисТранзакций: Виртуальный баланс 0. Списать 100 нельзя. 
    СчетА ->> МикроСервисТранзакций: Отмена транзкции Б

    МикроСервисТранзакций ->> МикроСервисТранзакций:Транзкция А валидна. Начало закрепления транзакции
   
    МикроСервисТранзакций ->> СчетА:(фиксация)Транзакция А Списать 100 
    СчетА ->> СчетА:Баланс 0 - Виртуальный 0

    МикроСервисТранзакций ->> СкладБ:(фиксация)Транзакция А Снять 10 персиков 
    СкладБ ->> СкладБ:Персики 0 - Виртуальный 0

    МикроСервисТранзакций ->> СкладА:(фиксация)Транзакция А Добавить 10 персиков
    СкладА ->> СкладА:Персики 10 - Виртуальный 10

    МикроСервисТранзакций ->> СчетБ:(фиксация)Транзакция А добавить 100
    СчетБ ->> СчетБ:Баланс 200 - Виртуальный 200
МикроСервисТранзакций ->> МикроСервисТранзакций:Все дочерние транзакции А зафиксированны
МикроСервисТранзакций ->> МикроСервисТранзакций:Транзакция А выполнена полностью

```
Работает в данный момент следующим образом. 

Есть разделение счета/складов/других вариантов на Текущие значение счета, и виртуального. 

Это нужно чтобы создать частичные блокироки на транзакции, которые будут учитывать последовательность, не завершенных блокировок. 

Чтобы избежать ситуации, когда отмена транзакции, или последовательность транзакций, приведут счет в не валидное состояние. 

## Проблема

Проблема заключается в том, где хранить состояние виртуального счета. 
Чья это ответственность. 

## Решение

Весь учет виртуальных данных, и блокировок вынести в микросервис транзакций. 

Перенос учета виртуальных данных на сервис транзакции, позволит вести учет с дополнительными измерениями, необходимые для контроля целостности самих транзакций. 

Так же это позволит убрать со всех сервисов ответственность за валидацию, и поэтапную проверку. оставляя только прямые обязанности в пополнии баланса/списания 
или аналогичных для других сервисов. 

Из плюсов так же, возможное использование средств транзкций mongoDB когда то в будущем. по нескольким документам. 


## Общая структура решения

Микросервису баланса, и склада(Где будут хранится товары для торговли) рекомендуется хранить Id локальной транзакции. 
И ее вид операции, чтобы избежать повторной проводки. 

Примерная структура базы данных



```proto
BalanceValue
{

	string idObject
	Decimal virtual 
	Decimal summ 
	[]BalanceTransact 
	{

		ID Глобальной транзакции 
		
		Значение 
		
		дата
		
		Id счета
		
		string status 
		
	}
}


OrderValue
{

	string idObject
	int virtual 
	int quanity  
	[]OrderTransact 
	{

		ID Глобальной транзакции 
		
		Значение 
		
		дата
		
		Id склада
		
		bool выполнен
		
		Bool отменен
		
	}
}


глобальная транзакция 
{

	Id транзакции 
	
	Значение
	
	Дата
	
	IsCompleted
	
	IsError
}
```
## События 

### Входящие события 

```proto
BalanceReplenished 
{

	DecimalValue sum,
	
	string id_user_buyer
	
}

OrderClosed
{

	DecimalValue sum,
	
	int count_product,
	
	string id_product,
	
	string id_order,
	
	string id_user_buyer,
	
	string id_user_seller,
	
}

TransactionCanceled
{

	string id_global_transact
	
	Source_Event_Transaction source
	
}

TransactionCompleted
{

	string id_global_transact
	
	Source_Event_Transaction source
	
	string id_object
	
	DecimalValue quanity
	
}
```

TransactionCompleted принимает Id обьекта, счета/элемента заявки

и количество 

### Исходящие события 

```proto
BalanceChanged
{

	string id_global_transact,

	string id_order,
	
	string id_user,
	
	DecimalValue sum,
	
	Operation mode,
	
	TransactionType type
	
}

ProductChanged
{

	string id_global_transact,
	
	string id_product,
	
	string id_user,
	
	string id_order,
	
	int count,
	
	Operation mode,
	
	TransactionType type
	
}
```

## 

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
enum Source_Event_Transaction {
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
	
	PRODUCT_ORDER_ADDITION_IMMEDIATE = 11;
	PRODUCT_ORDER_SUBTRACT_IMMEDIATE = 12;
}
```


# Status

Предложено