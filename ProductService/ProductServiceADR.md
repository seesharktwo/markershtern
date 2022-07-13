ProductServices ("Просмотр общего списка товаров, доступных для торговли")

adr by team#2

#Статус:
	Предложено

# Проблема:
	 Мироксервис с товарами:

		protos:

		сервис товара
		сообщения
		бд товаров в mongodb
		обмен событиями по kafka (crud)

	Просмотр списка товаров, доступных для торговли


# Решение:

	(Микросервис работает со списком уникальных товаров - регистрирует нвоый, выводит товар или список товаров)
### Микросервис с товарами:

		PROTO 

			service ProductService
			{
				/// метод для регистрации нового уникального продукта, 
				/// предпологается что ответ будет получаться микросервисом портфеля -
				/// для добавления товара в портфель юзера, тот в свою очередь присвоит заданный объем (и цену?)
				rpc RegisterNewProduct(NewProductRequest) returns NewProductResponce
				
				/// метод для взятия товара из списка по id товара
				rpc GetProduct(ProductRequest) returns ProductResponce

				rpc GetAllProducts(StreamProductsRequest) returns stream ProductResponce

			}



			message NewProductRequest
			{
				string Name = 1;
			}

			message NewProductResponce
			{
				bool Accepted = 1;

				string Id = 2;

				string Name = 3;
			}

			message ProductRequest
			{
				string Id = 1;
			}

			message ProductResponce
			{
				bool isFound = 1;

				string Id = 2;

				string Name = 3;
			}


		модель ТОВАРА есть следующая:
		{
			[BSON.ObjectID]
			string Id

			[unique]
			string Name

		}

		БД товаров с mongodb
			один список документов с товарами

		СЕРВИС ВЗАИМОДЕЙСТВИЯ С БД товаров
			// будет проверять имя нового товара на null/неуникальность, в соответствии с результатом проверки генерировать ответ
			метод регистрации нового товара в бд

			метод взятия уникального товара из бд по id

			метод взятия списка всех уникальных товаров
			

		ОБМЕН СОБЫТИЯМИ По kafka (CRUD)
			используя proto конвертировать ответы из c#-objs в сообщения и отправлять в шину 

### Просмотр списка товаров, доступных для торговли
		Проблема будет решаться с помощью микросервиса заявок
		микросервис должен содержать в своем интерфейсе метод на получение списка всех товаров с bid and ask

			service OrderService
			{
				/// какие-либо другие методы, которые определяются в задаче самого этого микросервиса
				....

				/// метод для запроса списка продуктов для торговли
				/// метод находит bid and ask для каждого товара, на который существуют заявки и формирует ответ
				grpc GetProductsWithBidAndAsk(StreamProductsWithBidAndAskRequest) returns stream ProductWithBindAndAskResponce
			}

			message StreamProductsWithBidAndAskRequest
			{
				??? нет вариантов для добавления сюда каких-либо полей
			}

			message ProductWithBidAndAskResponce
			{
				string Id = 1;

				string Name = 2;

				float Bid = 3;

				float Ask = 4;

			}
			
### Метод в фасаде на получение списка через кафку
	Проблема решается с помощью микросервиса фасада,
	тот должен содержать в своем интерфейсе метод на отправку ему данных

		service FacadeService
		{
			/// какие-либо другие методы, которые определяются в задаче самого этого микросервиса
			....

			/// метод для отправки ему списка продуктов для торговли
			/// метод находит bid and ask для каждого товара, на который существуют заявки и формирует ответ
			grpc SendProductsWithBidAndAsk(StreamProductsWithBidAndAskRequest) returns stream ProductWithBindAndAskResponce
		}