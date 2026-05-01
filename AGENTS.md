# Agents.md — UpperCube Real Estate Platform

Единый документ проекта. Содержит архитектуру, ERD, роуты, задачи по фазам, паттерны.
Обновляется по мере продвижения.

---

## 1. Контекст проекта

**Что делаем:** веб-приложение для платформы недвижимости (продажа/аренда домов).
**Solution:** `UpperCube.sln`
**Стек:** .NET 10 LTS, ASP.NET Core MVC, EF Core 10, PostgreSQL 18, MongoDB 8, SignalR, Identity, Serilog, FluentValidation, Bootstrap (Homelengo + Upcube шаблоны).
**Дедлайн:** 2–4 недели.
**Цель:** 25/25 по критериям оценивания.

### Роли пользователей

- **Guest** — не авторизован. Видит главную, каталог, карточку объекта, оценщик стоимости.
- **User** — зарегистрирован. + избранное, профиль, сообщения, сравнение.
- **Agent** — размещает объявления. + мои объекты, создание/редактирование, загрузка фото/360°.
- **Admin** — управляет всем. Отдельная админ-панель (Area).

### Тестовые учётки (после сидинга)

- `admin@uppercube.local` / `Admin123!` — Admin
- `agent@uppercube.local` / `Agent123!` — Agent (IsVerified=true)
- `user@uppercube.local` / `User1234!` — User

### Киллер-фичи (точки расширения)

1. **AI-оценщик стоимости** — `IValuator` со стратегиями (RegionalAverage, ComparableSales, Composite). Реализации готовы.
2. **360° виртуальные туры** — Pannellum, `IPropertyMediaRenderer`. Не интегрировано во view (Phase 7).
3. **Сравнение объектов** — до 4 штук side-by-side (Phase 7).
4. **FeatureGate** — `IFeatureAccessPolicy` с динамическим каталогом фич. Базовые политики готовы.

---

## 2. Архитектура

Clean Architecture, 4 проекта. Правило зависимостей — строго в одну сторону.

```
UpperCube.sln
├── src/UpperCube.Domain/            → ZERO dependencies
├── src/UpperCube.Application/       → Domain
├── src/UpperCube.Infrastructure/    → Application
└── src/UpperCube.Web/               → Application + Infrastructure (только DI в Program.cs)
```

### Жёсткие правила

- Domain НИКОГДА не ссылается на EF Core, Identity, ASP.NET, Infrastructure.
- Application содержит ТОЛЬКО интерфейсы, DTO, use-cases, валидаторы. Без реализаций БД.
- Infrastructure реализует интерфейсы из Application. Содержит DbContext, Mongo, Identity, SMTP.
- Web использует ТОЛЬКО абстракции из Application. Контроллеры не знают про EF/Mongo напрямую.
- Ссылки на Infrastructure в Web — ТОЛЬКО в Program.cs для вызова `AddInfrastructure()`.
- Пользовательские FK в Domain-сущностях — `string UserId/AgentId`, без навигации на ApplicationUser.

### Areas

- `Areas/Admin` — единственная Area. Шаблон Upcube, доступ `[Authorize(Roles = "Admin")]`.
- Публичная часть + клиент + агент — в корне Controllers/. Один layout Homelengo.

---

## 3. ERD — структура данных

### PostgreSQL (основная БД)

#### Identity (стандартные таблицы, 7 шт.)

`ApplicationUser : IdentityUser` расширен:
- `FirstName`, `LastName` (string)
- `AvatarPath` (string?)
- `PreferredLanguage` (string, default "ru")
- `IsBlocked` (bool)
- `IsVerified` (bool) — для агентов
- `CreatedAt` (DateTime)

#### Доменные таблицы (17 шт.)

