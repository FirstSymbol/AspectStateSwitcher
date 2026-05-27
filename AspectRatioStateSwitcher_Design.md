# AspectRatioStateSwitcher — Дизайн плагина Unity

## Обзор

Плагин позволяет задавать несколько именованных состояний сцены и автоматически переключаться между ними при изменении соотношения сторон экрана (aspect ratio). Состояния описываются пользовательским `enum`. Каждый объект, который должен реагировать на смену состояния, получает компонент-контейнер `AspectSnapshotContainer`, хранящий снимки для всех состояний сразу. Главный контроллер отслеживает aspect ratio и рассылает событие смены состояния — контейнеры применяют нужный снимок.

---

## Архитектура

```
[User code]
enum AspectStateId { Default, Portrait, Tablet, UltraWide }

AspectRatioStateSwitcher (MonoBehaviour)  ← один на сцену
│
├── AspectRatioMonitor                    ← отслеживает изменение aspect ratio
├── List<AspectStateBinding>              ← привязка enum-значения к диапазону
│   ├── { Default,   range: [1.0 → +∞] }
│   ├── { Portrait,  range: [-∞ → 1.0] }
│   └── { Tablet,    range: [0.75 → 1.33] }
├── TransitionSettings globalTransition
└── event Action<AspectStateId> OnStateChanged  ← broadcast для контейнеров

AspectSnapshotContainer (MonoBehaviour)   ← вешается на объект, которым управляют
│
├── List<StateEntry> entries
│   ├── { state: Default,  data: UITransformData { anchorMin, anchorMax, pos, size } }
│   ├── { state: Portrait, data: UITransformData { ... } }
│   └── { state: Tablet,   data: UITransformData { ... } }
└── SnapshotType type  ← UITransform | Transform | ComponentField | AnimatorParam | CanvasGroup
```

> Если объекту нужно менять и позицию, и параметр скрипта — вешаются два контейнера разного типа. Это явно и читаемо.

---

## Ядро данных

### AspectStateId (enum, определяется пользователем)

Пользователь объявляет enum один раз в своём проекте. Плагин работает с любым enum через generic-параметр или через `System.Enum` с рефлексией в Editor.

```csharp
// Пример в проекте пользователя:
public enum AspectStateId
{
    Default,
    Portrait,
    Tablet,
    UltraWide
}
```

Преимущества: type safety, autocomplete, невозможно ошибиться со строкой. Добавление нового состояния = добавление одного значения в enum + перекомпиляция.

---

### AspectRange

Диапазон aspect ratio, при котором состояние активно.

```
AspectRange
├── float min    // нижняя граница (включительно), float.MinValue если не задана
├── float max    // верхняя граница (исключительно), float.MaxValue если не задана
└── bool Matches(float aspect) → bool
```

Примеры диапазонов:
| Описание           | min    | max    |
|--------------------|--------|--------|
| landscape (≥ 1.0)  | 1.0    | +∞     |
| portrait  (< 1.0)  | -∞     | 1.0    |
| square-ish         | 0.9    | 1.1    |
| ultra-wide         | 2.0    | +∞     |
| narrow phone       | -∞     | 0.56   |

---

### AspectStateBinding

Один элемент конфигурации контроллера — связывает значение enum с диапазоном.

```
AspectStateBinding
├── AspectStateId state
├── AspectRange range
└── TransitionSettings transition  // переопределяет глобальные настройки для этого перехода
```

---

### StateEntry

Один элемент внутри `AspectSnapshotContainer` — снимок для конкретного состояния.

```
StateEntry
├── AspectStateId state   // для какого состояния этот снимок
└── ISnapshotData data    // данные (тип зависит от SnapshotType контейнера)
```

---

### ISnapshotData (интерфейс данных снимка)

```
ISnapshotData
├── void CaptureFrom(Component target)      // записать текущие значения
└── void ApplyTo(Component target, float t) // применить (t=0..1 для Lerp)
```

