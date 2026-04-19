# Архитектурный скелет MVP 0.1

> **Версия:** 0.1 — стартовый скелет  
> **Проект:** gameMVPv2 / Unity 6.2 LTS · URP · UI Toolkit · New Input System  
> **Соответствует:** `docs/game-design/MVP 0.1.md`, `.cursor/plans/план_mvp_0.1_c06638b8.plan.md`

---

## 1. Ключевые архитектурные решения

| Вопрос | Решение | Обоснование |
|---|---|---|
| DI | Простой **Composition Root** без внешних пакетов | VContainer лишний для MVP; вся сборка в одном месте прозрачна |
| Симуляция | **Headless Pure C#** — без MonoBehaviour зависимостей | Тестируется в EditMode без Play Mode; детерминирована |
| Данные | **ScriptableObject** только как read-only definitions | Лёгкая правка в Editor; Addressables для загрузки |
| Состояние | **Plain C# runtime state** — отдельно от definitions | `Definition` = что возможно; `State` = что происходит сейчас |
| Тик | Фиксированный, `0.25s` по умолчанию, настраивается в `GameConfig` | Детерминизм и простота отладки |
| UI | **UI Toolkit** — Presenter поверх View (UXML/USS) | Соответствует правилам проекта; разделение логики и вёрстки |
| Ассеты | **Addressables** для ScriptableObject definitions и UI ассетов | Не `Resources.Load` |
| Out of scope | Workers, prestige, offline, pathfinding, 3-я эпоха | Явно исключены из MVP |

---

## 2. Слои архитектуры

```
┌─────────────────────────────────────────────────┐
│                   UI Layer                       │
│  Presenters  ←  State change events              │
│  UXML Panels ←  Presenters binding              │
└──────────────────┬──────────────────────────────┘
                   │ reads State, calls Services
┌──────────────────▼──────────────────────────────┐
│               Service Layer                      │
│  BuildingUpgradeService                          │
│  TechnologyService                               │
│  GoalService                                     │
└──────────────────┬──────────────────────────────┘
                   │ mutates State
┌──────────────────▼──────────────────────────────┐
│             Simulation Layer (Pure C#)           │
│  SimulationTick ← EpochSimulator                 │
│  CrossEpochTransferSystem                        │
│  Priority Resolver                               │
└──────────────────┬──────────────────────────────┘
                   │ reads/writes
┌──────────────────▼──────────────────────────────┐
│               State Layer (Pure C#)              │
│  GameState → EpochState[] → ResourceLedger       │
│                           → BuildingRuntimeState[]│
│            → TechState                           │
│            → CrossEpochChannelState              │
│            → GoalState                           │
└──────────────────┬──────────────────────────────┘
                   │ reads
┌──────────────────▼──────────────────────────────┐
│               Data Layer (ScriptableObject)      │
│  GameConfig → EpochDefinition[]                  │
│            → CrossEpochLinkDefinition[]          │
│            → GoalDefinition                      │
│  EpochDefinition → ResourceDefinition[]          │
│                  → BuildingDefinition[]          │
│                  → RecipeDefinition[]            │
│                  → TechnologyDefinition[]        │
└─────────────────────────────────────────────────┘
                   │ loaded by
┌──────────────────▼──────────────────────────────┐
│           Bootstrap / Composition Root           │
│  GameBootstrap : MonoBehaviour                   │
│  SimulationRunner : MonoBehaviour                │
└─────────────────────────────────────────────────┘
```

---

## 3. Структура папок `Assets/`