```
Справочники:
  Cities              (Id, Name, Slug)
  Districts           (Id, CityId FK, Name, Slug)
  PropertyTypes       (Id, Name, Slug, IconClass)
  Categories          (Id, Name, Slug)
  Amenities           (Id, Name, IconClass)

Ядро:
  Properties          (Id, Title, Description,
                       Price_Amount, Price_Currency,    ← owned Money
                       Area_Value, Area_Unit,           ← owned Area
                       Rooms, Floor, TotalFloors,
                       TransactionType int, Status int,
                       AgentId string FK→AspNetUsers,
                       Address, Location_Latitude?, Location_Longitude?,
                       CityId, DistrictId, PropertyTypeId, CategoryId,
                       ViewsCount long, PublishedAt?,
                       CreatedAt, UpdatedAt)
  PropertyImages      (Id, PropertyId, Path, MediaType [Photo|Panorama360],
                       IsPrimary, Order, UploadedAt)
  PropertyAmenities   (PropertyId, AmenityId) — composite PK, M:N
  Favorites           (Id, UserId, PropertyId, AddedAt) UNIQUE(UserId, PropertyId)

Взаимодействия:
  Inquiries           (Id, PropertyId, FromUserId, InitialMessage,
                       Status [Open|InProgress|Closed], CreatedAt, UpdatedAt)
  Messages            (Id, InquiryId, SenderId, Text,
                       IsRead, SentAt, ReadAt?)

Киллер-фичи:
  Comparisons         (Id, UserId, Name, CreatedAt, UpdatedAt)
  ComparisonItems     (ComparisonId, PropertyId, Position) — composite PK
  Valuations          (Id, UserId?, InputJson, EstimatedMin, EstimatedMax,
                       Currency, StrategyUsed, CreatedAt, UpdatedAt)

Авторизация и модерация:
  FeatureCatalog      (Id, Code UNIQUE, DisplayName, Description, IsEnabled,
                       CreatedAt, UpdatedAt)
  ModerationActions   (Id, PropertyId, ModeratorId, Action, Reason?,
                       CreatedAt, UpdatedAt)
```

Итого SQL: **24 таблицы** (7 Identity + 17 доменных). Критерий "6+ нормализовано" — закрыт с запасом.
M:N связи: PropertyAmenities, Favorites, ComparisonItems.

### MongoDB (2 коллекции)

```
audit_logs:    { _id, UserId, UserName, Action, EntityType, EntityId,
                 PayloadJson, IpAddress, UserAgent, Timestamp }
error_logs:    { _id, Message, StackTrace, Url, HttpMethod,
                 UserId, Severity, Timestamp }
```

---

## 4. Текущее состояние DTO

### PropertyListItemDto (карточки)

```csharp
public sealed record PropertyListItemDto(
    int Id, string Title,
    decimal Price, string Currency,
    decimal Area, int Rooms,
    string City, string District, string? PrimaryImagePath,
    int Floor, string PropertyType, int TotalFloors,
    int TransactionType, string Address);
```

### PropertyDetailsDto (страница объекта)

```csharp
public sealed record PropertyDetailsDto(
    int Id, string Title, string Description,
    decimal Price, string Currency,
    decimal Area, int Rooms, int Floor, int TotalFloors,
    int Status, int TransactionType,
    string City, string District, string PropertyType, string Category,
    string Address,
    string AgentId, string AgentName,
    long ViewsCount,
    List<string> Images, List<string> Amenities);
```

### CatalogModelView (Web/Models/Catalog/)

```csharp
public class CatalogModelView
{
    public IEnumerable<PropertyListItemDto> Items { get; set; }
    public int? CityId, PropertyTypeId, TransactionType, Rooms;
    public decimal? MinPrice, MaxPrice;
    public int Page, TotalCount;
    public IEnumerable<SelectListItem>? Cities, PropertyTypes;
}
```

---

## 5. Карта роутов

### Публичные

| Роут | Controller.Action | Статус |
|---|---|---|
| `/` | Home.Index | ✅ 6 свежих |
| `/Catalog` | Catalog.Index | ✅ фильтры в URL |
| `/Catalog/LoadMore` | Catalog.LoadMore (AJAX) | ⏳ |
| `/Property/Details/{id}` | Property.Details | ✅ |
| `/Estimator` | заглушка | ⏳ Phase 7 |
| `/Compare` | заглушка | ⏳ Phase 7 |

### Auth

| Роут | Статус |
|---|---|
| `/Account/Login`, `/Register`, `/ForgotPassword`, `/TwoFactor`, `/Logout` | ✅ |
| `/Culture/Set` (RU/EN cookie) | ✅ |

### User (авторизован) — ⏳ Phase 5

- `/Account/Dashboard`, `/Profile`, `/Favorites`
- `POST /api/favorites/toggle` (AJAX)