Конкретные реализации:

#### UITransformData
`RectTransform`: anchorMin, anchorMax, anchoredPosition, sizeDelta, pivot.

#### TransformData
`Transform`: localPosition, localRotation, localScale.

#### ComponentFieldData
Произвольное поле компонента. Поле задаётся через `SerializedProperty` в Editor и кешируется через рефлексию в runtime.

```
ComponentFieldData
├── string fieldPath          // "speed", "_radius" и т.д.
├── SerializedValueWrapper value
└── bool interpolatable
```

#### AnimatorParamData
Параметр `Animator`: имя параметра + значение (float / int / bool).

#### CanvasGroupData
`CanvasGroup`: alpha, interactable, blocksRaycasts.

---

### TransitionSettings

```
TransitionSettings
├── TransitionMode mode   // Instant | Lerp | AnimationCurve
├── float duration
├── AnimationCurve curve  // опционально, для режима AnimationCurve
└── UnityEvent onComplete
```

---

## Компоненты (MonoBehaviour)

### AspectRatioStateSwitcher

Главный компонент, один на сцену.

**Поля (Inspector):**
- `List<AspectStateBinding> bindings` — список привязок enum → диапазон
- `TransitionSettings globalTransition`
- `float checkInterval` — частота проверки aspect (0 = каждый кадр)
- `bool applyOnStart`
- `UnityEvent<int> onStateChanged` — передаёт int-значение enum для совместимости с UnityEvent

**Логика:**
1. В `Update` (или по таймеру) вычисляет `aspect = (float)Screen.width / Screen.height`
2. Находит первый `AspectStateBinding`, чей `range.Matches(aspect) == true`
3. Если найденное состояние отличается от текущего — рассылает `OnStateChanged`
4. Приоритет при перекрытии диапазонов — порядок в списке

---

### AspectSnapshotContainer

Вешается на объект, которым нужно управлять.

**Поля (Inspector):**
- `SnapshotType type` — тип снимка (UITransform / Transform / ComponentField / ...)
- `Component target` — целевой компонент (заполняется автоматически по типу)
- `List<StateEntry> entries` — по одной записи на каждое нужное состояние
- `TransitionSettings transitionOverride` — локальное переопределение перехода

**Логика:**
1. В `Awake` подписывается на `AspectRatioStateSwitcher.OnStateChanged`
2. При получении события ищет `StateEntry` с совпадающим `state`
3. Если нашёл — вызывает `data.ApplyTo(target, t)` (с Lerp-корутиной при необходимости)
4. Если для текущего состояния записи нет — остаётся в последнем применённом

---

### AspectRatioMonitor (вспомогательный, static)

Централизованный опрос, исключает дублирование из нескольких контроллеров на сцене.

```
AspectRatioMonitor
├── float CurrentAspect
├── event Action<float> OnAspectChanged
└── float threshold = 0.01f
```

---

## Editor-инструменты

### Custom Inspector для AspectRatioStateSwitcher

- Список `AspectStateBinding` с Enum-дропдауном и редактором диапазона (два float-поля)
- Диаграмма диапазонов — горизонтальная полоса по оси aspect ratio, визуально показывает перекрытия и пробелы:

```
0.25  0.5   0.75  1.0   1.5   2.0   3.0
 |---Portrait----|----Default----|--Wide--|
```

- Кнопка **"Preview State"** — применяет выбранное состояние в Editor без Play Mode

### Custom Inspector для AspectSnapshotContainer

- Дропдаун `SnapshotType` вверху — определяет тип данных всех записей
- Список `StateEntry`: каждая строка — Enum-дропдаун (состояние) + инлайн-поля данных снимка
- Кнопка **"Capture"** рядом с каждой записью — записывает текущие значения объекта в этот снимок
- Кнопка **"Capture All"** — обновляет все записи разом
- Кнопка **"Preview"** рядом с записью — применяет снимок в Editor

