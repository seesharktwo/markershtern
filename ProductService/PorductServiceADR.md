ProductServices ("")

adr by team#2

Проблема:
Мироксервис с портфелем:
	модель товара
	модель событий товара
	бд товаров в mongodb
	сервис взаимодействия с бд товаров
	crud-api с товарами
	обмен событиями по kafka

Решение:

	МОДЕЛЬ ТОВАРА.
	модель товара есть следующая:
	{
		[id]
		string Id

		string OwnerID

		[unique]
		string Name

		int Price

		int Quanity
	}


	МОДЕЛЬ СОБЫТИЙ.
	модель события изменения товара: // можно разбить на несколько событий?
	{
		string ProductId
		
		string OldOwnerID

		string NewOwnerID

		int OldQuanity

		int newQuanity
	}
	модель события добавления товара:
	{
		string ProductId

		string OwnerID

		string ProductName

		int ProductPrice

		int ProductQuanity
	}


	БД ТОВАРОВ с mongodb
	??назначить класс подобие контексту, но для mongodb, где будут определены crud операции


	СЕРВИС ВЗАИМОДЕЙСТВИЯ С БД ТОВАРОВ
	Здесь будет находиться реализация для crud-операций из контекста mongodb
	Также в методах crud будет находиться проверка валидности входных параметров

	crud-api с товарами