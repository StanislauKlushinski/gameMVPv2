# Архитектурный скелет MVP 0.1

> Документ описывает классы, сервисы и контроллеры, необходимые для реализации MVP 0.1.
> Код не приводится — только поля и функциональное описание.
> Это ориентир для пошагового написания игры.

---

## Связь с Game Design

Для уточнения деталей механик ориентируйся на документы из папки `docs/game-design`.

Важно:
- `docs/architecture/architecture-skeleton.md` фиксирует архитектуру именно для `MVP 0.1`
- папка `docs/game-design` описывает целевое, более полное видение игры
- если в `game-design` механика описана глубже или сложнее, для MVP допустима упрощённая реализация
- при конфликте объёма ориентироваться в первую очередь на `docs/game-design/MVP 0.1.md`, а остальные документы использовать как источник деталей и направления развития

### Куда смотреть по механикам

- Общие рамки MVP: `docs/game-design/MVP 0.1.md`
- Общая структура дизайн-доков: `docs/game-design/README.md`
- Полное сводное описание всех систем: `docs/game-design/_compendium.md`
- Общая концепция и high-level видение: `docs/game-design/00. Концепт.md`
- Игровой цикл и порядок расчётов: `docs/game-design/01. Игровой цикл (core loop).md`
- Ресурсы, лимиты и уровни ресурсов: `docs/game-design/02. Ресурсы (типы и уровни).md`
- Потоки ресурсов, очередь потребления и bottleneck: `docs/game-design/03. Потоки ресурсов (производство - склад - потребление).md`
- Типы зданий и их базовые параметры: `docs/game-design/04. Здания (типы и функции).md`
- Таймеры, циклы и апгрейды зданий: `docs/game-design/05. Поведение зданий (таймеры, апгрейды).md`
- Рабочие как модификаторы эффективности: `docs/game-design/06. Рабочие (модификаторы эффективности).md`
- Логистика и ограничения передачи: `docs/game-design/07. Логистика (автоматическое распределение).md`
- Технологии и разблокировки: `docs/game-design/08. Технологии (дерево развития).md`
- Прогрессия и масштабирование: `docs/game-design/09. Прогрессия (разблокировки и масштабирование).md`
- Престиж как мета-прогресс: `docs/game-design/10. Престиж (мета-прогресс).md`
- Оффлайн прогресс как будущее расширение: `docs/game-design/11. Оффлайн прогресс (AFK расчёт).md`
- Глобальные улучшения как будущее расширение: `docs/game-design/12. Глобальные улучшения (множители).md`
- Финальная цель полной игры: `docs/game-design/13. Финальная цель (ракета завершение цикла).md`
- Эпохи как параллельные слои: `docs/game-design/14. Эпохи (параллельные слои).md`
- Кросс-эпохные связи и бонусы: `docs/game-design/15. Связь эпох (кросс-эффекты).md`

### Правило упрощения для MVP

Если полная версия механики из `docs/game-design` слишком сложна для `MVP 0.1`, то в архитектуре и реализации разрешено:
- сокращать количество состояний и параметров
- убирать вторичные системы и редкие edge cases
- заменять универсальные решения на более прямые и жёстко заданные
- оставлять расширяемые точки в коде, чтобы позже приблизиться к полному дизайну без переписывания всей основы

---

## Правило использования числовых типов

В idle-игре числа могут достигать значений `10^1000` и выше. Использование `float` или `double` напрямую для игровых величин недопустимо — они ограничены `~3.4e38` и `~1.7e308` соответственно.

### `GameNumber` — для всех игровых величин

Используй `GameNumber` везде, где число связано с экономикой игры:

- количество ресурсов (`amounts`)
- лимиты хранилищ (`storageLimits`)
- скорости производства и передачи (`rates`)
- стоимости строительства и апгрейдов (`costs`)
- множители и бонусы (результат вычислений)
- рабочие циклы эпох (`workCycles`)
- очки престижа (`prestigePoints`)
- глобальные множители (`globalMultipliers`)

### `float` — только для времени и Unity

Используй `float` только там, где число означает реальное время или является Unity-специфичным:

- `tickRate`, `cycleTime`, `cycleTimer`, `cycleProgress` — время в секундах
- `deltaTime` — Unity Time.deltaTime
- `offlineSeconds`, `offlineEfficiencyFactor`, `maxOfflineHours`
- `upgradeCostFactor`, `productionGrowthFactor`, `workerEffectFactor` — базовые конфигурационные коэффициенты (задаются дизайнером вручную, малые числа типа `1.5`, `0.15`; никогда не вырастут до больших значений)
- прогресс-бары и UI-анимации (`cycleProgress: float` от 0.0 до 1.0)

> **Правило:** если ты сомневаешься — используй `GameNumber`. Лучше избыточная точность, чем числовой overflow через 10 апгрейдов.

### MVP-упрощение для GameNumber

На старте внутри `GameNumber` можно хранить просто `double`. Когда приблизишься к пределу `double` (~1.7e308), меняешь внутреннюю реализацию на `mantissa (double) + exponent (int)` — и вся остальная кодовая база не требует изменений, потому что везде уже используется тип `GameNumber`.

---

## Принципы архитектуры

- **Data / State / Service / View** — строгое разделение слоёв
- **ScriptableObjects** — вся конфигурация (баланс, рецепты, здания, эпохи)
- **Pure C# Model** — игровое состояние без зависимости от Unity (чистые классы)
- **MonoBehaviour** — только как мост к Unity (визуал, input, корутины)
- **Tick-based** — единый игровой цикл через накопитель deltaTime
- **No pathfinding** — логистика через общий пул внутри эпохи, без физических маршрутов
- **GameNumber everywhere** — ни одна игровая величина не хранится как `float`

---

## 1. Data Layer — ScriptableObjects (конфигурация игры)

Создаются дизайнером/разработчиком один раз в Unity Editor. Не содержат рантайм-состояния.

---

### ResourceDefinition : ScriptableObject

Описание одного типа ресурса.