```
Assets/
│
├── Scenes/
│   ├── Bootstrap.unity          ← точка входа приложения
│   └── Game.unity               ← основная игровая сцена
│
├── Scripts/
│   ├── Runtime/                 ← asmdef: Game.Runtime
│   │   ├── Ids/                 ← value types-идентификаторы
│   │   │   └── EpochId.cs, ResourceId.cs, BuildingId.cs, TechId.cs
│   │   ├── Data/                ← ScriptableObject definitions
│   │   │   ├── GameConfig.cs
│   │   │   ├── EpochDefinition.cs
│   │   │   ├── ResourceDefinition.cs
│   │   │   ├── RecipeDefinition.cs
│   │   │   ├── BuildingDefinition.cs
│   │   │   ├── TechnologyDefinition.cs
│   │   │   ├── CrossEpochLinkDefinition.cs
│   │   │   └── GoalDefinition.cs
│   │   ├── State/               ← чистое runtime-состояние
│   │   │   ├── GameState.cs
│   │   │   ├── EpochState.cs
│   │   │   ├── ResourceLedger.cs
│   │   │   ├── BuildingRuntimeState.cs
│   │   │   ├── TechState.cs
│   │   │   ├── CrossEpochChannelState.cs
│   │   │   └── GoalState.cs
│   │   ├── Simulation/          ← pure C# тик-логика
│   │   │   ├── SimulationTick.cs
│   │   │   ├── EpochSimulator.cs
│   │   │   ├── PriorityResolver.cs
│   │   │   └── CrossEpochTransferSystem.cs
│   │   └── Services/            ← операции игрока
│   │       ├── BuildingUpgradeService.cs
│   │       ├── TechnologyService.cs
│   │       └── GoalService.cs
│   │
│   ├── Infrastructure/          ← asmdef: Game.Infrastructure
│   │   ├── Bootstrap/
│   │   │   ├── GameBootstrap.cs        ← MonoBehaviour: Composition Root
│   │   │   └── SimulationRunner.cs     ← MonoBehaviour: тик-драйвер
│   │   └── Addressables/
│   │       └── AddressablesLoader.cs
│   │
│   └── UI/                      ← asmdef: Game.UI
│       ├── HudController.cs     ← корневой контроллер UI
│       ├── Presenters/
│       │   ├── EpochPanelPresenter.cs
│       │   ├── TechPanelPresenter.cs
│       │   ├── CrossEpochPanelPresenter.cs
│       │   └── GoalTrackerPresenter.cs
│       └── ViewModels/
│           ├── ResourceViewModel.cs
│           ├── BuildingViewModel.cs
│           └── TechViewModel.cs
│
├── Data/                        ← ScriptableObject asset instances
│   ├── Config/
│   │   └── GameConfig.asset
│   ├── Epochs/
│   │   ├── StoneAge.asset
│   │   └── IndustrialAge.asset
│   ├── Resources/
│   │   ├── StoneAge_Wood.asset
│   │   ├── StoneAge_Stone.asset
│   │   ├── StoneAge_Planks.asset
│   │   ├── StoneAge_Tools.asset
│   │   ├── Industrial_Ore.asset
│   │   ├── Industrial_Coal.asset
│   │   ├── Industrial_Metal.asset
│   │   └── Industrial_MachineParts.asset
│   ├── Buildings/
│   │   ├── StoneAge_Woodcutter.asset
│   │   ├── StoneAge_Quarry.asset
│   │   ├── StoneAge_Workshop.asset
│   │   ├── StoneAge_RelayWarehouse.asset
│   │   ├── Industrial_Mine.asset
│   │   ├── Industrial_Smelter.asset
│   │   ├── Industrial_AssemblyPlant.asset
│   │   └── Industrial_LogisticsHub.asset
│   ├── Recipes/
│   ├── Technologies/
│   │   ├── Tech_WoodProcessing.asset
│   │   ├── Tech_StoneTools.asset
│   │   ├── Tech_CrossEpochDelivery.asset
│   │   ├── Tech_OreMining.asset
│   │   ├── Tech_MetalSmelting.asset
│   │   └── Tech_MiningMechanization.asset
│   └── Goals/
│       └── Goal_LaunchPadPrototype.asset
│
├── UI/
│   ├── Documents/               ← UXML
│   │   ├── GameHUD.uxml
│   │   ├── EpochPanel.uxml
│   │   ├── BuildingCard.uxml
│   │   ├── TechPanel.uxml
│   │   ├── CrossEpochPanel.uxml
│   │   └── GoalTracker.uxml
│   └── Stylesheets/             ← USS
│       └── main.uss
│
├── Addressables/                ← Addressables groups
│
└── Tests/
    ├── EditMode/                ← asmdef: Game.Tests.EditMode
    │   ├── ResourceLedgerTests.cs
    │   ├── EpochSimulatorTests.cs
    │   ├── PriorityResolverTests.cs
    │   └── CrossEpochTransferTests.cs
    └── PlayMode/                ← asmdef: Game.Tests.PlayMode
        └── SimulationSmokeTest.cs
```

---

## 4. Assembly Definitions

| Asmdef | Зависимости | Платформы |
|---|---|---|
| `Game.Runtime` | — | Any |
| `Game.Infrastructure` | `Game.Runtime` | Any |
| `Game.UI` | `Game.Runtime`, `UnityEngine.UIElementsModule` | Any |
| `Game.Tests.EditMode` | `Game.Runtime`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner` | Editor only |
| `Game.Tests.PlayMode` | `Game.Runtime`, `Game.Infrastructure`, `UnityEngine.TestRunner` | Any |

---

## 5. Data Layer — ScriptableObject Definitions

### 5.1 Идентификаторы (Ids/)

```csharp
// Ids/ResourceId.cs
/// <summary>Typed identifier for a resource, wrapping a string asset id.</summary>
public readonly struct ResourceId : IEquatable<ResourceId>
{
    public readonly string Value;
    public ResourceId(string value) => Value = value;
    public bool Equals(ResourceId other) => Value == other.Value;
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    public override string ToString() => Value;
    public static implicit operator string(ResourceId id) => id.Value;
}

// То же самое для EpochId, BuildingId, TechId
```

### 5.2 GameConfig

```csharp
// Data/GameConfig.cs
[CreateAssetMenu(menuName = "Game/Config/GameConfig")]
public sealed class GameConfig : ScriptableObject
{
    [SerializeField] private float _tickInterval = 0.25f;
    [SerializeField] private EpochDefinition[] _epochs;
    [SerializeField] private CrossEpochLinkDefinition[] _crossEpochLinks;
    [SerializeField] private GoalDefinition _mainGoal;

