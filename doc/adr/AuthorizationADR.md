# Context: 
Микросервис отвечающий за аутентификацию и авторизацию пользователя
# Status
Предложено
# Decision
### Данный в БД хранятся следующим образом: 
    string user_Id
    string login
    string hash
    string salt
    
> поля user_Id и login уникальны, salt вычисляется для каждого пользователя отдельно.  

> на данном этапе в базе данных храниться какое-то количество тестовых профилей.
### Proto: 

    service AuthService{
        rpc Login(LoginRequest) returns(LoginResponse);
    }

    message LoginRequest {
        string login = 1;
        string password = 2;
    }

    message LoginResponse {
        oneof result{
            Errors error = 1;
            string user_Id = 2;
        }
    }
    
    enum Errors {
	    LOGIN_NOT_FOUND = 0;
	    WRONG_PASSWORD = 1;
    }
    
При получении LoginRequest микросервис сравнивает вложенные в него данные с теми, что хранятся в БД.
* Если пользователя с указанным логином не существует, то в LoginResponse отправляем соответствующую ошибку.
* Если пользователь с указанным логином существует, то вычисляем хэш пароля с солью (которая указана в БД) и сравниваем его со значением хэша в БД:
* - Если хэш не совпадает, то в LoginResponse отправляем соответствующую ошибку.
* - Иначе в LoginResponse отправляем идентификатор пользователя (user_Id).  
> Фасад отправляет LoginRequest для авторизации пользователя.