**Поля:**
- `id: string` — уникальный ключ ("wood", "tools", "stone_cycle")
- `displayName: string` — имя для UI ("Дерево", "Рабочий цикл")
- `icon: Sprite`
- `tier: enum { Base, Intermediate, Advanced, EpochOutput }` — уровень ресурса
  - `EpochOutput` — итоговый ресурс-цикл эпохи; не хранится в пуле эпохи, идёт напрямую в `GameState.globalWorkCycles`
- `defaultStorageLimit: GameNumber` — базовый лимит хранения; для `EpochOutput` игнорируется (хранение в глобальном пуле)
- `epochId: string` — к какой эпохе принадлежит; для `EpochOutput` может быть `null` (глобальный ресурс)

---

### RecipeDefinition : ScriptableObject

Описание производственного рецепта.

**Поля:**
- `id: string`
- `inputs: List<ResourceAmount>` — входные ресурсы и их количества за один цикл
- `outputs: List<ResourceAmount>` — выходные ресурсы (если output имеет tier = EpochOutput → идёт в глобальный пул)
- `cycleTime: float` — время одного производственного цикла (секунды); `float` — это реальное время, не игровая величина
- `productionType: enum { Continuous, Discrete }` — непрерывное (добыча) или дискретное (крафт)

> **Continuous** — ресурс начисляется плавно каждый тик: `output += rate * deltaTime`
> **Discrete** — ресурс начисляется порцией при завершении цикла; входные ресурсы списываются в начале цикла

---

### BuildingDefinition : ScriptableObject

Шаблон типа здания. На основе него создаются экземпляры BuildingState.

**Поля:**
- `id: string` — ("lumberjack", "stone_cycle_forge", ...)
- `displayName: string` — "Лесоруб", "Кузня циклов"
- `buildingType: enum { Extractor, Producer, Residential, Logistics, Tech, CycleGenerator }` — тип здания
  - `CycleGenerator` — особый тип: производит EpochOutput-ресурс этой эпохи; выход идёт в `GameState.globalWorkCycles`
- `epochId: string` — к какой эпохе принадлежит
- `recipe: RecipeDefinition` — рецепт (null для добывающих)
- `baseProductionRate: GameNumber` — базовая скорость для добывающих зданий (ед./сек)
- `maxLevel: int` — максимальный уровень
- `maxInstances: int` — максимальное количество экземпляров в эпохе (default 3)
- `baseBuildCost: List<ResourceAmount>` — стоимость строительства первого экземпляра
- `upgradeCostBase: List<ResourceAmount>` — базовая стоимость апгрейда
- `upgradeCostFactor: float` — множитель роста стоимости апгрейда (`base * factor ^ level`); малый коэффициент → `float`
- `productionGrowthFactor: float` — рост производства с уровнем (`base * factor ^ (level-1)`); малый коэффициент → `float`
- `workerSlots: int` — слоты для рабочих
- `workerEffectFactor: float` — коэффициент влияния рабочих (`1 + workers * factor`); малый коэффициент → `float`
- `internalBufferSize: GameNumber` — размер внутреннего буфера входных ресурсов; может расти через апгрейды → `GameNumber`
- `defaultPriority: int` — начальный приоритет в очереди логистики
- `prefab: GameObject` — визуальный префаб здания

---

### TechNodeDefinition : ScriptableObject

Один узел дерева технологий.

**Поля:**
- `id: string` — ("wood_processing", "mechanization", ...)
- `displayName: string` — "Обработка дерева"
- `description: string` — описание эффекта для UI
- `epochId: string` — к какой эпохе принадлежит ("stone_age", "industrial", "global")
- `cost: List<ResourceAmount>` — стоимость исследования
- `prerequisites: List<string>` — список id предшествующих технологий
- `effects: List<TechEffect>` — эффекты при разблокировке

**TechEffect (вложенный класс/struct):**
- `type: enum { UnlockBuilding, UnlockResource, UnlockEpoch, GlobalProductionBonus, CrossEpochBonus, IncreaseStorageLimit, UnlockCrossEpochChannel, IncreaseCycleOutput }`
  - `IncreaseCycleOutput` — увеличивает количество рабочих циклов, производимых CycleGenerator-зданием в эпохе
- `targetId: string` — id здания / ресурса / эпохи / бонуса
- `value: float` — числовое значение (0.15 для +15%); это конфигурационный коэффициент → `float`

> **6 технологий MVP:**
> "Обработка дерева" → "Каменные инструменты" → "Межэпохная доставка" → "Добыча руды" → "Плавка металла" → "Механизация добычи"

---

### EpochDefinition : ScriptableObject

Описание одной эпохи (статические данные).

**Поля:**
- `id: string` — "stone_age", "industrial"
- `displayName: string` — "Каменный век", "Индустриальная эпоха"
- `resources: List<ResourceDefinition>` — все ресурсы эпохи, включая EpochOutput-ресурс
- `buildings: List<BuildingDefinition>` — все здания эпохи, включая CycleGenerator
- `technologies: List<TechNodeDefinition>` — технологии эпохи
- `isUnlockedByDefault: bool` — Каменный век = true
- `unlockRequirements: List<string>` — id технологий, необходимых для разблокировки эпохи
- `epochCycleResourceId: string` — ID ресурса-цикла этой эпохи (tier = EpochOutput), например `"stone_age_cycle"`, `"industrial_cycle"`. Производится зданием типа `CycleGenerator` и идёт напрямую в `GameState.globalWorkCycles`.

> **Рабочий цикл эпохи** — итоговый продукт всей производственной цепочки эпохи. По аналогии с Revolution Idle, где оборот круга = 1 революция, здесь завершение полного производственного цикла эпохи = 1 рабочий цикл. Они накапливаются глобально и служат условием престижа.

---

### CrossEpochLinkDefinition : ScriptableObject

Описание межэпохного канала передачи ресурса.

**Поля:**
- `id: string` — "tools_export"
- `fromEpochId: string` — "stone_age"
- `toEpochId: string` — "industrial"
- `resourceId: string` — "tools" (что передаётся)
- `baseTransferRatePerSecond: GameNumber` — базовая скорость передачи; может масштабироваться → `GameNumber`
- `transferBufferLimit: GameNumber` — максимальный объём буфера канала; может масштабироваться → `GameNumber`
- `unlockTechId: string` — "inter_epoch_delivery" (технология для разблокировки)