    public float TickInterval => _tickInterval;
    public IReadOnlyList<EpochDefinition> Epochs => _epochs;
    public IReadOnlyList<CrossEpochLinkDefinition> CrossEpochLinks => _crossEpochLinks;
    public GoalDefinition MainGoal => _mainGoal;
}
```

### 5.3 ResourceDefinition

```csharp
// Data/ResourceDefinition.cs
[CreateAssetMenu(menuName = "Game/Data/Resource")]
public sealed class ResourceDefinition : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private Sprite _icon;
    [SerializeField] private float _baseStorageCap;

    public ResourceId Id => new(_id);
    public string DisplayName => _displayName;
    public Sprite Icon => _icon;
    public float BaseStorageCap => _baseStorageCap;
}
```

### 5.4 RecipeDefinition

```csharp
// Data/RecipeDefinition.cs
[CreateAssetMenu(menuName = "Game/Data/Recipe")]
public sealed class RecipeDefinition : ScriptableObject
{
    [System.Serializable]
    public struct ResourceAmount
    {
        public ResourceDefinition Resource;
        public float Amount;
    }

    [SerializeField] private ResourceAmount[] _inputs;
    [SerializeField] private ResourceAmount[] _outputs;
    /// <summary>Units produced per second at level 1.</summary>
    [SerializeField] private float _baseRatePerSecond;
    [SerializeField] private TechnologyDefinition _requiredTech;

    public IReadOnlyList<ResourceAmount> Inputs => _inputs;
    public IReadOnlyList<ResourceAmount> Outputs => _outputs;
    public float BaseRatePerSecond => _baseRatePerSecond;
    public TechnologyDefinition RequiredTech => _requiredTech;
}
```

### 5.5 BuildingDefinition

```csharp
// Data/BuildingDefinition.cs
[CreateAssetMenu(menuName = "Game/Data/Building")]
public sealed class BuildingDefinition : ScriptableObject
{
    [System.Serializable]
    public struct UpgradeCost
    {
        public ResourceDefinition Resource;
        public float BaseCost;
        /// <summary>cost = BaseCost * GrowthFactor^level</summary>
        public float GrowthFactor;
    }

    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private EpochDefinition _epoch;
    [SerializeField] private RecipeDefinition _recipe;
    [SerializeField] private UpgradeCost[] _buildCost;
    [SerializeField] private float _productionMultiplierPerLevel = 1.2f;
    [SerializeField] private float _storageModifier;
    [SerializeField] private float _logisticsModifier;
    [SerializeField] private int _defaultPriority = 1;   // 0=low 1=normal 2=high
    [SerializeField] private TechnologyDefinition _unlockTech;

    public BuildingId Id => new(_id);
    public string DisplayName => _displayName;
    public EpochDefinition Epoch => _epoch;
    public RecipeDefinition Recipe => _recipe;
    public IReadOnlyList<UpgradeCost> BuildCost => _buildCost;
    public float ProductionMultiplierPerLevel => _productionMultiplierPerLevel;
    public float StorageModifier => _storageModifier;
    public float LogisticsModifier => _logisticsModifier;
    public int DefaultPriority => _defaultPriority;
    public TechnologyDefinition UnlockTech => _unlockTech;
}
```

### 5.6 TechnologyDefinition

```csharp
// Data/TechnologyDefinition.cs
[CreateAssetMenu(menuName = "Game/Data/Technology")]
public sealed class TechnologyDefinition : ScriptableObject
{
    [System.Serializable]
    public struct TechCost
    {
        public ResourceDefinition Resource;
        public float Amount;
    }

    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private string _description;
    [SerializeField] private TechCost[] _cost;
    [SerializeField] private TechnologyDefinition[] _prerequisites;
    /// <summary>Flat percentage bonus applied to base production rate of target epoch. 0.15 = +15%.</summary>
    [SerializeField] private float _productionBonus;
    [SerializeField] private EpochDefinition _bonusTargetEpoch;

    public TechId Id => new(_id);
    public string DisplayName => _displayName;
    public string Description => _description;
    public IReadOnlyList<TechCost> Cost => _cost;
    public IReadOnlyList<TechnologyDefinition> Prerequisites => _prerequisites;
    public float ProductionBonus => _productionBonus;
    public EpochDefinition BonusTargetEpoch => _bonusTargetEpoch;
}
```

### 5.7 EpochDefinition

```csharp
// Data/EpochDefinition.cs
[CreateAssetMenu(menuName = "Game/Data/Epoch")]
public sealed class EpochDefinition : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private ResourceDefinition[] _resources;
    [SerializeField] private BuildingDefinition[] _buildings;
    [SerializeField] private TechnologyDefinition[] _technologies;
    [SerializeField] private float _baseLogisticsCapacity = 100f;
    [SerializeField] private bool _unlockedByDefault;

    public EpochId Id => new(_id);
    public string DisplayName => _displayName;
    public IReadOnlyList<ResourceDefinition> Resources => _resources;
    public IReadOnlyList<BuildingDefinition> Buildings => _buildings;
    public IReadOnlyList<TechnologyDefinition> Technologies => _technologies;
    public float BaseLogisticsCapacity => _baseLogisticsCapacity;
    public bool UnlockedByDefault => _unlockedByDefault;
}
```

### 5.8 CrossEpochLinkDefinition

```csharp
// Data/CrossEpochLinkDefinition.cs
[CreateAssetMenu(menuName = "Game/Data/CrossEpochLink")]
public sealed class CrossEpochLinkDefinition : ScriptableObject
{
    public enum LinkType { ResourceTransfer, ProductionBonus }

