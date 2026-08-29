# Интеграция LMS с `mephi_login`

## Что изменено

### В LMS (`/LMS`)
- добавлена страница `Pages/Login.razor` с кнопкой **«Войти через МИФИ»**;
- добавлен сервис `Services/AuthSessionService.cs` для хранения JWT в `localStorage`;
- добавлен `Services/AuthOptions.cs` с настройками:
  - `AuthServiceBaseUrl = http://localhost:8080`
  - `ProjectName = lms-dashboard`
  - `CasLoginBaseUrl = https://auth.mephi.ru/login`
- `Pages/Home.razor`:
  - принимает `?token=...` после редиректа из auth-service;
  - сохраняет JWT в `localStorage`;
  - если токена нет, перенаправляет на `/login`.
- `Layout/MainLayout.razor`:
  - отображает имя пользователя из JWT;
  - поддерживает logout.
- `wwwroot/index.html`:
  - добавлены JS-функции для работы с `localStorage`.

### В auth-service
- исправлено хеширование пароля в `Shared/Utils/Password.cs`:
  - вместо записи бинарного SHA1 как текста теперь используется `Base64`.
- в `Server/Program.cs` добавлен сид проекта `lms-dashboard` через:
  - `SeedData:LmsProject:Name`
- в `Server/Properties/appsettings.json` добавлен блок:

```json
"SeedData": {
  "LmsProject": {
    "Name": "lms-dashboard"
  }
}
```

- в `Server/Controllers/AuthController.cs` скорректирована пересборка `service`-URL для CAS в `mephi_login`, чтобы `serviceValidate` использовал ту же строку, что и на этапе редиректа в CAS.

## Как работает вход
1. Пользователь открывает `/login` в LMS.
2. Нажимает **«Войти через МИФИ»**.
3. LMS делает редирект на:
   - `https://auth.mephi.ru/login?service=...`
4. После успешной аутентификации CAS возвращает пользователя в auth-service на:
   - `/mephi_login?ticket=...&redirectUri=...&projectName=lms-dashboard`
5. `auth-service`:
   - валидирует CAS ticket;
   - при необходимости автоматически создаёт пользователя;
   - генерирует JWT для проекта `lms-dashboard`;
   - редиректит обратно в LMS:
     - `http://localhost:5293/?token=...`
6. LMS сохраняет токен в `localStorage` и открывает главную страницу.

## Что запускать
### auth-service
Используйте существующий docker-compose внутри auth-service.

### LMS
Запуск из папки `/LMS` обычным `dotnet run`.