---

### GlobalGameConfig : ScriptableObject

Глобальная конфигурация симуляции.

**Поля:**
- `tickRate: float` — частота тика (0.5 сек для MVP); реальное время → `float`
- `autoSaveInterval: float` — интервал автосохранения (30 сек); реальное время → `float`
- `maxOfflineHours: float` — максимальное AFK время (8 часов); реальное время → `float`
- `offlineEfficiencyFactor: float` — коэффициент эффективности AFK расчёта (0.5 в MVP); малый коэф → `float`
- `epochs: List<EpochDefinition>` — все эпохи игры
- `crossEpochLinks: List<CrossEpochLinkDefinition>` — все каналы
- `prestigeThresholdBase: GameNumber` — сколько глобальных рабочих циклов нужно для первого престижа
- `prestigeThresholdGrowthFactor: float` — во сколько раз растёт порог после каждого престижа (например, 5.0); малый коэф → `float`

---

### FinalObjectiveDefinition : ScriptableObject

Описание многоэтапной финальной цели MVP — "Прототип пускового модуля".

**Поля:**
- `id: string` — "launch_prototype"
- `displayName: string`
- `stages: List<ObjectiveStageDefinition>` — этапы цели

**ObjectiveStageDefinition (вложенный класс):**
- `stageName: string` — "Наладить поток инструментов"
- `description: string`
- `requiredResources: List<ResourceAmount>` — что нужно суммарно произвести/накопить
- `requiredTechIds: List<string>` — что должно быть исследовано к этому этапу
- `requiredGlobalCycles: GameNumber` — минимум рабочих циклов в глобальном пуле для прохождения этого этапа (0 = не требуется); для финального этапа ставится ненулевое значение

> **4 этапа MVP:**
> 1. Стабильный поток инструментов в Каменном веке
> 2. Запуск экспорта инструментов в Индустриальную эпоху
> 3. Выпуск машинных деталей
> 4. Постройка Прототипа пускового модуля (требует ресурсы обеих эпох + минимальное количество глобальных циклов)

---

## 2. Runtime State — состояние игры (чистые C# классы)

Живёт в памяти, сериализуется при сохранении. Не содержит логики.

---

### GameNumber (struct)

Универсальный числовой тип для всех игровых величин. Используется вместо `float`/`double` для любого числа, связанного с экономикой игры.

**Внутреннее представление:**
- `mantissa: double` — значащая часть; нормализована: `1.0 ≤ mantissa < 10.0`
- `exponent: int` — степень десяти
- Значение = `mantissa × 10^exponent`
- Диапазон: от `~10^(−2 147 483 648)` до `~10^(+2 147 483 647)`

**Статические константы:** `GameNumber.Zero`, `GameNumber.One`

**Операторы:** `+`, `-`, `*`, `/`, `>=`, `<=`, `==`, `>`, `<`, неявное преобразование из `double`

**Сериализация:** в JSON хранится как строка `"1.234e56"`, при загрузке десериализуется обратно в struct

> **MVP-упрощение:** допустимо реализовать GameNumber как обёртку вокруг `double`. Когда значения начнут приближаться к `1e308`, заменяешь внутренность на `mantissa+exponent` — весь остальной код остаётся без изменений.

---

### ResourceAmount (struct)

Пара "ресурс + количество". Используется повсеместно — в рецептах, стоимостях, списках произведённых ресурсов.

**Поля:**
- `resourceId: string`
- `amount: GameNumber` — количество ресурса; `GameNumber`, потому что стоимости и объёмы масштабируются

---

### ResourcePoolState

Состояние общего пула ресурсов одной эпохи.

**Поля:**
- `epochId: string`
- `amounts: Dictionary<string, GameNumber>` — resourceId → текущее количество
- `storageLimits: Dictionary<string, GameNumber>` — resourceId → текущий лимит (растёт через технологии)
- `incomingRatesCache: Dictionary<string, GameNumber>` — resourceId → суммарная скорость прихода (+X/сек, пересчитывается каждый тик для UI)
- `outgoingRatesCache: Dictionary<string, GameNumber>` — resourceId → суммарная скорость расхода

> Ресурсы с tier = `EpochOutput` в этом пуле **не хранятся** — они направляются напрямую в `GameState.globalWorkCycles`.

---

### BuildingState

Рантайм-состояние одного экземпляра здания.

**Поля:**
- `instanceId: string` — уникальный id экземпляра (GUID)
- `definitionId: string` — ссылка на BuildingDefinition.id
- `epochId: string`
- `level: int` — текущий уровень (начинается с 1)
- `isActive: bool` — включено/выключено игроком
- `status: enum { Active, WaitingForInput, WaitingForOutputSpace, Disabled }` — текущее состояние
- `priority: int` — приоритет логистики (0..10, чем выше — тем раньше получает ресурсы)
- `assignedWorkers: int` — назначено рабочих
- `internalBuffer: Dictionary<string, GameNumber>` — внутренний буфер входных ресурсов (сглаживает скачки)
- `cycleTimer: float` — таймер текущего производственного цикла в секундах; реальное время → `float`
- `cycleProgress: float` — от 0.0 до 1.0 для прогресс-бара; UI-значение → `float`
- `isCycleRunning: bool` — запущен ли цикл (ресурсы уже списаны)
- `effectiveProductionRate: GameNumber` — фактическая скорость с учётом всех множителей (кэш для UI)

---

### EpochState

Полное рантайм-состояние одной эпохи.

**Поля:**
- `epochId: string`
- `isUnlocked: bool`
- `resourcePool: ResourcePoolState`
- `buildings: List<BuildingState>` — все размещённые здания
- `unlockedBuildingIds: HashSet<string>` — какие типы зданий доступны для строительства
- `unlockedResourceIds: HashSet<string>` — какие ресурсы видны и активны
- `unlockedTechIds: HashSet<string>` — исследованные технологии этой эпохи
- `receivedCrossEpochBonuses: Dictionary<string, GameNumber>` — тип_бонуса → значение (бонусы от других эпох; могут расти через престиж → `GameNumber`)
- `totalWorkersCapacity: int` — общий лимит рабочих (от жилых зданий)
- `totalWorkersAssigned: int` — назначено рабочих суммарно
- `logisticsMultiplier: GameNumber` — бонус к скорости логистики от логистических зданий