    [SerializeField] private EpochDefinition _sourceEpoch;
    [SerializeField] private EpochDefinition _targetEpoch;
    [SerializeField] private LinkType _type;
    [SerializeField] private ResourceDefinition _transferredResource;
    [SerializeField] private float _baseTransferRatePerSecond;
    [SerializeField] private float _transferLimit;
    [SerializeField] private TechnologyDefinition _unlockCondition;

    public EpochDefinition SourceEpoch => _sourceEpoch;
    public EpochDefinition TargetEpoch => _targetEpoch;
    public LinkType Type => _type;
    public ResourceDefinition TransferredResource => _transferredResource;
    public float BaseTransferRatePerSecond => _baseTransferRatePerSecond;
    public float TransferLimit => _transferLimit;
    public TechnologyDefinition UnlockCondition => _unlockCondition;
}
```

### 5.9 GoalDefinition

```csharp
// Data/GoalDefinition.cs
[CreateAssetMenu(menuName = "Game/Data/Goal")]
public sealed class GoalDefinition : ScriptableObject
{
    [System.Serializable]
    public struct ResourceRequirement
    {
        public ResourceDefinition Resource;
        public EpochDefinition Epoch;
        public float Amount;
    }

    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private string _description;
    [SerializeField] private ResourceRequirement[] _requirements;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string Description => _description;
    public IReadOnlyList<ResourceRequirement> Requirements => _requirements;
}
```

---

## 6. State Layer — Runtime State (Pure C#)

### 6.1 ResourceLedger

```csharp
// State/ResourceLedger.cs
/// <summary>Tracks current resource amounts and storage caps for one epoch.</summary>
public sealed class ResourceLedger
{
    private readonly Dictionary<ResourceId, float> _amounts = new();
    private readonly Dictionary<ResourceId, float> _caps = new();

    public float Get(ResourceId id) => _amounts.GetValueOrDefault(id);
    public float GetCap(ResourceId id) => _caps.GetValueOrDefault(id, float.MaxValue);

    public void SetCap(ResourceId id, float cap) => _caps[id] = cap;

    /// <summary>Returns actual amount added after clamping to cap.</summary>
    public float Add(ResourceId id, float amount)
    {
        float current = Get(id);
        float cap = GetCap(id);
        float toAdd = Mathf.Min(amount, cap - current);
        _amounts[id] = current + toAdd;
        return toAdd;
    }

    /// <summary>Returns true if the full amount was available and consumed.</summary>
    public bool TryConsume(ResourceId id, float amount)
    {
        float current = Get(id);
        if (current < amount) return false;
        _amounts[id] = current - amount;
        return true;
    }

    public bool IsFull(ResourceId id) => Get(id) >= GetCap(id) - 0.001f;
}
```

### 6.2 BuildingRuntimeState

```csharp
// State/BuildingRuntimeState.cs
public enum IdleReason
{
    None,
    MissingInput,
    StorageFull,
    TransferBlocked,
    LockedByTech,
    NoDemand
}

/// <summary>Mutable runtime snapshot of a single building instance.</summary>
public sealed class BuildingRuntimeState
{
    public BuildingDefinition Definition { get; }
    public bool IsBuilt { get; set; }
    public int Level { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public IdleReason IdleReason { get; set; }

    public BuildingRuntimeState(BuildingDefinition definition)
    {
        Definition = definition;
        Priority = definition.DefaultPriority;
    }

    /// <summary>Effective production multiplier at current level.</summary>
    public float ProductionMultiplier =>
        Mathf.Pow(Definition.ProductionMultiplierPerLevel, Level - 1);
}
```

### 6.3 EpochState

```csharp
// State/EpochState.cs
public sealed class EpochState
{
    public EpochDefinition Definition { get; }
    public ResourceLedger Ledger { get; } = new();
    public IReadOnlyList<BuildingRuntimeState> Buildings { get; }
    public bool IsUnlocked { get; set; }
    public float LogisticsCapacity { get; set; }
    public float LogisticsLoad { get; set; }

    public EpochState(EpochDefinition definition)
    {
        Definition = definition;
        IsUnlocked = definition.UnlockedByDefault;
        LogisticsCapacity = definition.BaseLogisticsCapacity;
        Buildings = definition.Buildings
            .Select(b => new BuildingRuntimeState(b))
            .ToList();
    }

    public BuildingRuntimeState GetBuilding(BuildingId id) =>
        Buildings.First(b => b.Definition.Id.Equals(id));
}
```

### 6.4 TechState

```csharp
// State/TechState.cs
public sealed class TechState
{
    private readonly HashSet<TechId> _unlocked = new();

    public bool IsUnlocked(TechId id) => _unlocked.Contains(id);

