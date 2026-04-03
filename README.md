# Comments App

SPA-приложение для комментариев с поддержкой вложенных ответов, загрузки файлов и CAPTCHA.

## Демо

https://comments-app-production-dae3.up.railway.app

## Стек

Backend: ASP.NET Core 9, Entity Framework Core  
Database: SQLite  
Frontend: Angular 19  
Server: Nginx  
Containerization: Docker, docker-compose  
Hosting: Railway  
Image processing: SixLabors.ImageSharp  

## Функциональность

Комментарии:
- создание комментариев (userName, email, homePage, captcha, text)
- вложенные ответы (неограниченная глубина)
- сортировка по userName, email, createdAt (asc/desc)
- пагинация (25 на страницу)
- сортировка по умолчанию: новые сверху (LIFO)

Файлы:
- загрузка изображений (jpg, gif, png)
- автоматический ресайз до 320x240
- загрузка txt до 100KB
- просмотр изображений через lightbox

Редактор:
- кнопки форматирования: italic, bold, code, link
- предпросмотр без перезагрузки
- разрешённые HTML теги: <a>, <code>, <i>, <strong>
- проверка закрытия тегов

Безопасность:
- защита от XSS (санитизация + whitelist тегов)
- защита от SQL-инъекций (Entity Framework)
- CAPTCHA при каждой отправке
- валидация на клиенте и сервере

## Архитектура

comments-app/
  backend/
    Controllers/
      CommentsController.cs
      CaptchaController.cs
    Data/
      AppDbContext.cs
    Entities/
      Comment.cs
      User.cs
    DTOs/
      CommentCreateDto.cs
      CommentResponseDto.cs
      PagedResultDto.cs
    Services/
      CaptchaService.cs
      FileService.cs
    wwwroot/uploads/
    Program.cs
    Dockerfile

  frontend/
    src/app/
      components/
        comment-form/
        comment-list/
        comment-item/
        lightbox/
      models/
        comment.model.ts
      services/
        comment.service.ts
    Dockerfile
    nginx.conf

  docker-compose.yml

## Запуск через Docker

git clone https://github.com/Yevhenii81/Comments-app.git
cd Comments-app
docker-compose up --build

Открыть: http://localhost

Фоновый режим:
docker-compose up -d --build

Остановка:
docker-compose down

## Локальный запуск

Требования:
- .NET 9 SDK
- Node.js 20+
- Angular CLI
- dotnet-ef

Backend:
cd backend/backend
dotnet restore
dotnet ef database update
dotnet run

API: http://localhost:5000

Frontend:
cd frontend
npm install
ng serve

Frontend: http://localhost:4200

Важно:
в src/app/services/comment.service.ts указать:
http://localhost:5000/api

## База данных

Users:
- Id
- UserName
- Email
- HomePage
- IpAddress
- BrowserAgent

Comments:
- Id
- Text
- CreatedAt
- FilePath
- FileType
- UserId
- ParentId

Связи:
- один пользователь > много комментариев
- комментарий > комментарий (вложенность)

## API

GET /api/comments
POST /api/comments
GET /api/captcha

GET /api/comments параметры:
- page
- sortBy (createdAt, username, email)
- sortDesc (true/false)

POST /api/comments:
- userName
- email
- homePage (optional)
- captchaToken
- captchaAnswer
- text
- parentId (optional)
- file (optional)

## Чек-лист

- комментарии
- вложенные ответы
- сортировка
- пагинация
- CAPTCHA
- загрузка файлов
- lightbox
- редактор текста
- предпросмотр
- защита от XSS
- защита от SQL-инъекций
- валидация
- Docker
