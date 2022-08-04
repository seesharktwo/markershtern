# Context: 
Микросервис отвечающий за аутентификацию и авторизацию пользователя
# Status
Предложено
# Decision
### Данный в БД хранятся следующим образом: 

    ObjectID user_Id
    string login
    string hash
    string salt
    
> поля user_Id и login уникальны, salt вычисляется для каждого пользователя отдельно.  

> на данном этапе в базе данных храниться какое-то количество тестовых профилей.
### Proto: 
```proto
    service AuthService{
        rpc Login(LoginRequest) returns(LoginResponse);
    }

    message LoginRequest {
        string login = 1;
        string password = 2;
    }

    message LoginResponse {
        string user_Id = 2;
    }    
```
При получении LoginRequest микросервис сравнивает вложенные в него данные с теми, что хранятся в БД.
* Если пользователя с указанным логином не существует, то в сервисе генерируется RpcException со статусом NotFound.
* Если пользователь с указанным логином существует, то вычисляем хэш пароля с солью (которая указана в БД) и сравниваем его со значением хэша в БД:
* - Если хэш не совпадает, то в сервисе генерируется RpcException со статусом InvalidArgument.
* - Иначе в LoginResponse отправляем идентификатор пользователя (user_Id).  
> Фасад отправляет LoginRequest для авторизации пользователя.