    public bool CanUnlock(TechnologyDefinition tech)
    {
        if (IsUnlocked(tech.Id)) return false;
        return tech.Prerequisites.All(p => IsUnlocked(p.Id));
    }

    public void Unlock(TechId id) => _unlocked.Add(id);
}
```

### 6.5 CrossEpochChannelState

```csharp
// State/CrossEpochChannelState.cs
public sealed class CrossEpochChannelState
{
    public CrossEpochLinkDefinition Definition { get; }
    public bool IsUnlocked { get; set; }
    public float BufferAmount { get; set; }
    public float CurrentLoad { get; set; }

    public CrossEpochChannelState(CrossEpochLinkDefinition definition)
        => Definition = definition;

    public bool IsSaturated => CurrentLoad >= Definition.TransferLimit * 0.95f;
}
```

### 6.6 GoalState

```csharp
// State/GoalState.cs
public sealed class GoalState
{
    public GoalDefinition Definition { get; }
    public bool IsCompleted { get; set; }
    public Dictionary<string, float> Progress { get; } = new();

    public GoalState(GoalDefinition definition) => Definition = definition;
}
```

### 6.7 GameState (корневой объект)

```csharp
// State/GameState.cs
/// <summary>Single source of truth for all runtime simulation data.</summary>
public sealed class GameState
{
    public IReadOnlyList<EpochState> Epochs { get; }
    public TechState Tech { get; } = new();
    public IReadOnlyList<CrossEpochChannelState> CrossEpochChannels { get; }
    public GoalState Goal { get; }

    public event Action OnStateChanged;

    public GameState(GameConfig config)
    {
        Epochs = config.Epochs.Select(e => new EpochState(e)).ToList();
        CrossEpochChannels = config.CrossEpochLinks
            .Select(l => new CrossEpochChannelState(l)).ToList();
        Goal = new GoalState(config.MainGoal);
    }

    public EpochState GetEpoch(EpochId id) =>
        Epochs.First(e => e.Definition.Id.Equals(id));

    public void NotifyChanged() => OnStateChanged?.Invoke();
}
```

---

## 7. Simulation Layer — Pure C# Tick Logic

### 7.1 EpochSimulator

```csharp
// Simulation/EpochSimulator.cs
/// <summary>
/// Executes one tick for a single epoch.
/// Order: check inputs → consume → produce → apply storage caps → record idle reasons.
/// </summary>
public sealed class EpochSimulator
{
    private readonly PriorityResolver _priorityResolver;

    public EpochSimulator(PriorityResolver priorityResolver)
        => _priorityResolver = priorityResolver;

    public void Tick(EpochState epoch, TechState tech, float deltaTime)
    {
        var orderedBuildings = _priorityResolver.Sort(epoch.Buildings);

        foreach (var building in orderedBuildings)
        {
            if (!building.IsBuilt)
                continue;

            if (!IsTechUnlocked(building, tech))
            {
                building.IsActive = false;
                building.IdleReason = IdleReason.LockedByTech;
                continue;
            }

            TryProduceBuilding(building, epoch.Ledger, tech, deltaTime);
        }

        RecalculateLogisticsLoad(epoch, deltaTime);
    }

    private static void TryProduceBuilding(
        BuildingRuntimeState building,
        ResourceLedger ledger,
        TechState tech,
        float deltaTime)
    {
        var recipe = building.Definition.Recipe;
        if (recipe == null) return;

        float rate = recipe.BaseRatePerSecond * building.ProductionMultiplier * deltaTime;

        // Check outputs won't overflow
        foreach (var output in recipe.Outputs)
        {
            if (ledger.IsFull(output.Resource.Id))
            {
                building.IsActive = false;
                building.IdleReason = IdleReason.StorageFull;
                return;
            }
        }

        // Check and consume inputs
        foreach (var input in recipe.Inputs)
        {
            if (!ledger.TryConsume(input.Resource.Id, input.Amount * rate))
            {
                building.IsActive = false;
                building.IdleReason = IdleReason.MissingInput;
                return;
            }
        }

        // Produce outputs
        foreach (var output in recipe.Outputs)
            ledger.Add(output.Resource.Id, output.Amount * rate);

        building.IsActive = true;
        building.IdleReason = IdleReason.None;
    }

    private static bool IsTechUnlocked(BuildingRuntimeState building, TechState tech)
        => building.Definition.UnlockTech == null
           || tech.IsUnlocked(building.Definition.UnlockTech.Id);

