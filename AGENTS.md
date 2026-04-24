# Agents.md — RealEstate Platform

Единый документ проекта. Содержит архитектуру, ERD, роуты, задачи по фазам, паттерны.
Обновляется по мере продвижения.

---

## 1. Контекст проекта

**Что делаем:** веб-приложение для платформы недвижимости (продажа/аренда домов).  
**Стек:** .NET 10 LTS, ASP.NET Core MVC, EF Core 10, PostgreSQL 18, MongoDB 8, SignalR, Identity, Serilog, FluentValidation, Bootstrap (Homelengo + Upcube шаблоны).  
**Дедлайн:** 2–4 недели.  
**Цель:** 25/25 по критериям оценивания.

### Роли пользователей

- **Guest** — не авторизован. Видит главную, каталог, карточку объекта, оценщик стоимости.
- **User** — зарегистрирован. + избранное, профиль, сообщения, сравнение.
- **Agent** — размещает объявления. + мои объекты, создание/редактирование, загрузка фото/360°.
- **Admin** — управляет всем. Отдельная админ-панель (Area).

### Киллер-фичи (точки расширения)

1. **AI-оценщик стоимости** — `IValuator` со стратегиями (RegionalAverage, ComparableSales, Composite).
2. **360° виртуальные туры** — Pannellum, `IPropertyMediaRenderer`.
3. **Сравнение объектов** — до 4 штук side-by-side, `IComparator`.
4. **FeatureGate** — `IFeatureAccessPolicy` с динамическим каталогом фич. Демонстрирует сценарий "выделения функциональности".

---

## 2. Архитектура

Clean Architecture, 4 проекта. Правило зависимостей — строго в одну сторону.

```
RealEstate.sln
├── src/RealEstate.Domain/            → ZERO dependencies
├── src/RealEstate.Application/       → Domain
├── src/RealEstate.Infrastructure/    → Application
└── src/RealEstate.Web/               → Application + Infrastructure (только DI в Program.cs)
```

### Жёсткие правила

- Domain НИКОГДА не ссылается на EF Core, Identity, ASP.NET, Infrastructure.
- Application содержит ТОЛЬКО интерфейсы, DTO, use-cases, валидаторы. Без реализаций БД.
- Infrastructure реализует интерфейсы из Application. Содержит DbContext, Mongo, Identity, SMTP.
- Web использует ТОЛЬКО абстракции из Application. Контроллеры не знают про EF/Mongo напрямую.
- Ссылки на Infrastructure в Web — ТОЛЬКО в Program.cs для вызова `AddInfrastructure()`.
- Пользовательские FK в Domain-сущностях — `string UserId/AgentId`, без навигации на ApplicationUser. ApplicationUser живёт в Infrastructure.

### Areas

- `Areas/Admin` — единственная Area. Шаблон Upcube, доступ `[Authorize(Roles = "Admin")]`.
- Публичная часть + клиент + агент — в корне Controllers/. Один layout Homelengo.

---

## 3. ERD — структура данных

### PostgreSQL (основная БД)

#### Identity (стандартные таблицы, 7 шт.)

AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, AspNetUserLogins, AspNetUserTokens, AspNetRoleClaims.

`ApplicationUser : IdentityUser` расширен:
- `FirstName` (string)
- `LastName` (string)
- `AvatarPath` (string?)
- `PreferredLanguage` (string, default "ru")
- `IsBlocked` (bool)
- `IsVerified` (bool) — для агентов
- `CreatedAt` (DateTime)

#### Доменные таблицы (17 шт.)

