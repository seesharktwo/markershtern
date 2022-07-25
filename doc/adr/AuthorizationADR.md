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
    
> поля user_Id и login уникальны
### Proto: 
    service AuthService{
        rpc Login(LoginRequest) returns(LoginResponse);
        rpc UserExistenceCheck(UserExistenceCheckRequest) returns(UserExistenceCheckResponse);
        rpc TokenValidate(TokenValidateRequst) returns(TokenValidateResponse);
    }

    message LoginRequest {
        string login = 1;
        string password = 2;
    }

    message LoginResponse {
        oneof result{
            string error = 1;
            string token = 2;
        }
    }
    
При получении LoginRequest микросервис сравнивает вложенные в него данные с теми, что хранятся в БД.
* Если пользователя с указанным логином не существует, то в LoginResponse отправляем соответствующую ошибку.
* Если пользователь с указанным логином существует, то вычисляем хэш пароля с солью (которая указана в БД) и сравниваем его со значением хэша в БД:
* - Если хэш не совпадает, то в LoginResponse отправляем соответствующую ошибку.
* - Иначе в LoginResponse отправляем сгенерированный временный токен (JWT).  
> Фасад отправляет LoginRequest для авторизации пользователя. 

    message UserExistenceCheckRequest {
        string login = 1;
    }
    
    message UserExistenceCheckResponse {
        oneof result{
            string error = 1;
            string login = 2;
        }
    }
    
При получении UserExistenceCheckRequest микросевис сравнивает вложенные в него данные с теми, что хранятся в БД.
* Если пользователя с указанным логином существует, то в UserExistenceCheckResponse отправляем соответствующую ошибку.
* Иначе в UserExistenceCheckResponse обратно отправляем login пользователя.
> Фасад отправляет RegistrationCheckRequest перед отправкой события на регистрацию, чтобы исключить возможность повторных логинов.   
> 
> Фасад отправляет RegistrationCheckRequest с какой-нибудь периодичностью после отправки события на регистрацию, чтобы понять зарегистрирован ли пользователь.

    message TokenValidateRequest {
        string token = 1;
    }

    message TokenValidateResponse {
        oneof result{
            string error = 1;
            string userId = 2;
        }
    } 

При получении TokenValidateRequest микросервис производит валидацию токена.
* Если токен корректен, то в TokenValidateResponse отправляется userId.
* Иначе в TokenValidateResponse отправляется соответствующая ошибка.
> Фасад отправляет TokenValidateRequest для проверки токена пользователя. 
> 
> Подразумевается, что после захода в систему пользователю выдается токен, который будет отправляться вместе с каким-либо запросом пользователя.  
Таким образом, когда фасад получает запрос на что-либо от пользователя, он отправляет микросервису авторизации запрос с проверкой токена.  
> * Если токен корректен, то фасад отправляет запрос пользователя нужному микросервису, используя полученный в ответе user_Id.
> * Иначе запрос пользователя отклоняется, а самого пользователя переносит на экран входа в систему.

    message UserRegisterRequstedEvent {
        string login = 1;
        string password = 2;
    } 

Микросервис подписан на событие UserRegisterRequstedEvent в кафке, при его получении указанные данные пользователя заносятся в БД.  
Вместо пароля в БД добавляется его хэш с применением соли.  
После занесения информации в БД микросервис отправляет в определенный топик кафки событие UserRegisteredEvent, в котором передаётся userId.