---

### CrossEpochChannelState

Рантайм-состояние межэпохного канала.

**Поля:**
- `linkId: string`
- `isUnlocked: bool`
- `bufferAmount: GameNumber` — текущий объём ресурса в транзите
- `currentTransferRatePerSecond: GameNumber` — фактическая скорость передачи (с множителями)
- `isBottleneck: bool` — буфер заполнен → передача заблокирована

---

### FinalObjectiveState

Прогресс к финальной цели.

**Поля:**
- `objectiveId: string`
- `currentStageIndex: int` — индекс текущего этапа
- `stageTotalResourcesProduced: Dictionary<string, GameNumber>` — resourceId → накоплено для текущего этапа
- `isCompleted: bool`

---

### OfflineReport

Результат расчёта AFK прогресса. Возвращается OfflineProgressService, показывается игроку.

**Поля:**
- `offlineSeconds: float` — сколько времени прошло; реальное время → `float`
- `resourcesGained: List<ResourceAmount>` — что начислено (amount внутри — `GameNumber`)
- `workCyclesGained: GameNumber` — сколько рабочих циклов было произведено за оффлайн
- `activeBuildingCount: int`
- `idleBuildingCount: int`
- `wasCapReached: bool` — был ли достигнут лимит хранения

---

### PrestigeState

Мета-прогресс игрока, который сохраняется между престижами.

**Поля:**
- `prestigeCount: int` — сколько раз сделан престиж
- `totalPrestigePoints: GameNumber` — накоплено очков престижа за всё время (не сбрасывается)
- `spentPrestigePoints: GameNumber` — потрачено очков
- `permanentMultipliers: Dictionary<string, GameNumber>` — "production", "cycleOutput", "logistics" → постоянный множитель (выживает при сбросе)

> В MVP: `PrestigeState` хранится, очки считаются, но дерево улучшений и кнопка престижа — заглушки.

---

### GameState

Корневой объект состояния игры. Сериализуется при сохранении.

**Поля:**
- `epochs: Dictionary<string, EpochState>` — epochId → состояние
- `crossEpochChannels: Dictionary<string, CrossEpochChannelState>` — linkId → состояние
- `globalMultipliers: Dictionary<string, GameNumber>` — "production", "logistics", "crossEpoch" → значение
- `globalWorkCycles: GameNumber` — **главная игровая величина**: суммарные рабочие циклы, произведённые всеми эпохами за всё время (не сбрасывается при сбросе эпох)
- `workCyclesThisRun: GameNumber` — рабочие циклы, произведённые в текущем прогоне (сбрасывается при престиже)
- `epochCyclesContributed: Dictionary<string, GameNumber>` — epochId → вклад этой эпохи в глобальные циклы (для UI и баланса)
- `prestige: PrestigeState` — мета-прогресс (не сбрасывается)
- `totalResourcesProduced: Dictionary<string, GameNumber>` — для статистики и расчёта очков престижа
- `finalObjective: FinalObjectiveState`
- `lastSaveTimestampUtc: long` — Unix timestamp последнего сохранения (для AFK)
- `totalPlayTimeSeconds: float` — реальное время игры → `float`

---

## 3. Services — логика игры (чистые C# классы, без MonoBehaviour)

Получают зависимости через конструктор. Не имеют ссылок на Unity объекты.

---

### MultiplierService

Вычисляет итоговые множители для конкретного здания. Используется ProductionService.

**Функционал:**
- `GetSpeedMultiplier(BuildingState, EpochState, GameState) → GameNumber`
  Формула: `worker_bonus * level_bonus * global_bonus * cross_epoch_bonus * prestige_bonus`
- `GetOutputMultiplier(BuildingState, EpochState, GameState) → GameNumber`
- `GetCycleOutputMultiplier(BuildingState, EpochState, GameState) → GameNumber` — для CycleGenerator-зданий; учитывает бонусы от технологий типа `IncreaseCycleOutput`
- `GetEfficiencyMultiplier(BuildingState) → GameNumber` — влияет на стоимость входных ресурсов
- Кэширует результат, инвалидирует при изменении уровня/рабочих/технологий

> worker_bonus = 1 + assignedWorkers * workerEffectFactor
> level_bonus = productionGrowthFactor ^ (level - 1)
> prestige_bonus = prestige.permanentMultipliers["production"] (если применимо)

---

### ProductionService

Обновляет производственные циклы всех зданий одной эпохи за один тик.

**Функционал:**
- `Tick(EpochState state, float deltaTime, GameState gameState, GlobalGameConfig config)` — основной вызов
- Итерирует по всем зданиям эпохи, пропускает неактивные (isActive = false)
- **Для Extractor (Continuous):**
  - Начисляет ресурс в пул: `pool.Add(resourceId, rate * dt * speedMultiplier)`; `rate` и результат — `GameNumber`
  - Обновляет effectiveProductionRate для UI
- **Для Producer (Discrete):**
  - Если цикл не запущен: проверить наличие входных ресурсов в internalBuffer
  - Если есть: списать входные ресурсы, выставить isCycleRunning = true, сбросить таймер
  - Если нет: выставить status = WaitingForInput
  - Если цикл запущен: `cycleTimer += deltaTime * speedMultiplier`; обновить cycleProgress
  - При завершении цикла (cycleTimer >= cycleTime): добавить выходные ресурсы в пул, выставить isCycleRunning = false
  - Проверить место в пуле перед добавлением → если нет места: status = WaitingForOutputSpace, pause
- **Для CycleGenerator (Discrete):**
  - Работает как Producer, но при завершении цикла:
    - Вычислить количество производимых циклов: `cycles = baseCyclesPerRun * cycleOutputMultiplier`
    - Добавить `cycles` в `gameState.globalWorkCycles` и `gameState.workCyclesThisRun`
    - Добавить `cycles` в `gameState.epochCyclesContributed[epochId]`
    - Выходной ресурс (`tier = EpochOutput`) **не добавляется в пул эпохи**
- Пересчитывает incomingRates и outgoingRates в ResourcePoolState для UI