```
Справочники:
  Cities              (Id int PK, Name, Slug)
  Districts           (Id int PK, CityId FK→Cities, Name, Slug)
  PropertyTypes       (Id int PK, Name, Slug, IconClass)
  Categories          (Id int PK, Name, Slug)
  Amenities           (Id int PK, Name, IconClass)

Ядро:
  Properties          (Id int PK, Title, Description,
                       Price_Amount decimal, Price_Currency string,   ← owned Money
                       Area_Value decimal, Area_Unit int,             ← owned Area
                       Rooms int, Floor int, TotalFloors int,
                       TransactionType int, Status int,
                       AgentId string FK→AspNetUsers,
                       Address string, Location_Latitude double?, Location_Longitude double?,
                       CityId FK→Cities, DistrictId FK→Districts,
                       PropertyTypeId FK→PropertyTypes, CategoryId FK→Categories,
                       ViewsCount long, PublishedAt DateTime?,
                       CreatedAt, UpdatedAt)
  PropertyImages      (Id int PK, PropertyId FK→Properties,
                       Path string, MediaType int [Photo|Panorama360],
                       IsPrimary bool, Order int, UploadedAt DateTime)
  PropertyAmenities   (PropertyId FK, AmenityId FK) — composite PK, M:N
  Favorites           (Id int PK, UserId string, PropertyId FK→Properties,
                       AddedAt DateTime)
                       UNIQUE(UserId, PropertyId)

Взаимодействия:
  Inquiries           (Id int PK, PropertyId FK→Properties,
                       FromUserId string, InitialMessage string,
                       Status int [Open|InProgress|Closed],
                       CreatedAt, UpdatedAt)
  Messages            (Id int PK, InquiryId FK→Inquiries,
                       SenderId string, Text string,
                       IsRead bool, SentAt DateTime, ReadAt DateTime?)

Киллер-фичи:
  Comparisons         (Id int PK, UserId string, Name string, CreatedAt, UpdatedAt)
  ComparisonItems     (ComparisonId FK, PropertyId FK, Position int) — composite PK
  Valuations          (Id int PK, UserId string?,
                       InputJson string, EstimatedMin decimal, EstimatedMax decimal,
                       Currency string, StrategyUsed string, CreatedAt, UpdatedAt)

Авторизация и модерация:
  FeatureCatalog      (Id int PK, Code string UNIQUE, DisplayName, Description,
                       IsEnabled bool, CreatedAt, UpdatedAt)
  ModerationActions   (Id int PK, PropertyId FK→Properties, ModeratorId string,
                       Action int, Reason string?, CreatedAt, UpdatedAt)
```

Итого SQL: **24 таблицы** (7 Identity + 17 доменных). Критерий "6+ нормализовано" — закрыт с запасом.  
M:N связи: PropertyAmenities, Favorites (user↔property), ComparisonItems.

### MongoDB (2 коллекции)

```
audit_logs:
  { _id, UserId, UserName, Action, EntityType, EntityId,
    PayloadJson, IpAddress, UserAgent, Timestamp }

error_logs:
  { _id, Message, StackTrace, Url, HttpMethod,
    UserId, Severity, Timestamp }
```

---

## 4. Карта роутов

### Публичные (Guest+)

| Роут | Controller.Action | Описание | HTML-база |
|---|---|---|---|
| `/` | Home.Index | Главная | homelengo/home-02.html |
| `/catalog` | Catalog.Index | Каталог с картой | homelengo/property-halfmap-grid.html |
| `/catalog/load-more` | Catalog.LoadMore (AJAX) | Подгрузка карточек | — |
| `/property/{id}` | Property.Details | Карточка объекта + 360° | homelengo/property-details-v1.html |
| `/estimator` | Estimator.Index | AI-оценщик форма | **кастомная** (Homelengo стиль) |
| `/estimator/calculate` | Estimator.Calculate (AJAX) | Расчёт оценки | — |
| `/compare` | Compare.Index | Сравнение до 4 объектов | **кастомная** |
| `/compare/toggle` | Compare.Toggle (AJAX) | Добавить/убрать из сравнения | — |

### Auth (Guest)

| Роут | Controller.Action | Описание | HTML-база |
|---|---|---|---|
| `/account/login` | Account.Login | Вход | homelengo/auth-login.html (Upcube стиль) |
| `/account/register` | Account.Register | Регистрация | homelengo/auth-register.html |
| `/account/forgot-password` | Account.ForgotPassword | Восстановление | homelengo/auth-recoverpw.html |
| `/account/confirm-email` | Account.ConfirmEmail | Подтверждение email | (редирект) |
| `/account/2fa` | Account.TwoFactor | Ввод 2FA кода | кастом |
| `POST /account/logout` | Account.Logout | Выход | — |