### ContextMenu на компонентах

Правая кнопка мыши на компоненте в Inspector → **"Add AspectSnapshotContainer"** → выбирает подходящий `SnapshotType` автоматически и добавляет компонент на тот же GameObject.

---

## Файловая структура плагина

```
Assets/
└── AspectRatioStateSwitcher/
    ├── Runtime/
    │   ├── AspectRatioStateSwitcher.cs
    │   ├── AspectSnapshotContainer.cs
    │   ├── AspectRatioMonitor.cs
    │   └── Data/
    │       ├── AspectRange.cs
    │       ├── AspectStateBinding.cs
    │       ├── StateEntry.cs
    │       ├── TransitionSettings.cs
    │       └── Snapshots/
    │           ├── ISnapshotData.cs
    │           ├── UITransformData.cs
    │           ├── TransformData.cs
    │           ├── ComponentFieldData.cs
    │           ├── AnimatorParamData.cs
    │           └── CanvasGroupData.cs
    ├── Editor/
    │   ├── AspectRatioStateSwitcherEditor.cs
    │   ├── AspectSnapshotContainerEditor.cs
    │   ├── AspectRangeDrawer.cs
    │   └── AspectRangeDiagram.cs
    └── Samples/
        ├── BasicUISwitch/
        └── ComponentParamsSwitch/
```

---

## Пример использования

### Пример 1 — UI позиции (landscape / portrait)

```
// Enum в проекте
enum AspectStateId { Default, Portrait }

// AspectRatioStateSwitcher (на UIManager)
bindings:
  Default  → range [1.0 → +∞]
  Portrait → range [-∞ → 1.0]

// AspectSnapshotContainer на объекте Panel
type: UITransform
entries:
  Default  → anchoredPos(0, 0),   anchors(center-center)
  Portrait → anchoredPos(0, -300), anchors(top-center)

// AspectSnapshotContainer на объекте Button
type: UITransform
entries:
  Default  → anchoredPos(200, 0)
  Portrait → anchoredPos(0, -500)
```

### Пример 2 — параметры скриптов

```
// AspectSnapshotContainer на объекте EnemySpawner
type: ComponentField, target: EnemySpawner, fieldPath: "spawnRate"
entries:
  Default  → 1.0
  Portrait → 0.5

// AspectSnapshotContainer на объекте PlayerMovement
type: ComponentField, target: PlayerMovement, fieldPath: "speed"
entries:
  Default  → 6.0
  Portrait → 4.0
```

### Пример 3 — несколько диапазонов

```
enum AspectStateId { Narrow, Portrait, Landscape, Wide }

bindings:
  Narrow    → [-∞   → 0.6 ]
  Portrait  → [0.6  → 1.0 ]
  Landscape → [1.0  → 1.78]
  Wide      → [1.78 → +∞  ]
```

---

## Расширяемость

- **Кастомный тип снимка**: реализовать `ISnapshotData` — плагин покажет его в дропдауне `SnapshotType`
- **Кастомный переход**: реализовать `ITransitionHandler` и зарегистрировать через атрибут `[TransitionHandler]`
- **События**: `onStateChanged(int)` позволяет вешать логику через Inspector; в коде — типизированное `OnStateChanged` через `Action<AspectStateId>`

---

## Ограничения и допущения

- `ComponentFieldData` использует рефлексию — в IL2CPP-сборках нужен атрибут `[Preserve]` на отслеживаемых полях
- Lerp-переход работает только для interpolatable-типов (float, int, Vector, Color); для остальных — мгновенная замена
- Диапазоны могут перекрываться; приоритет — порядок в списке `bindings` (первый подходящий выигрывает)
- Enum определяется пользователем; при смене имён значений enum существующие `StateEntry` теряют привязку (защита — не переименовывать значения, только добавлять новые)