---

### LogisticsService

Распределяет ресурсы из общего пула по внутренним буферам зданий с учётом приоритетов.

**Функционал:**
- `Distribute(EpochState state, float deltaTime)` — основной вызов
- Собирает список всех производственных зданий, сортирует по priority (DESC)
- Для каждого здания: если internalBuffer ниже порога — запросить из пула до internalBufferSize
  - Объём, который можно взять за тик: `min(request, transferRate * dt * logisticsMultiplier)`; все операции через `GameNumber`
  - Если в пуле не хватает → здание не получает ресурс, status = WaitingForInput
- Выявляет логистический bottleneck: если суммарный спрос > transferRate → isBottleneck
- Обновляет epochState.logisticsMultiplier на основе активных логистических зданий

---

### BuildingService

Строительство, апгрейд и настройка зданий.

**Функционал:**
- `CanBuild(string definitionId, EpochState) → bool`
  — проверить: тип разблокирован, ресурсы есть, не превышен maxInstances
- `Build(string definitionId, EpochState) → BuildingState`
  — списать baseBuildCost (суммы в `GameNumber`), создать новый BuildingState с level=1
- `CanUpgrade(BuildingState, EpochState) → bool`
- `Upgrade(BuildingState, EpochState)`
  — списать стоимость (`upgradeCostBase * upgradeCostFactor ^ level`), level++, инвалидировать кэш MultiplierService
- `Toggle(BuildingState)` — переключить isActive
- `SetPriority(BuildingState, int priority)` — изменить приоритет (0..10)
- `AssignWorkers(BuildingState, int delta, EpochState)` — изменить assignedWorkers с проверкой слотов и глобального лимита
- `GetUpgradeCost(BuildingState) → List<ResourceAmount>` — вычислить текущую стоимость апгрейда (количества в `GameNumber`)

---

### TechService

Исследование технологий и применение их эффектов к GameState.

**Функционал:**
- `CanResearch(string techId, GameState) → bool`
- `Research(string techId, GameState)` — списать ресурсы, применить все TechEffect
- `ApplyEffect(TechEffect, GameState)` — диспатч по типу:
  - `UnlockBuilding` → добавить в epoch.unlockedBuildingIds
  - `UnlockResource` → добавить в epoch.unlockedResourceIds
  - `UnlockEpoch` → epoch.isUnlocked = true
  - `GlobalProductionBonus` → gameState.globalMultipliers["production"] += value (через `GameNumber`)
  - `CrossEpochBonus` → targetEpoch.receivedCrossEpochBonuses["production"] += value
  - `IncreaseStorageLimit` → pool.storageLimits[resourceId] *= (1 + value)
  - `UnlockCrossEpochChannel` → channelState.isUnlocked = true
  - `IncreaseCycleOutput` → увеличивает множитель выхода CycleGenerator в targetEpoch через `GetCycleOutputMultiplier`

---

### CrossEpochService

Управляет передачей ресурсов между эпохами и применением числовых бонусов.

**Функционал:**
- `Tick(GameState, float deltaTime)` — обновить все активные каналы
- Для каждого активного канала:
  - Взять ресурс из fromEpoch.pool (не превышая `transferRate * dt`; операции через `GameNumber`)
  - Добавить во внутренний буфер канала (не превышая transferBufferLimit)
  - Выгрузить из буфера в toEpoch.pool
  - Если буфер заполнен → channel.isBottleneck = true
- `ApplyCrossEpochBonuses(GameState)` — применить receivedCrossEpochBonuses к множителям эпох

---

### ObjectiveService

Отслеживает прогресс к финальной цели.

**Функционал:**
- `Tick(FinalObjectiveState, GameState)` — проверить текущий этап
- `CheckStageComplete(ObjectiveStageDefinition, GameState) → bool`
  — проверить requiredResources (суммарно произведено), requiredTechIds (все исследованы) и `requiredGlobalCycles` (globalWorkCycles >= required)
- При выполнении этапа: `currentStageIndex++`; если последний — `isCompleted = true`
- `TriggerCompletion(GameState)` — финальная цель достигнута; в MVP показывает экран победы и разблокирует prestиж
- `GetStageProgress(ObjectiveStageDefinition, GameState) → float` — от 0 до 1 для UI

---

### PrestigeService

Управляет мета-прогрессом: проверяет условие, рассчитывает награду, выполняет сброс.

**Функционал:**
- `CanPrestige(GameState, GlobalGameConfig) → bool`
  — `gameState.globalWorkCycles >= CurrentPrestigeThreshold(gameState.prestige, config)`
- `CurrentPrestigeThreshold(PrestigeState, GlobalGameConfig) → GameNumber`
  — `config.prestigeThresholdBase * (config.prestigeThresholdGrowthFactor ^ prestige.prestigeCount)`
- `GetPrestigeReward(GameState, GlobalGameConfig) → PrestigeRewardPreview`
  — предварительный расчёт: сколько очков получит игрок
  — Формула: `earned = sqrt(globalWorkCycles)` (или другая, настраиваемая в GlobalGameConfig)
- `ExecutePrestige(GameState, GlobalGameConfig)`
  1. Вычислить и начислить `prestige.totalPrestigePoints += earned`
  2. Увеличить `prestige.prestigeCount++`
  3. Сбросить `workCyclesThisRun = GameNumber.Zero`; `epochCyclesContributed` очистить
  4. Сбросить все `EpochState` (ресурсы, здания, уровни, рабочие, локальные технологии)
  5. Сбросить `crossEpochChannels` (все разблокировки и буферы)
  6. **Не сбрасывать**: `globalWorkCycles`, `prestige`, `totalResourcesProduced` (статистика)
  7. Применить `prestige.permanentMultipliers` к новому состоянию через MultiplierService

> В MVP: `ExecutePrestige` существует как метод, но кнопка престижа в UI заблокирована до завершения FinalObjective. После победы — показывается экран с результатами и кнопкой запуска нового цикла.

---

### OfflineProgressService

Расчёт прогресса за время AFK. В MVP — упрощённая формула без поцикловой симуляции.