### Agent — ⏳ Phase 5

- `/agent/properties`, `/create`, `/{id}/edit`, `POST /{id}/delete`

### Messaging (Phase 6), Admin (Phase 8) — ⏳

### Служебные ✅

- `/error/404`, `/error/403`, `/error/500`

---

## 6. Ключевые абстракции

### IFeatureAccessPolicy

```csharp
public interface IFeatureAccessPolicy
{
    string PolicyName { get; }
    Task<FeatureAccessResult> EvaluateAsync(string featureCode, ClaimsPrincipal user);
}
public record FeatureAccessResult(bool IsAllowed, string? DenyReason = null);
```

Реализовано: `RoleBasedAccessPolicy`, `VerificationAccessPolicy`, `FeatureAccessChecker`.
Phase 7: `OwnershipAccessPolicy`, `QuotaAccessPolicy`, `CompositeAccessPolicy`.

### IValuator

```csharp
public interface IValuator
{
    string Name { get; }
    Task<ValuationResult> EstimateAsync(ValuationRequest request);
}
```

Реализовано: `RegionalAverageValuator`, `ComparableSalesValuator`, `CompositeValuator` (ансамбль 0.4 + 0.6).

### IPropertyRepository

```csharp
public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<Property> Items, int TotalCount)> SearchAsync(
        PropertySearchFilter filter, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Property>> GetLatestPublishedAsync(int count, CancellationToken ct = default);
    Task IncrementViewsAsync(int id, CancellationToken ct = default);
    Task AddAsync(Property property, CancellationToken ct = default);
    Task UpdateAsync(Property property, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
```

Прочие: `IFavoriteRepository`, `IInquiryRepository`, `IMessageRepository`, `IComparisonRepository`, `IValuationRepository`, `IFeatureCatalogRepository`, `IDictionaryRepository<T> where T : Entity<int>`, `IModerationRepository`, `IUnitOfWork`.

Mongo: `IAuditLogStore`, `IErrorLogStore`.

---

## 7. Mapping слой (Web/Mapping/)

`PropertyMapping.cs` — extension methods:
- `ToListItemDto()` ✅
- `ToDetailsDto()` ✅

Маппер живёт в Web (не в Application) — сознательное решение, потому что использует упрощения для view.

---

## 8. wwwroot и шаблоны

### Homelengo layout ✅
- `Views/Shared/_Layout.cshtml` — header/footer/nav из home-02.html
- `Views/Shared/_PropertyCard.cshtml` — partial для карточки
- `Views/Shared/_AuthLayout.cshtml` — отдельный layout для auth
- JS: jQuery → Bootstrap → плагины темы (порядок важен!)

### Upcube layout 🟡
- `Areas/Admin/Views/Shared/_AdminLayout.cshtml` — структура есть, контента нет

---

## 9. Критерии оценивания → статус

| № | Критерий | Макс | Статус |
|---|---|---|---|
| 1 | Архитектура | 5 | ✅ Clean Arch без нарушений |
| 2 | Дизайн и ТЗ | 2 | 🟡 нужно 360° + докрутить шапку |
| 3 | AJAX | 1 | 🟡 фильтры в URL, нужны load-more + favorites |
| 4 | Админка 6+ страниц | 2 | ⏳ Phase 8 |
| 5 | Авторизация и роли | 2 | 🟡 Identity ✅, политики Phase 9 |
| 6 | Страницы 6+ | 1 | 🟡 7 готовых страниц |
| 7 | Фильтрация в URL | 1 | ✅ |
| 8 | Валидация форм | 1 | ⏳ Phase 5 |
| 9 | Пагинация AJAX | 1 | ⏳ Phase 5 |
| 10 | БД 6+ таблиц | 3 | ✅ 24 таблицы |
| 11 | SignalR полноценный | 1 | ⏳ Phase 6 |
| 12 | Локализация 2+ языка | 1 | ✅ |
| 13 | MongoDB | 1 | 🟡 stores готовы, middleware Phase 9 |
| 14 | Логирование | 1 | 🟡 Serilog ✅ |
| 15 | 404/403 | 1 | ✅ |
| 16 | Middleware полезный | 1 | ⏳ Phase 9 |

---

## 10. Прогресс по фазам