    private static void RecalculateLogisticsLoad(EpochState epoch, float deltaTime)
    {
        float demand = epoch.Buildings
            .Where(b => b.IsBuilt && b.IsActive)
            .Sum(b => b.Definition.LogisticsModifier);

        epoch.LogisticsLoad = demand;
    }
}
```

### 7.2 PriorityResolver

```csharp
// Simulation/PriorityResolver.cs
/// <summary>Sorts buildings by priority descending so high-priority buildings get resources first.</summary>
public sealed class PriorityResolver
{
    public IEnumerable<BuildingRuntimeState> Sort(IReadOnlyList<BuildingRuntimeState> buildings)
        => buildings.OrderByDescending(b => b.Priority);
}
```

### 7.3 CrossEpochTransferSystem

```csharp
// Simulation/CrossEpochTransferSystem.cs
/// <summary>Transfers resources between epochs through defined channels, respecting transfer rate and limit.</summary>
public sealed class CrossEpochTransferSystem
{
    public void Tick(
        GameState state,
        float deltaTime)
    {
        foreach (var channel in state.CrossEpochChannels)
        {
            if (!channel.IsUnlocked)
            {
                channel.CurrentLoad = 0;
                continue;
            }

            var link = channel.Definition;
            var source = state.GetEpoch(link.SourceEpoch.Id);
            var target = state.GetEpoch(link.TargetEpoch.Id);
            var resourceId = link.TransferredResource.Id;

            float maxTransferThisTick = link.BaseTransferRatePerSecond * deltaTime;
            float remaining = link.TransferLimit - channel.BufferAmount;
            float toTransfer = Mathf.Min(maxTransferThisTick, remaining);
            toTransfer = Mathf.Min(toTransfer, source.Ledger.Get(resourceId));

            if (toTransfer > 0)
            {
                source.Ledger.TryConsume(resourceId, toTransfer);
                target.Ledger.Add(resourceId, toTransfer);
                channel.BufferAmount += toTransfer;
            }

            channel.CurrentLoad = toTransfer / Mathf.Max(maxTransferThisTick, 0.0001f);
        }
    }
}
```

### 7.4 SimulationTick (оркестратор одного тика)

```csharp
// Simulation/SimulationTick.cs
/// <summary>Orchestrates one full simulation tick across all epochs and cross-epoch channels.</summary>
public sealed class SimulationTick
{
    private readonly EpochSimulator _epochSimulator;
    private readonly CrossEpochTransferSystem _crossEpochTransfer;

    public SimulationTick(EpochSimulator epochSimulator, CrossEpochTransferSystem crossEpochTransfer)
    {
        _epochSimulator = epochSimulator;
        _crossEpochTransfer = crossEpochTransfer;
    }

    public void Execute(GameState state, float deltaTime)
    {
        foreach (var epoch in state.Epochs.Where(e => e.IsUnlocked))
            _epochSimulator.Tick(epoch, state.Tech, deltaTime);

        _crossEpochTransfer.Tick(state, deltaTime);

        state.NotifyChanged();
    }
}
```

---

## 8. Services Layer — Операции Игрока

### 8.1 BuildingUpgradeService

```csharp
// Services/BuildingUpgradeService.cs
public sealed class BuildingUpgradeService
{
    private readonly GameState _state;

    public BuildingUpgradeService(GameState state) => _state = state;

    /// <summary>Returns true if the player can afford to build at current level.</summary>
    public bool CanBuild(BuildingRuntimeState building, EpochState epoch)
    {
        if (building.IsBuilt) return false;
        return CanAfford(building.Definition.BuildCost, epoch.Ledger, 0);
    }

    public bool CanUpgrade(BuildingRuntimeState building, EpochState epoch)
    {
        if (!building.IsBuilt) return false;
        return CanAfford(building.Definition.BuildCost, epoch.Ledger, building.Level);
    }

    public void Build(BuildingRuntimeState building, EpochState epoch)
    {
        if (!CanBuild(building, epoch)) return;
        Spend(building.Definition.BuildCost, epoch.Ledger, 0);
        building.IsBuilt = true;
        building.Level = 1;
        _state.NotifyChanged();
    }

    public void Upgrade(BuildingRuntimeState building, EpochState epoch)
    {
        if (!CanUpgrade(building, epoch)) return;
        Spend(building.Definition.BuildCost, epoch.Ledger, building.Level);
        building.Level++;
        _state.NotifyChanged();
    }

    private static bool CanAfford(
        IReadOnlyList<BuildingDefinition.UpgradeCost> costs,
        ResourceLedger ledger,
        int level)
    {
        return costs.All(c =>
            ledger.Get(c.Resource.Id) >= c.BaseCost * Mathf.Pow(c.GrowthFactor, level));
    }

    private static void Spend(
        IReadOnlyList<BuildingDefinition.UpgradeCost> costs,
        ResourceLedger ledger,
        int level)
    {
        foreach (var c in costs)
            ledger.TryConsume(c.Resource.Id, c.BaseCost * Mathf.Pow(c.GrowthFactor, level));
    }
}
```

### 8.2 TechnologyService

```csharp
// Services/TechnologyService.cs
public sealed class TechnologyService
{
    private readonly GameState _state;

    public TechnologyService(GameState state) => _state = state;

    public bool CanResearch(TechnologyDefinition tech, EpochState payingEpoch)
    {
        if (!_state.Tech.CanUnlock(tech)) return false;
        return tech.Cost.All(c => payingEpoch.Ledger.Get(c.Resource.Id) >= c.Amount);
    }

    public void Research(TechnologyDefinition tech, EpochState payingEpoch)
    {
        if (!CanResearch(tech, payingEpoch)) return;

        foreach (var c in tech.Cost)
            payingEpoch.Ledger.TryConsume(c.Resource.Id, c.Amount);

        _state.Tech.Unlock(tech.Id);
        ApplyCrossEpochBonus(tech);
        UnlockChannels(tech);
        _state.NotifyChanged();
    }