**Функционал:**
- `Calculate(GameState, long nowUtcTimestamp) → OfflineReport`
- Вычислить `offlineTime = min(nowUtc - lastSave, maxOfflineHours * 3600)`; результат в секундах → `float`
- `effectiveTime = offlineTime * offlineEfficiencyFactor`
- Для каждой активной эпохи: для каждого активного Extractor — `earned = rate * effectiveTime`; `rate` и `earned` — `GameNumber`
- Для Producer зданий: `cycles = floor(effectiveTime / cycleTime)`, начислить `cycles * output`
- Для CycleGenerator зданий: `cyclesProduced = floor(effectiveTime / cycleTime) * baseCyclesPerRun`; добавить в `gameState.globalWorkCycles` и `report.workCyclesGained`
- Заполнить OfflineReport и вернуть

---

### SaveService

Сериализация/десериализация GameState.

**Функционал:**
- `Save(GameState)` — сериализовать в JSON, записать в PlayerPrefs (MVP) или файл; `GameNumber` сериализуется как строка
- `Load() → GameState` — прочитать и десериализовать; вернуть null если сохранения нет
- `HasSave() → bool`
- `DeleteSave()` — сброс для тестирования

---

## 4. Controllers / MonoBehaviours — связь логики с Unity

Получают GameManager через инъекцию или синглтон. Не содержат бизнес-логики.

---

### GameManager : MonoBehaviour (singleton)

Главный менеджер. Создаёт сервисы, хранит GameState, управляет тиком.

**Поля:**
- `config: GlobalGameConfig`
- `epochDefinitions: List<EpochDefinition>`
- `objectiveDefinition: FinalObjectiveDefinition`
- `gameState: GameState`
- Экземпляры всех сервисов (ProductionService, LogisticsService, TechService, PrestigeService, ...)
- `tickAccumulator: float` — накопитель deltaTime → `float` (это реальное время)
- `autoSaveAccumulator: float` — счётчик до следующего автосохранения → `float`

**События:**
- `OnTick: Action` — вызывается после каждого тика
- `OnBuildingChanged: Action<string>` — instanceId
- `OnTechResearched: Action<string>`
- `OnObjectiveStageCompleted: Action<int>`
- `OnGameCompleted: Action` — финальная цель достигнута
- `OnPrestigeAvailable: Action` — срабатывает, когда globalWorkCycles достигает порога (для UI-уведомления)
- `OnPrestigeExecuted: Action` — после выполнения сброса

**Что делает:**
- `Awake`: создать все сервисы; загрузить GameState или создать новую; применить OfflineProgressService.Calculate
- `Start`: показать OfflineReportController если прогресс был
- `Update`: `tickAccumulator += Time.deltaTime`; если `>= tickRate` → вызвать Tick, сбросить аккумулятор
- `Tick(float dt)`:
  1. `LogisticsService.Distribute(epochState, dt)` — для каждой активной эпохи
  2. `ProductionService.Tick(epochState, dt, gameState, config)` — для каждой активной эпохи
  3. `CrossEpochService.Tick(gameState, dt)`
  4. `ObjectiveService.Tick(gameState)` — проверить цель
  5. Если `PrestigeService.CanPrestige(gameState, config)` → `OnPrestigeAvailable?.Invoke()`
  6. `OnTick?.Invoke()`
- `RequestPrestige()` — вызов из UI; проверить CanPrestige, выполнить PrestigeService.ExecutePrestige, послать OnPrestigeExecuted
- Методы-прокси для UI: `RequestBuild`, `RequestUpgrade`, `RequestResearch`, `RequestToggle`, `RequestAssignWorkers`, `RequestSetPriority`

---

### EpochViewController : MonoBehaviour

Управляет панелью (вкладкой) одной эпохи.

**Поля:**
- `epochId: string`
- `resourceHUD: ResourceHUDController`
- `buildingListContainer: Transform`
- `buildShopPanel: BuildShopPanelController`
- `buildingViews: Dictionary<string, BuildingViewController>` — instanceId → view

**Что делает:**
- `Initialize(EpochDefinition, EpochState)` — создать начальные BuildingViewController
- Подписаться на `GameManager.OnTick`
- `OnTick()`: вызвать `resourceHUD.Refresh(epochState.resourcePool)`, обновить все buildingViews
- Подписаться на `GameManager.OnBuildingChanged`
- `RefreshBuildShop()` — обновить список доступных для строительства зданий

---

### BuildingViewController : MonoBehaviour

Отображает один экземпляр здания. Принимает пользовательский ввод.

**Поля:**
- `instanceId: string`
- UI-элементы: `nameLabel`, `levelLabel`, `statusIcon`, `cycleProgressBar`, `workersLabel`, `effectiveRateLabel`
- Кнопки: `upgradeButton`, `toggleButton`, `increaseWorkersButton`, `decreaseWorkersButton`, `prioritySlider`

**Что делает:**
- `Refresh(BuildingState, BuildingDefinition)` — обновить все UI-элементы:
  - `NumberFormatter.Format(effectiveProductionRate)` — effectiveProductionRate уже `GameNumber`
  - Отобразить level, status, cycleProgressBar.value = cycleProgress (float 0..1)
  - upgradeButton.interactable = BuildingService.CanUpgrade(...)
- `OnUpgradeClicked()` → `GameManager.RequestUpgrade(instanceId)`
- `OnToggleClicked()` → `GameManager.RequestToggle(instanceId)`
- `OnIncreaseWorkers()` / `OnDecreaseWorkers()` → `GameManager.RequestAssignWorkers(instanceId, ±1)`
- `OnPriorityChanged(value)` → `GameManager.RequestSetPriority(instanceId, value)`

---

### BuildShopPanelController : MonoBehaviour

Список доступных зданий для строительства в текущей эпохе.

**Поля:**
- `epochId: string`
- `buildingItemPrefab: GameObject`
- `buildingItems: List<BuildingShopItemView>`

**Что делает:**
- `Refresh(EpochState)` — для каждого разблокированного типа здания: показать название, количество / maxInstances, стоимость через `NumberFormatter.Format(amount)` (amount — `GameNumber`)
- `OnBuildClicked(definitionId)` → `GameManager.RequestBuild(definitionId, epochId)`

---

### ResourceHUDController : MonoBehaviour