### User (авторизован)

| Роут | Controller.Action | Описание | HTML-база |
|---|---|---|---|
| `/account/dashboard` | Account.Dashboard | Дашборд после логина | homelengo/dashboard.html |
| `/account/profile` | Account.Profile | Профиль + настройки | homelengo/my-profile.html |
| `/account/favorites` | Account.Favorites | Избранное | homelengo/my-favorites.html |

### Agent (Role=Agent)

| Роут | Controller.Action | Описание | HTML-база |
|---|---|---|---|
| `/agent/properties` | Agent.MyProperties | Мои объявления | homelengo/my-property.html |
| `/agent/properties/create` | Agent.Create | Создание | homelengo/add-property.html |
| `/agent/properties/{id}/edit` | Agent.Edit | Редактирование | homelengo/add-property.html (та же вьюха) |
| `POST /agent/properties/{id}/delete` | Agent.Delete | Удаление | — |

### Messaging (User + Agent)

| Роут | Controller.Action | Описание | HTML-база |
|---|---|---|---|
| `/messages` | Messages.Index | Список чатов | homelengo/message.html |
| `/messages/{inquiryId}` | Messages.Thread | Конкретный чат | homelengo/message.html |
| `/hubs/chat` | **SignalR Hub** | Реалтайм чат | — |

### Admin (Areas/Admin, Role=Admin)

| Роут | Controller.Action | Описание | HTML-база |
|---|---|---|---|
| `/admin` | Admin/Dashboard.Index | Дашборд | upcube/index.html |
| `/admin/users` | Admin/Users.Index | Список пользователей | upcube/tables-datatable.html |
| `/admin/users/{id}/edit` | Admin/Users.Edit | Редактирование | upcube/form-elements.html |
| `/admin/moderation` | Admin/Moderation.Index | Модерация объявлений | upcube/tables-datatable.html |
| `POST /admin/moderation/{id}/approve` | Admin/Moderation.Approve | Одобрить | — |
| `POST /admin/moderation/{id}/reject` | Admin/Moderation.Reject | Отклонить | — |
| `/admin/dictionaries/{type}` | Admin/Dictionaries.* | Справочники CRUD | upcube/tables-basic.html + form-elements.html |
| `/admin/logs/audit` | Admin/Logs.Audit | Журнал действий (Mongo) | upcube/tables-datatable.html |
| `/admin/logs/errors` | Admin/Logs.Errors | Журнал ошибок (Mongo) | upcube/tables-datatable.html |
| `/admin/features` | Admin/FeatureCatalog.Index | Каталог фич | upcube/tables-datatable.html |

### Служебные

| Роут | Описание | HTML-база |
|---|---|---|
| `/error/404` | Страница 404 | upcube/pages-404.html |
| `/error/403` | Страница 403 (нет доступа) | upcube/pages-500.html (переделан) |

---

## 5. Ключевые абстракции (Application layer)

### IFeatureAccessPolicy — центральная точка расширения

```csharp
// Application/Abstractions/Features/
public interface IFeatureAccessPolicy
{
    string PolicyName { get; }
    Task<FeatureAccessResult> EvaluateAsync(string featureCode, ClaimsPrincipal user);
}

public record FeatureAccessResult(bool IsAllowed, string? DenyReason = null);

// Реализации в Application/UseCases/Features/:
//   RoleBasedAccessPolicy      — проверяет роль
//   OwnershipAccessPolicy      — проверяет что агент владеет ресурсом
//   QuotaAccessPolicy          — лимит N/день, N/месяц
//   VerificationAccessPolicy   — только верифицированные агенты
//   CompositeAccessPolicy      — AND/OR из любых комбинаций

// Usage in Web:
//   [FeatureGate("property.valuation")]
//   public IActionResult EstimateValue(...) { ... }
```

### IValuator — стратегии оценки стоимости