    private void ApplyCrossEpochBonus(TechnologyDefinition tech)
    {
        if (tech.ProductionBonus <= 0 || tech.BonusTargetEpoch == null) return;
        // Bonus is read at tick time from TechState — no direct state mutation needed here.
        // EpochSimulator checks tech.ProductionBonus when calculating rates.
    }

    private void UnlockChannels(TechnologyDefinition tech)
    {
        foreach (var channel in _state.CrossEpochChannels)
        {
            if (channel.Definition.UnlockCondition?.Id.Equals(tech.Id) == true)
                channel.IsUnlocked = true;
        }
    }
}
```

### 8.3 GoalService

```csharp
// Services/GoalService.cs
public sealed class GoalService
{
    private readonly GameState _state;
    public event Action OnGoalCompleted;

    public GoalService(GameState state) => _state = state;

    public void CheckGoal()
    {
        if (_state.Goal.IsCompleted) return;

        bool allMet = _state.Goal.Definition.Requirements.All(req =>
        {
            var epoch = _state.GetEpoch(req.Epoch.Id);
            return epoch.Ledger.Get(req.Resource.Id) >= req.Amount;
        });

        if (!allMet) return;

        _state.Goal.IsCompleted = true;
        _state.NotifyChanged();
        OnGoalCompleted?.Invoke();
    }
}
```

---

## 9. Infrastructure Layer — Bootstrap & Runner

### 9.1 GameBootstrap (Composition Root)

```csharp
// Infrastructure/Bootstrap/GameBootstrap.cs
/// <summary>
/// MonoBehaviour entry point. Creates the full object graph and wires all systems.
/// Lives on a persistent GameObject in Bootstrap.unity.
/// </summary>
public sealed class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GameConfig _config;
    [SerializeField] private SimulationRunner _runner;
    [SerializeField] private HudController _hud;

    private GameState _state;
    private SimulationTick _simulationTick;
    private BuildingUpgradeService _buildingUpgrade;
    private TechnologyService _technology;
    private GoalService _goal;

    private void Awake()
    {
        _state = new GameState(_config);

        var priorityResolver = new PriorityResolver();
        var epochSimulator = new EpochSimulator(priorityResolver);
        var crossEpochTransfer = new CrossEpochTransferSystem();
        _simulationTick = new SimulationTick(epochSimulator, crossEpochTransfer);

        _buildingUpgrade = new BuildingUpgradeService(_state);
        _technology = new TechnologyService(_state);
        _goal = new GoalService(_state);

        _goal.OnGoalCompleted += OnGoalCompleted;

        _runner.Initialize(_simulationTick, _state, _config.TickInterval);
        _hud.Initialize(_state, _buildingUpgrade, _technology, _goal);
    }

    private void OnGoalCompleted()
    {
        Debug.Log("[GameBootstrap] Goal completed — show win screen");
        // TODO: load win screen
    }
}
```

### 9.2 SimulationRunner

```csharp
// Infrastructure/Bootstrap/SimulationRunner.cs
/// <summary>
/// Drives the fixed simulation tick using Unity's Update loop.
/// Decoupled from game logic — it only calls SimulationTick.Execute.
/// </summary>
public sealed class SimulationRunner : MonoBehaviour
{
    private SimulationTick _tick;
    private GameState _state;
    private GoalService _goal;
    private float _interval;
    private float _accumulator;
    private bool _initialized;

    public void Initialize(SimulationTick tick, GameState state, float intervalSeconds)
    {
        _tick = tick;
        _state = state;
        _interval = intervalSeconds;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized) return;

        _accumulator += Time.deltaTime;
        while (_accumulator >= _interval)
        {
            _tick.Execute(_state, _interval);
            _accumulator -= _interval;
        }
    }
}
```

---

## 10. UI Layer

### 10.1 HudController

```csharp
// UI/HudController.cs
public sealed class HudController : MonoBehaviour
{
    [SerializeField] private UIDocument _document;
    [SerializeField] private EpochPanelPresenter[] _epochPresenters;
    [SerializeField] private TechPanelPresenter _techPresenter;
    [SerializeField] private CrossEpochPanelPresenter _crossEpochPresenter;
    [SerializeField] private GoalTrackerPresenter _goalPresenter;

    private GameState _state;

    public void Initialize(
        GameState state,
        BuildingUpgradeService upgradeService,
        TechnologyService techService,
        GoalService goalService)
    {
        _state = state;
        _state.OnStateChanged += Refresh;

        var root = _document.rootVisualElement;

        foreach (var presenter in _epochPresenters)
            presenter.Initialize(root, state, upgradeService);

        _techPresenter.Initialize(root, state, techService);
        _crossEpochPresenter.Initialize(root, state);
        _goalPresenter.Initialize(root, state, goalService);

        Refresh();
    }

    private void Refresh()
    {
        foreach (var p in _epochPresenters) p.Refresh();
        _techPresenter.Refresh();
        _crossEpochPresenter.Refresh();
        _goalPresenter.Refresh();
    }