Панель ресурсов эпохи (таблица: иконка + количество + скорость + лимит).

**Поля:**
- `epochId: string`
- `rowTemplate: ResourceRowView`
- `rows: Dictionary<string, ResourceRowView>` — resourceId → строка

**Что делает:**
- `Initialize(List<ResourceDefinition>)` — создать строки для всех ресурсов (кроме EpochOutput — они отображаются в GlobalCyclePanel)
- `Refresh(ResourcePoolState)`:
  - `NumberFormatter.Format(amount)` — amount: `GameNumber`
  - `NumberFormatter.FormatRate(incomingRate - outgoingRate)` — нетто-скорость (+/-X/сек)
  - Красный цвет если нетто < 0 и amount < 10% лимита
  - Жёлтый цвет если amount > 95% лимита

---

### GlobalCyclePanelController : MonoBehaviour

Панель отображения глобальных рабочих циклов и прогресса к престижу.

**Поля:**
- `globalCyclesLabel: Text` — "Рабочие циклы: 1.23K"
- `cyclesThisRunLabel: Text` — "В этом прогоне: 456"
- `epochContributionLabels: Dictionary<string, Text>` — epochId → строка вклада
- `prestigeProgressBar: Slider` — заполненность до порога престижа (0..1)
- `prestigeThresholdLabel: Text` — "Порог: 10.0K"
- `prestigeButton: Button` — активна когда CanPrestige = true

**Что делает:**
- Подписаться на `GameManager.OnTick` и `GameManager.OnPrestigeAvailable`
- `Refresh(GameState, GlobalGameConfig)`:
  - `NumberFormatter.Format(gameState.globalWorkCycles)`
  - Вычислить `progress = workCyclesThisRun / prestigeThreshold`; progress → `float` через `GameNumber.ToDouble()` (нормализовано 0..1 для Slider)
  - Отобразить вклад каждой эпохи из epochCyclesContributed
- `OnPrestigeAvailable()` — активировать prestigeButton, показать анимацию
- `OnPrestigeClicked()` → `GameManager.RequestPrestige()`

---

### TechTreeController : MonoBehaviour

Панель дерева технологий.

**Поля:**
- `epochId: string`
- `nodeViews: Dictionary<string, TechNodeView>`

**Что делает:**
- `Initialize(List<TechNodeDefinition>)` — создать узлы и соединительные линии
- `Refresh(EpochState, ResourcePoolState)` — обновить статус каждого узла (Researched/Available/Locked)
- `OnResearchClicked(techId)` → `GameManager.RequestResearch(techId)`

---

### CrossEpochPanelController : MonoBehaviour

Отображает состояние межэпохного канала и числового бонуса.

**Поля:**
- `channelBufferBar: Slider`
- `transferRateLabel: Text`
- `bottleneckIndicator: GameObject`
- `bonusLabel: Text`

**Что делает:**
- `Refresh(CrossEpochChannelState, GameState)`:
  - `NumberFormatter.Format(bufferAmount)` + `NumberFormatter.Format(transferBufferLimit)` — оба `GameNumber`
  - Показать/скрыть bottleneckIndicator

---

### ObjectivePanelController : MonoBehaviour

Панель прогресса к финальной цели.

**Поля:**
- `stageNameLabel: Text`
- `stageDescriptionLabel: Text`
- `requirementsList: Transform`
- `cyclesRequirementLabel: Text` — показывает прогресс по requiredGlobalCycles
- `overallProgressBar: Slider`

**Что делает:**
- `Refresh(FinalObjectiveState, FinalObjectiveDefinition, GameState)`:
  - Для каждого требования этапа показать прогресс
  - Для `requiredGlobalCycles > 0`: показать `NumberFormatter.Format(globalWorkCycles)` / `NumberFormatter.Format(requiredGlobalCycles)`
- При isCompleted → показать победный экран через `GameManager.OnGameCompleted`

---

### OfflineReportController : MonoBehaviour

Попап при старте игры с информацией о прогрессе за AFK.

**Поля:**
- `panel: GameObject`
- `timeLabel: Text`
- `resourcesGainedList: Transform`
- `workCyclesGainedLabel: Text` — "Рабочих циклов произведено: +45"
- `continueButton: Button`

**Что делает:**
- `Show(OfflineReport)` — показать панель, заполнить данные
  - `NumberFormatter.Format(report.workCyclesGained)` — `GameNumber`
- `OnContinueClicked()` — скрыть панель

---

### EpochSwitcherController : MonoBehaviour

Вкладки/переключатель между эпохами.

**Поля:**
- `epochTabs: List<EpochTabButton>`
- `epochViews: Dictionary<string, EpochViewController>`

**Что делает:**
- `Initialize(List<EpochState>)` — показать только разблокированные вкладки
- `OnTabClicked(epochId)` — активировать нужный EpochViewController
- При разблокировке новой эпохи — показать новую вкладку с анимацией

---

### PrestigeScreenController : MonoBehaviour

Экран престижа — показывается после победы (финальная цель выполнена).

**Поля:**
- `panel: GameObject`
- `cyclesProducedLabel: Text` — "Произведено циклов: 1.23K"
- `pointsEarnedLabel: Text` — "Очков престижа: +35"
- `totalPointsLabel: Text` — "Всего: 35"
- `newRunButton: Button` — запустить новый цикл

**Что делает:**
- `Show(PrestigeRewardPreview)` — заполнить данные из `PrestigeService.GetPrestigeReward()`
- `OnNewRunClicked()` → `GameManager.RequestPrestige()`

---

## 5. Утилиты (чистые статические классы)

---

### NumberFormatter : static class

Форматирование `GameNumber` для UI. Все публичные методы принимают `GameNumber`.

**Методы:**
- `Format(GameNumber value) → string` — "1.2K", "4.5M", "3.1B", "1.23e15"
- `FormatRate(GameNumber perSec) → string` — "+1.2K/с" или "−500/с"
- `FormatTime(float seconds) → string` — "4ч 23м"; время в секундах → `float`
- `FormatPercent(float value) → string` — "+15%"; процент — малое число → `float`
- `FormatCycles(GameNumber cycles) → string` — форматирует рабочие циклы ("1.23K циклов")