```csharp
// Application/Abstractions/Valuation/
public interface IValuator
{
    string Name { get; }
    Task<ValuationResult> EstimateAsync(ValuationRequest request);
}

public record ValuationRequest(
    int CityId, int DistrictId, int PropertyTypeId,
    decimal Area, int Rooms, int Floor, int TotalFloors);

public record ValuationResult(decimal Min, decimal Max, string Currency);

// Реализации в Application/UseCases/Valuation/:
//   RegionalAverageValuator   — средняя цена м² в районе × площадь
//   ComparableSalesValuator   — топ-5 похожих из БД
//   CompositeValuator         — ансамбль (средневзвешенное от всех)
```

### Репозитории

```csharp
// Application/Abstractions/Repositories/
public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<Property> Items, int TotalCount)> SearchAsync(
        PropertySearchFilter filter, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Property property, CancellationToken ct = default);
    Task UpdateAsync(Property property, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

// Аналогично: IFavoriteRepository, IInquiryRepository, IMessageRepository,
//   IComparisonRepository, IValuationRepository, IFeatureCatalogRepository,
//   IDictionaryRepository<T> (generic для справочников)
//   IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken ct); }

// Persistence (Mongo):
//   IAuditLogStore { Task WriteAsync(AuditLogEntry entry); Task<...> QueryAsync(...); }
//   IErrorLogStore { Task WriteAsync(ErrorLogEntry entry); Task<...> QueryAsync(...); }
```

---

## 6. Маппинг wwwroot → Views

Файлы HTML в wwwroot — референс для вёрстки. При создании View (.cshtml) берём разметку из соответствующего HTML, выносим общие части в _Layout, а динамические данные привязываем через `@Model`.

### Homelengo layout (_Layout.cshtml)

