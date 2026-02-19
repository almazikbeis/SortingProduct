# SortingProduct

Небольшой бэкенд для тестового: товары загружаются из Excel и раскладываются по группам так, чтобы в каждой группе сумма была **не больше 200 евро** и по возможности **поближе к 200**.

## Живая демка (Railway)

- API: `https://sortingproduct-production.up.railway.app`
- Swagger: `https://sortingproduct-production.up.railway.app/swagger`
- Health-check: `https://sortingproduct-production.up.railway.app/health`

## Что сделано по проекту (коротко)

- Код разложен по слоям в стиле **Clean Architecture (лайт-версия в одном проекте)**:
  - `Domain` — сущности (`ProductBatch`, `ProductGroup`, ...)
  - `Application` — сервисы/логика (`ProductImportService`, `ProductGroupingService`) + интерфейсы репозиториев
  - `Infrastructure` — EF Core (`AppDbContext`, репозитории) + импорт Excel (ClosedXML)
  - `Controllers` — API endpoints
  - `Api/BackgroundJobs` — фоновая задача (раз в 5 минут)

- EF Core миграции **применяются при старте приложения** (`Database.Migrate()` в `Program.cs`).
  Это сделано специально, чтобы на хостинге типа Railway не приходилось руками запускать `dotnet ef database update`.
  (Есть небольшой retry, потому что БД может подняться чуть позже контейнера.)

## Что делает приложение

1) Принимается `.xlsx` файл ? строки сохраняются в БД как партии товаров (`product_batches`)
2) Сервис собирает группы до 200 евро (`product_groups` + `product_group_items`)
   - есть фоновая задача, которая запускается каждые 5 минут
   - дополнительно сделан ручной запуск, чтобы не ждать
3) Есть эндпоинты для просмотра списка групп и товаров внутри каждой группы

## Как протестировать (самый простой сценарий)

Тестирование удобно делать через Swagger.

### 1) Загрузка Excel
`POST /api/import/xlsx`
- тип: `multipart/form-data`
- поле: `File` (файл `.xlsx`)

Ответ примерно такой:
```json
{ "importedCount": 4 }
```

### 2) (Опционально) Проверка загруженных партий и остатков
`GET /api/admin/batches`

Здесь видно партии + остатки:
- `InitialQuantity` — сколько пришло из Excel
- `RemainingQuantity` — сколько ещё не распределено по группам
- `Status` — New/Processing/Processed

### 3) Сборка групп
Есть два варианта:

**Вариант A:** дождаться фоновой задачи (раз в 5 минут)

**Вариант B (быстро):** ручной запуск
`POST /api/admin/grouping/run`

Ответ:
```json
{ "createdGroups": 3 }
```

### 4) Просмотр списка групп
`GET /api/groups`

Ответ примерно такой:
```json
[
  {
    "id": "...",
    "name": "Group 1",
    "totalPriceEur": 199.8,
    "createdAt": "..."
  }
]
```

### 5) Просмотр товаров внутри группы
`GET /api/groups/{groupId}/items`

`groupId` берётся из ответа `GET /api/groups`.

Поля:
- `unitPriceEur` — цена за 1 штуку
- `quantity` — сколько штук положено в эту группу
- `lineTotalEur` — `unitPriceEur * quantity` (сумма строки)

## Про алгоритм группировки (очень коротко)

Группа собирается до 200 евро по простой эвристике:
- пока есть место — выбирается самый дорогой товар, который ещё помещается в остаток
- если количество большое — оно может быть разделено между несколькими группами

Это не “идеальная математика” (не knapsack), но обычно суммы получаются близко к 200.

## Локальный запуск (если нужно)

1) Требуется поднять PostgreSQL и создать базу `sorting_product`
2) Требуется указать строку подключения в `SortingProduct/appsettings.Development.json`
3) Требуется применить миграции:

```bash
dotnet ef database update --project SortingProduct/SortingProduct.csproj --startup-project SortingProduct/SortingProduct.csproj --context SortingProduct.Infrastructure.Persistence.AppDbContext
```

4) Далее запускается проект и открывается Swagger (локальный URL будет в консоли/Visual Studio).