### Phase 0 · Setup ✅
### Phase 1 · Domain ✅
### Phase 2 · Application ✅
### Phase 3 · Infrastructure ✅
### Phase 4 · Web baseline ✅

### Phase 5 · Core features 🟡

**Готово:**
- [x] PropertyListItemDto и PropertyDetailsDto расширены
- [x] PropertyMapping (ToListItemDto, ToDetailsDto)
- [x] HomeController с 6 свежими + view
- [x] _PropertyCard.cshtml partial
- [x] CatalogController с фильтрами в URL (критерий #7)
- [x] Catalog/Index.cshtml с формой фильтров
- [x] PropertyController.Details + view
- [x] IncrementViewsAsync
- [x] GetByIdAsync с полными Include + ThenInclude

**Осталось:**
- [ ] CatalogController.LoadMore (AJAX) + кнопка "Показать ещё"
- [ ] FavoritesApiController POST /api/favorites/toggle
- [ ] JS-обработчик клика на сердечке
- [ ] Account.Favorites + view
- [ ] AgentController: MyProperties / Create / Edit / Delete
- [ ] Загрузка фото через IImageStorage
- [ ] Client-side validation (jQuery Validate Unobtrusive)
- [ ] Account.Dashboard + view
- [ ] Account.Profile + view (read-only)
- [ ] Косметика на Property/Details: галерея, кнопки "В избранное"/"Связаться"
- [ ] @Html.AntiForgeryToken() для AJAX

### Phase 6 · Messaging / SignalR ⏳
### Phase 7 · Killer features ⏳
### Phase 8 · Админ-панель ⏳
### Phase 9 · Security + middleware ⏳
### Phase 10 · Polish ⏳

---

## 11. Конвенции кода

### Naming
- Контроллеры: `XxxController`
- Репозитории: `IXxxRepository` → `XxxRepository`
- ViewModel: исторически в проекте используется `XxxModelView` (например `CatalogModelView`)
- DTO: `XxxDto`

### DI
- Primary constructors: `class HomeController(IPropertyRepository repo) : Controller`
- Инжектим интерфейсы, не классы

### Файлы
- Один класс = один файл (исключение: близкие интерфейсы могут быть рядом)
- File-scoped namespaces

### Git
- Коммиты: `feat:`, `fix:`, `refactor:`, `chore:`, `docs:`
- Коммит после каждой завершённой задачи

---

## 12. docker-compose.yml

```yaml
services:
  postgres:
    image: postgres:18-alpine
    ports: ["5433:5432"]   # хост 5433, контейнер 5432
    environment:
      POSTGRES_DB: uppercube
      POSTGRES_USER: uppercube
      POSTGRES_PASSWORD: devpassword

  mongo:
    image: mongo:8
    ports: ["27017:27017"]
    environment:
      MONGO_INITDB_ROOT_USERNAME: uppercube
      MONGO_INITDB_ROOT_PASSWORD: devpassword

  mailhog:
    image: mailhog/mailhog:latest
    ports:
      - "1025:1025"
      - "8025:8025"
```

**Порт Postgres на хосте — 5433** (не дефолтный 5432). В connection string учти это.

---

## 13. Демо на защите — сценарий "выделения фичи"

### До:
```csharp
[Authorize]
public IActionResult EstimateValue(...) { ... }
```

### После:
```csharp
[Authorize]
[FeatureGate("property.valuation")]
public IActionResult EstimateValue(...) { ... }
```

### Что происходит:
1. Атрибут перехватывается handler'ом
2. Handler читает FeatureCatalog (IsEnabled?)
3. Через `IFeatureAccessChecker` запускаются все политики (Role + Verification + Quota)
4. Любой deny → доступ закрыт
5. Админ может выключить фичу toggle'ом в FeatureCatalog
6. Новая фича = строка в каталоге + атрибут на action. Код не меняется.

**Это и есть "выделение функциональности с минимальным изменением кода" из требований курса.**

---

## 14. При переходе в новый чат

В первом сообщении напиши:

> Учусь лайвкодить. Давай задачи по одной, направляй меня вопросами, не пиши код за меня. При ошибках указывай на проблему, не сразу решение. Архив проекта прикладываю.

Без этого Claude скатится в "вот тебе готовый код" — что мешает учёбе.