    private void OnDestroy() { if (_state != null) _state.OnStateChanged -= Refresh; }
}
```

### 10.2 EpochPanelPresenter (скелет)

```csharp
// UI/Presenters/EpochPanelPresenter.cs
public sealed class EpochPanelPresenter : MonoBehaviour
{
    [SerializeField] private EpochDefinition _epochDefinition;

    private EpochState _epochState;
    private BuildingUpgradeService _upgradeService;
    private VisualElement _root;

    public void Initialize(VisualElement root, GameState state, BuildingUpgradeService upgradeService)
    {
        _epochState = state.GetEpoch(_epochDefinition.Id);
        _upgradeService = upgradeService;
        _root = root.Q(_epochDefinition.Id.Value + "_panel");
    }

    public void Refresh()
    {
        if (_epochState == null || !_epochState.IsUnlocked) return;

        // Update each resource display: amount / cap / delta
        foreach (var res in _epochState.Definition.Resources)
        {
            var el = _root?.Q(res.Id.Value + "_resource");
            if (el == null) continue;
            el.Q<Label>("amount").text =
                $"{_epochState.Ledger.Get(res.Id):F0} / {_epochState.Ledger.GetCap(res.Id):F0}";
        }

        // Update each building card: level, idle reason, build/upgrade buttons
        foreach (var building in _epochState.Buildings)
        {
            var el = _root?.Q(building.Definition.Id.Value + "_building");
            if (el == null) continue;
            el.Q<Label>("level").text = building.IsBuilt ? $"Lv {building.Level}" : "—";
            el.Q<Label>("idle_reason").text =
                building.IdleReason == IdleReason.None ? "" : building.IdleReason.ToString();
        }
    }
}
```

---

## 11. Тесты — Контрольные точки

### 11.1 ResourceLedger (EditMode)

```csharp
// Tests/EditMode/ResourceLedgerTests.cs
public class ResourceLedgerTests
{
    [Test]
    public void Add_RespectsStorageCap()
    {
        var ledger = new ResourceLedger();
        var id = new ResourceId("wood");
        ledger.SetCap(id, 100f);
        ledger.Add(id, 150f);
        Assert.AreEqual(100f, ledger.Get(id), 0.001f);
    }

    [Test]
    public void TryConsume_ReturnsFalse_WhenInsufficient()
    {
        var ledger = new ResourceLedger();
        var id = new ResourceId("stone");
        ledger.Add(id, 10f);
        Assert.IsFalse(ledger.TryConsume(id, 20f));
        Assert.AreEqual(10f, ledger.Get(id), 0.001f);
    }
}
```

### 11.2 EpochSimulator (EditMode)

```csharp
// Tests/EditMode/EpochSimulatorTests.cs
public class EpochSimulatorTests
{
    // Минимальный тест: здание останавливается при нехватке входного ресурса
    [Test]
    public void Building_StopsWithIdleReason_WhenInputMissing()
    {
        // Arrange: создать minimal EpochState вручную через ScriptableObject.CreateInstance
        // ...
        // Assert: building.IsActive == false, building.IdleReason == MissingInput
    }

    // Тест: здание останавливается при переполнении выходного склада
    [Test]
    public void Building_StopsWithIdleReason_WhenStorageFull() { /* ... */ }

    // Тест: здание производит корректно при наличии входов и места
    [Test]
    public void Building_Produces_WhenConditionsMet() { /* ... */ }
}
```

---

## 12. Пакеты для добавления

В `Packages/manifest.json` нужно добавить:

```json
"com.unity.addressables": "2.3.1"
```

Текущие пакеты уже включают: `New Input System 1.18.0`, `UI Toolkit (UIElements)`, `URP 17.3.0`, `Test Framework 1.6.0`.  
`UGUI 2.0.0` присутствует — для нового UI не использовать (правило проекта).

---

## 13. Первые задачи для старта (Backlog Sprint 0)

| # | Задача | Результат |
|---|---|---|
| 1 | Добавить Addressables в `manifest.json` | Пакет установлен |
| 2 | Создать структуру папок из раздела 3 | Assets готов |
| 3 | Создать 4 `.asmdef` файла | Компилируется без ошибок |
| 4 | Реализовать `ResourceId`, `EpochId`, `BuildingId`, `TechId` | Базовые типы |
| 5 | Реализовать все ScriptableObject definitions (пустые, без логики) | Data layer |
| 6 | Создать asset-файлы для StoneAge (ресурсы, здания, рецепты) | Контент готов |
| 7 | Реализовать `ResourceLedger` + тесты | Хранилище работает |
| 8 | Реализовать `EpochSimulator` + тесты на idle reasons | Симуляция тикает |
| 9 | Реализовать `GameState`, `GameBootstrap`, `SimulationRunner` | Играбельная точка входа |
| 10 | Минимальный debug UI на UI Toolkit (текст, без вёрстки) | Состояние читаемо |

**Milestone:** после задачи 10 первая эпоха тикает и вы видите ресурсы и idle reasons в интерфейсе без красивой вёрстки. Это и есть правильная точка для добавления второй эпохи.