Общие элементы из home-02.html:
- `<head>` — мета, CSS-ссылки на homelengo/css/*
- Header/навигация — общий для всех публичных страниц
- Footer — общий
- JS-подключения в конце body: homelengo/js/*

Разные страницы подставляются через `@RenderBody()`.

### Upcube layout (_AdminLayout.cshtml)

Из upcube/index.html:
- Sidebar-меню (левая панель)
- Top bar (поиск, уведомления, профиль)
- CSS/JS из upcube/assets/*

Размещается в `Areas/Admin/Views/Shared/_AdminLayout.cshtml`.

---

## 7. Критерии оценивания → как закрываем

| № | Критерий | Макс | Чем закрываем |
|---|---|---|---|
| 1 | Архитектура | 5 | Clean Arch (Domain/Application/Infrastructure/Web), без нарушений |
| 2 | Дизайн и ТЗ | 2 | Homelengo адаптивный + SPA-подход через AJAX + 360° |
| 3 | AJAX-запросы | 1 | 5+: фильтры, пагинация, избранное, сравнение, оценщик, чат |
| 4 | Админ-панель | 2 | 6 страниц: Dashboard, Users, Moderation, Dictionaries, Logs, FeatureCatalog |
| 5 | Авторизация и роли | 2 | Identity + 4 роли + authorization policies + FeatureGate handlers |
| 6 | Наличие страниц | 1 | 15+ страниц, 7+ динамических |
| 7 | Фильтрация данных | 1 | Каталог: query-string фильтры, сохранение в URL, восстановление после F5 |
| 8 | Валидация форм | 1 | FluentValidation (server) + jQuery Validate Unobtrusive (client) |
| 9 | Пагинация | 1 | AJAX пагинация в каталоге + в админских таблицах с сохранением query-state |
| 10 | Структура БД | 3 | 17+ таблиц, нормализовано, M:N через join-таблицы |
| 11 | WebSockets / SignalR | 1 | Полноценный чат агент↔клиент: история, typing indicator, unread counter, live push |
| 12 | Локализация | 1 | RU + EN, .resx, CultureMiddleware, переключатель в UI |
| 13 | MongoDB | 1 | AuditLog + ErrorLog — реальное применение, не заглушка |
| 14 | Логирование | 1 | Serilog + AuditMiddleware (действия → Mongo) |
| 15 | Обработка маршрутов (404, 403) | 1 | Кастомные страницы, UseStatusCodePagesWithReExecute |
| 16 | Middleware | 1 | AuditMiddleware (полезный) + ExceptionHandlingMiddleware + CultureMiddleware |
| **Итого** | | **25** | |

---

## 8. Задачи по фазам

Статусы: `[ ]` — не начато, `[x]` — готово, `[~]` — в процессе.

### Phase 0 · Setup ~ 0.5д

- [x] Git repo + .gitignore + .editorconfig
- [x] Solution + 4 проекта + references
- [x] global.json (.NET 10)
- [x] Directory.Build.props + Directory.Packages.props (CPM)
- [x] docker-compose: postgres + mongo + mailhog
- [x] appsettings.json + appsettings.Development.json
- [x] README.md

### Phase 1 · Domain ~ 1д

- [x] Common: Entity<TId>, AuditableEntity<TId>
- [x] Enums: PropertyStatus, MediaType, TransactionType, InquiryStatus, ModerationActionType
- [x] ValueObjects: Money, GeoPoint, Area
- [x] Entities справочники: City, District, PropertyType, Category, Amenity
- [x] Entities ядро: Property (с доменными методами Submit/Approve/Reject/Archive), PropertyImage, PropertyAmenity, Favorite
- [x] Entities взаимодействия: Inquiry, Message
- [x] Entities киллер-фичи: Comparison, ComparisonItem, Valuation, FeatureCatalogEntry
- [x] Entities авторизация: ModerationAction
- [x] Entities Mongo-модели: AuditLogEntry, ErrorLogEntry
- [x] Перенос шаблонов в wwwroot/homelengo/ и wwwroot/upcube/

### Phase 2 · Application ~ 1.5д

- [ ] Abstractions/Repositories: IPropertyRepository, IFavoriteRepository, IInquiryRepository, IMessageRepository, IComparisonRepository, IValuationRepository, IFeatureCatalogRepository, IDictionaryRepository<T>, IModerationRepository
- [ ] Abstractions/Repositories: IUnitOfWork
- [ ] Abstractions/Persistence: IAuditLogStore, IErrorLogStore
- [ ] Abstractions/Features: IFeatureAccessPolicy, IFeatureAccessChecker, FeatureAccessResult
- [ ] Abstractions/Valuation: IValuator, ValuationRequest, ValuationResult
- [ ] Abstractions/Media: IImageStorage, IPropertyMediaRenderer
- [ ] Abstractions/Notifications: IEmailSender
- [ ] DTO: PropertyListItemDto, PropertyDetailsDto, PropertySearchFilter, CreatePropertyDto, UpdatePropertyDto, UserDto, InquiryDto, MessageDto, ValuationRequestDto, ComparisonDto, FeatureCatalogDto, DictionaryItemDto
- [ ] UseCases/Catalog: SearchPropertiesQuery + handler
- [ ] UseCases/Properties: CreatePropertyCommand, UpdatePropertyCommand, DeletePropertyCommand, GetPropertyDetailsQuery
- [ ] UseCases/Valuation: RegionalAverageValuator, ComparableSalesValuator, CompositeValuator
- [ ] UseCases/Comparison: ComparePropertiesQuery
- [ ] UseCases/Moderation: ApprovePropertyCommand, RejectPropertyCommand
- [ ] UseCases/Features: RoleBasedAccessPolicy, OwnershipAccessPolicy, QuotaAccessPolicy, VerificationAccessPolicy, CompositeAccessPolicy
- [ ] Validation (FluentValidation): CreatePropertyValidator, UpdatePropertyValidator, RegisterValidator, InquiryValidator
- [ ] DependencyInjection.cs: AddApplication() extension method

### Phase 3 · Infrastructure ~ 2д

- [ ] Persistence/AppDbContext.cs — все DbSet'ы
- [ ] Persistence/Configurations/ — IEntityTypeConfiguration для каждой сущности. Owned types для Money, GeoPoint, Area. Composite keys для join-таблиц. Индексы.
- [ ] Identity/ApplicationUser.cs : IdentityUser (расширенные поля)
- [ ] Первая миграция: `dotnet ef migrations add Init`
- [ ] Persistence/Repositories/ — EF-реализации всех интерфейсов
- [ ] Persistence/UnitOfWork.cs
- [ ] Persistence/Interceptors/AuditableEntityInterceptor.cs — автоматическое заполнение CreatedAt/UpdatedAt
- [ ] Mongo/MongoContext.cs
- [ ] Mongo/AuditLogStore.cs, ErrorLogStore.cs
- [ ] Services/Email/SmtpEmailSender.cs (через MailKit → Mailhog)
- [ ] Services/ImageStorage/LocalDiskImageStorage.cs
- [ ] Seeding/RoleSeeder.cs — роли Admin, Agent, User
- [ ] Seeding/AdminSeeder.cs — дефолтный admin@realestate.local / Admin123!
- [ ] Seeding/DictionarySeeder.cs — города, районы, типы, категории, удобства
- [ ] Seeding/PropertySeeder.cs — 10-15 тестовых объявлений + 2-3 equirectangular 360° фото
- [ ] DependencyInjection.cs: AddInfrastructure(IConfiguration) extension method

### Phase 4 · Web baseline ~ 2д

- [ ] Program.cs: полная конфигурация DI (AddApplication, AddInfrastructure, AddSignalR, AddLocalization, Serilog, etc.)
- [ ] Views/Shared/_Layout.cshtml — из homelengo/home-02.html (header, footer, CSS/JS)
- [ ] Areas/Admin/Views/Shared/_AdminLayout.cshtml — из upcube/index.html (sidebar, topbar, CSS/JS)
- [ ] Views/Shared/_ValidationScriptsPartial.cshtml — jQuery Validate
- [ ] Views/_ViewImports.cshtml — tag helpers, using'и
- [ ] Views/_ViewStart.cshtml
- [ ] Areas/Admin/Views/_ViewImports.cshtml
- [ ] Areas/Admin/Views/_ViewStart.cshtml (→ _AdminLayout)
- [ ] ErrorController + Error404.cshtml + Error403.cshtml
- [ ] Program.cs: UseStatusCodePagesWithReExecute("/error/{0}")
- [ ] Middleware/RequestCultureMiddleware.cs (или встроенный UseRequestLocalization)
- [ ] UI-переключатель языка RU/EN в _Layout
- [ ] Resources/SharedResource.resx + SharedResource.en.resx (базовые строки навигации)
- [ ] AccountController: Login, Register, ForgotPassword, ConfirmEmail, Logout, TwoFactor
- [ ] Соответствующие View'ы из auth-*.html

### Phase 5 · Core features ~ 3д

- [ ] HomeController.Index + View (home-02.html → cshtml)
- [ ] CatalogController.Index + View (property-halfmap-grid.html → cshtml)
- [ ] CatalogController.LoadMore (AJAX endpoint, JSON)
- [ ] Фильтры каталога: тип сделки, цена min/max, площадь, комнаты, город, район, тип объекта. Всё в query-string, восстанавливается при F5.
- [ ] PropertyController.Details + View (property-details-v1.html → cshtml)
- [ ] Кнопка "В избранное" (AJAX toggle, работает для User+)
- [ ] Кнопка "Добавить в сравнение" (AJAX, до 4 штук, хранится в session/cookie для гостей)
- [ ] Кнопка "Отправить заявку" (создаёт Inquiry)
- [ ] AgentController.MyProperties + View (my-property.html → cshtml, таблица с статусами)
- [ ] AgentController.Create GET/POST + View (add-property.html → cshtml)
- [ ] AgentController.Edit GET/POST (та же View, префилл из БД)
- [ ] AgentController.Delete POST
- [ ] Загрузка изображений: валидация mime (jpg/png/webp), ограничение размера (5MB), сохранение в wwwroot/uploads/{propertyId}/
- [ ] Client-side validation: jQuery Validate Unobtrusive + data-val атрибуты
- [ ] Server-side validation: FluentValidation + ModelState
- [ ] Account/Dashboard + View (dashboard.html → cshtml)
- [ ] Account/Profile + View (my-profile.html → cshtml): смена имени, аватара, языка
- [ ] Account/Favorites + View (my-favorites.html → cshtml): список с пагинацией

### Phase 6 · Messaging / SignalR ~ 1.5д

- [ ] Hubs/ChatHub.cs: методы SendMessage, MarkAsRead, StartTyping, StopTyping
- [ ] MessagesController.Index + View (message.html → cshtml): список Inquiries текущего пользователя
- [ ] MessagesController.Thread: загрузка истории сообщений + SignalR-подключение
- [ ] JS-клиент SignalR: отправка, приём, отображение сообщений
- [ ] Typing indicator (отображается когда собеседник печатает)
- [ ] Unread counter в шапке _Layout (live обновление через SignalR)
- [ ] Всплывающее уведомление о новом сообщении (toast)
- [ ] Создание Inquiry из карточки объекта (Property.Details) → автоматическое открытие чата

### Phase 7 · Killer features ~ 2д

#### AI-оценщик (1д)

- [ ] EstimatorController.Index + View (кастомная форма: город, район, тип, площадь, комнаты, этаж)
- [ ] EstimatorController.Calculate AJAX POST → CompositeValuator
- [ ] RegionalAverageValuator: берёт средний Price/Area по published Properties в том же районе, умножает на запрошенную площадь
- [ ] ComparableSalesValuator: находит 5 ближайших по параметрам Properties, берёт медиану
- [ ] CompositeValuator: средневзвешенное (0.4 regional + 0.6 comparable), возвращает min/max ±10%
- [ ] Сохранение результата в Valuations (для истории и квот)
- [ ] Красивый вывод результата (анимированная карточка с диапазоном цены)
- [ ] `[FeatureGate("property.valuation")]` на action — демо выделения

#### 360° туры (0.5д)

- [ ] Подключение Pannellum.js (CDN или локально в wwwroot)
- [ ] В PropertyController.Details: если есть PropertyImage с MediaType=Panorama360, рендерить Pannellum viewer
- [ ] Seed: 2-3 тестовых equirectangular фото из Wikimedia Commons (CC0/CC-BY)

#### Сравнение объектов (0.5д)

- [ ] CompareController.Index + View (кастомная таблица side-by-side)
- [ ] CompareController.Toggle AJAX (добавить/убрать, хранение в cookie/session для гостей, в БД для авторизованных)
- [ ] URL state: `/compare?ids=1,5,12,17` — восстанавливается при F5
- [ ] Параметры сравнения: цена, площадь, комнаты, этаж, район, тип, удобства, фото

### Phase 8 · Админ-панель ~ 2д

- [ ] Areas/Admin/ структура папок Controllers/ + Views/
- [ ] Admin/DashboardController.Index + View: 4 счётчика (объявления, пользователи, pending, сегодня). Данные через AJAX.
- [ ] Admin/UsersController: Index (DataTable), Edit (форма). Блокировка, разблокировка, смена роли, верификация агента.
- [ ] Admin/ModerationController: Index (pending listings), Approve POST, Reject POST (с Reason). Создание ModerationAction в БД.
- [ ] Admin/DictionariesController: CRUD для Cities, Districts, PropertyTypes, Categories, Amenities. Один контроллер с роутом `/admin/dictionaries/{type}`. Inline-editing через DataTables (AJAX).
- [ ] Admin/LogsController: Audit (из Mongo, фильтр по дате/пользователю/действию), Errors (из Mongo, фильтр по severity/дате)
- [ ] Admin/FeatureCatalogController: список фич, toggle IsEnabled (AJAX switch)

### Phase 9 · Security + middleware ~ 1д

- [ ] Middleware/AuditMiddleware.cs: логирует POST/PUT/DELETE запросы → IAuditLogStore (Mongo). Пишет UserId, Action (route), EntityType, IP, UserAgent.
- [ ] Middleware/ExceptionHandlingMiddleware.cs: try/catch → IErrorLogStore (Mongo) + красивая страница ошибки (или JSON для AJAX)
- [ ] Antiforgery: `[ValidateAntiForgeryToken]` на все POST actions. `@Html.AntiForgeryToken()` во всех формах.
- [ ] Rate limiting: `builder.Services.AddRateLimiter()` на `/account/login`, `/account/register`, `/estimator/calculate`
- [ ] Identity настройки в Program.cs:
    - `options.Password.RequiredLength = 8` + complexity
    - `options.Lockout.MaxFailedAccessAttempts = 5`
    - `options.SignIn.RequireConfirmedEmail = true`
- [ ] Смена пароля: требует ввод текущего + email confirmation
- [ ] Смена email: отправка ссылки на НОВЫЙ адрес, применение только после подтверждения
- [ ] 2FA: TOTP через Identity (встроенный `UserManager.GenerateNewTwoFactorRecoveryCodesAsync`). Или email-код — проще для MVP.
- [ ] Санитизация ввода: AntiXSS через `HtmlEncoder` в output. Input validation через FluentValidation (strip tags).
- [ ] Файлы: проверка mime + расширения, ограничение размера, rename при сохранении

### Phase 10 · Polish ~ 1д

- [ ] Response compression: `builder.Services.AddResponseCompression()` — Brotli + Gzip
- [ ] WebOptimizer: бандлинг CSS/JS, минификация
- [ ] Output caching: `[OutputCache]` на Home.Index, Catalog.Index (с vary по query)
- [ ] HTTPS redirect + HSTS
- [ ] Финальный seed: убедиться что все данные консистентны, все роли работают
- [ ] E2E проверка по каждому критерию (чеклист из секции 7)
- [ ] Красивый README с скриншотами

---

## 9. Конвенции кода

### Naming

- Контроллеры: `XxxController`
- Репозитории: `IXxxRepository` → `XxxRepository`
- Use-cases: `XxxCommand`, `XxxQuery` (без MediatR, просто сервисы)
- Валидаторы: `XxxValidator`
- View Models: `XxxViewModel`
- DTO: `XxxDto`
- Middleware: `XxxMiddleware` + extension `UseXxx()`

### Файлы

- Один класс = один файл
- Namespace = путь к папке
- Файл-scoped namespaces (`namespace X;`)

### Git

- Осмысленные коммиты: `feat:`, `fix:`, `refactor:`, `chore:`, `docs:`
- Коммит после каждой завершённой задачи (не реже 1 раза в час работы)
- Ветка `main` + feature-ветки если удобно (не обязательно для одного разработчика)

---

## 10. docker-compose.yml

```yaml
services:
  postgres:
    image: postgres:18-alpine
    ports: ["5432:5432"]
    environment:
      POSTGRES_DB: realestate
      POSTGRES_USER: realestate
      POSTGRES_PASSWORD: devpassword

  mongo:
    image: mongo:8
    ports: ["27017:27017"]
    environment:
      MONGO_INITDB_ROOT_USERNAME: realestate
      MONGO_INITDB_ROOT_PASSWORD: devpassword

  mailhog:
    image: mailhog/mailhog:latest
    ports:
      - "1025:1025"   # SMTP
      - "8025:8025"   # Web UI (http://localhost:8025)
```

### Connection strings (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=realestate;Username=realestate;Password=devpassword",
    "Mongo": "mongodb://realestate:devpassword@localhost:27017/?authSource=admin"
  }
}
```

---

## 11. Демо на защите — сценарий "выделения фичи"

Ключевой момент защиты — показать преподу что точка расширения работает.

### До:

```csharp
// Оценщик доступен всем авторизованным
[Authorize]
public IActionResult EstimateValue(...) { ... }
```

### После (1 строка правки):

```csharp
[Authorize]
[FeatureGate("property.valuation")]
public IActionResult EstimateValue(...) { ... }
```

### Что происходит:

1. `FeatureGateAuthorizationHandler` ловит атрибут
2. Читает из FeatureCatalog: фича "property.valuation" → IsEnabled? → да
3. Читает привязанные политики (из кода/DI): QuotaPolicy("3 per day, free users")
4. Проверяет: пользователь уже сделал 3 оценки сегодня? → AccessDenied с сообщением
5. Админ в FeatureCatalog может выключить фичу целиком toggle'ом → доступ закрыт для всех
6. Новая фича = новая строка в FeatureCatalog + атрибут на action. Код сервиса/контроллера не меняется.

**Это буквально то что просят в общих требованиях курса:** "выделить функциональность в отдельную услугу с минимальным изменением кода".