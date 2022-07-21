# Context: 

Микросервис отвечающий за баланс пользователя

# Decision

Микросервис хранит данные о балансе пользователя. 

Обрабатывает транзакции. 

одна транзакция на 1 пользователя. Они обрабатываются отдельно, даже если идет покупка и передача денег от одного к другому. 

Состояние счета не может быть отрицательным. В таком случаи баланс считается не валидным. 

Не валидные балансы прекращают обрабатывать любые транзакции, кроме обратных транзакций, пока счет не станет валидным. 


## база данных

users{

	string id
	
	double money
	
	listTransact {
	
		string GlobalTransactId
		
		string TransactId
		
		data Date
	
	}
	
	isValid
}

## Grpc

service BalanceService 
{
  rpc GetBalance(UserRequest) returns(BalanceResponce);
}

message UserRequest
{
	string Id = 1;
}

message BalanceResponce
{
	string userId = 1;

	double balance = 2;
}

## События

### Консюмер

BalanceTransact {

	string idGlobalTransact,

	string idTransact,

	string idOrder,

	string idUser,

	double sum,

	bool isAdd,

	bool isReverse

}

CreateUser {
	
	Тут будет событие когда создается пользователь в системе. Его пока нет.
	
}

### Продюсер 

AbortingTransaction {

	string idGlobalTransact

	string idTransact

	string source

}

CompletedTransaction {

	string idGlobalTransact

	string idTransact

	string source

}

NotValidStatus {

	id
	
	string source
}



# Status

Предложено