---

### BottleneckDetector : static class

Выявляет узкие места для UI-индикаторов.

**Типы bottleneck:**
- `ResourceShortage` — ресурс кончился, здание стоит
- `StorageFull` — пул переполнен, добыча остановлена
- `LogisticsOverload` — скорость передачи не успевает за спросом
- `CrossEpochChannelFull` — буфер межэпохного канала заполнен
- `CycleGeneratorIdle` — CycleGenerator простаивает из-за нехватки входных ресурсов

**Методы:**
- `GetBottlenecks(EpochState) → List<BottleneckInfo>`
- `GetCrossEpochBottleneck(CrossEpochChannelState) → BottleneckInfo?`
- `BottleneckInfo`: `{ type, affectedBuildingIds, resourceId, description: string }`

---

### BalanceCalculator : static class

Вычисления по балансным формулам. Все игровые величины возвращаются как `GameNumber`.

**Методы:**
- `GetUpgradeCost(BuildingDefinition def, int currentLevel) → List<ResourceAmount>`
  Формула: `base * costFactor ^ currentLevel`; `base` — `GameNumber`, `costFactor` — `float`
- `GetEffectiveProductionRate(BuildingDefinition, int level, int workers, GameNumber speedMultiplier) → GameNumber`
  Формула: `baseRate * growthFactor ^ (level-1) * speedMultiplier`
- `GetWorkerBonus(int workers, float factor) → GameNumber`
  Формула: `1 + workers * factor`
- `GetCycleTime(BuildingDefinition, int level, float speedMultiplier) → float`
  Формула: `baseTime / speedMultiplier`; результат — время в секундах → `float`
- `GetTransferRate(GameNumber baseRate, GameNumber logisticsMultiplier) → GameNumber`
- `GetPrestigeThreshold(int prestigeCount, GameNumber base, float growthFactor) → GameNumber`
  Формула: `base * growthFactor ^ prestigeCount`
- `GetPrestigePointsEarned(GameNumber globalWorkCycles) → GameNumber`
  Формула: `sqrt(globalWorkCycles)`

---

## 6. Порядок операций в игровом тике

```
GameManager.Tick(deltaTime):

1. [для каждой активной эпохи]
   LogisticsService.Distribute(epochState, dt)
   → сортировать здания по priority
   → пополнить internalBuffer из пула с учётом transferRate * logisticsMultiplier
   → все количества — GameNumber

2. [для каждой активной эпохи]
   ProductionService.Tick(epochState, dt, gameState)
   → Extractor: pool += rate * dt * speedMultiplier  (GameNumber)
   → Producer: обновить таймер (float), запустить/завершить цикл, списать/начислить (GameNumber)
   → CycleGenerator: обновить таймер (float), при завершении:
       gameState.globalWorkCycles += cycles * cycleOutputMultiplier  (GameNumber)
       gameState.workCyclesThisRun += cycles  (GameNumber)
       epochCyclesContributed[epochId] += cycles  (GameNumber)
   → Обновить кэши incomingRates / outgoingRates  (GameNumber)

3. CrossEpochService.Tick(gameState, dt)
   → взять из fromEpoch.pool, добавить в канальный буфер  (GameNumber)
   → выгрузить из буфера в toEpoch.pool  (GameNumber)
   → обновить isBottleneck

4. ObjectiveService.Tick(gameState)
   → проверить прогресс текущего этапа
   → проверить requiredGlobalCycles <= gameState.globalWorkCycles
   → при выполнении перейти к следующему

5. Проверить OnPrestigeAvailable
   if PrestigeService.CanPrestige(gameState, config):
       OnPrestigeAvailable?.Invoke()

6. OnTick?.Invoke()
   → все подписанные View-контроллеры обновляют UI
```

---

## 7. Что входит в MVP — сводная таблица

| Система | Реализуется | Что включает |
|---|---|---|
| Ресурсы | Да | 8 ресурсов (по 4 в эпохе), пул, лимиты, скорости; **все количества — GameNumber** |
| Здания | Да | 8 типов + 2 CycleGenerator (по 1 в эпохе), уровни, апгрейды, статусы |
| Производство | Да | Continuous + Discrete + CycleGenerator; таймеры (`float`), объёмы (`GameNumber`) |
| Логистика | Да | Общий пул, приоритеты, transfer rate; скорости — `GameNumber` |
| Рабочие | Частично | Глобальный лимит + назначение |
| Технологии | Да | 6 узлов + 1 тип IncreaseCycleOutput |
| Эпохи | Да | 2 параллельные, у каждой — epochCycleResourceId и CycleGenerator |
| Межэпохная связь | Да | 1 ресурсный канал + 1 числовой бонус |
| **Рабочие циклы** | **Да** | **globalWorkCycles (GameNumber); CycleGenerator в каждой эпохе; epochCyclesContributed** |
| Финальная цель | Да | 4 этапа; последний этап требует минимум globalWorkCycles |
| Оффлайн прогресс | Заглушка | Упрощённый расчёт; учитывает CycleGenerator → workCyclesGained |
| Сохранение | Да | JSON в PlayerPrefs; GameNumber сериализуется как строка |
| **Престиж** | **Заглушка** | **PrestigeState считается; PrestigeService реализован; кнопка активна после победы** |
| GameNumber | Да | Все игровые величины через GameNumber; MVP-реализация на double |
| Глобальные улучшения | Нет | — |

---

## 8. Что добавляется после MVP

- Дерево улучшений престижа (PrestigeUpgradeService) — трата накопленных очков
- Глобальные улучшения (GlobalUpgradeService)
- Полный оффлайн прогресс с поцикловой симуляцией
- Финальная ракета как 5-этапная цель с высокими требованиями по globalWorkCycles
- Эпохи: Античность, Космическая — каждая со своим CycleGenerator и epochCycleResourceId
- Сложные кросс-эпохные графы (CrossEpochLinkGraph)
- Множественные источники globalWorkCycles с разными весовыми коэффициентами
- Замена внутренности GameNumber с `double` на `mantissa+exponent` при необходимости
- Логистика с несколькими маршрутами и буферами
- Детальный UI рабочих с ручным drag-and-drop
