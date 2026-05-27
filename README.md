# AspectRatioStateSwitcher

A Unity plugin for defining named states and automatically switching between them when the screen aspect ratio changes. Useful for supporting multiple layouts — portrait, landscape, tablet, ultrawide — without writing any switch logic by hand.

---

## How it works

You define states in a **config asset** (e.g. `Portrait`, `Landscape`, `Wide`) and assign aspect ratio ranges to each. The **Switcher** monitors the screen every frame and notifies registered **Snapshots** when the active state changes. Each Snapshot is a separate MonoBehaviour component that stores the values of one target component for every state and applies the correct one on switch.

```
AspectStateConfig
  Portrait   [-∞ → 1]
  Landscape  [1 → 1.78]
  Wide       [1.78 → +∞]

AspectRatioStateSwitcher  ← one per scene
  config → AspectStateConfig

UITransformSnapshot       ← one per object that reacts
  switcher → AspectRatioStateSwitcher  (self-registers on Enable)
  target   → RectTransform
  entries:
    Portrait  → anchoredPos (0, -300), sizeDelta (400, 200)
    Landscape → anchoredPos (200, 0),  sizeDelta (600, 80)
```

---

## Setup

### 1 — Create a State Config

Right-click in the Project window → **Create → ARSS → Aspect State Config**.

Open the asset and add your states. Each state has a **name** and an **aspect ratio range**:

| State             | Min  | Max  | Typical device                   |
|-------------------|------|------|----------------------------------|
| Portrait          | -∞   | 0.75 | Phone held vertically, -∞ - 9:16 |
| PortraitToSquare  | 0.75 | 1    | Tablet, 3:4 - 1:1                |
| SquareToSuper     | 1    | 1.33 | Tablet, 1:1 - 4:3                |
| Super             | 0.75 | 1.33 | Tablet, 3:4 - 4:3                |
| SquareToLandscape | 1    | 1.78 | Tablet, most phones, 1:1 - 16:9  |
| Landscape         | 1.33 | 1.78 | Tablet, most phones, 4:3 - 16:9  |
| Wide              | 1.78 | +∞   | Desktop, ultrawide, 16:9 - +∞    |

The range diagram at the top of the asset inspector shows overlaps and gaps at a glance.

> **Priority:** if ranges overlap, the first matching state wins (top of the list).

---

### 2 — Add AspectRatioStateSwitcher to the scene

Add the component to any persistent GameObject (e.g. UIManager).

| Field | Description |
|-------|-------------|
| **State Config** | The config asset you created |
| **Global Transition** | Default transition applied to all snapshots |
| **State Stabilization** | Seconds the new state must hold before switching (prevents flicker at boundaries) |
| **Apply On Start** | Immediately apply the correct state on scene load |
| **On State Changed** | UnityEvent fired with the new `AspectState` value |

The inspector also shows all registered snapshots grouped by type, with a button to select each one.

---

### 3 — Add a Snapshot component to each reacting object

Add a snapshot component (e.g. **UITransformSnapshot**) to a GameObject that should change its appearance or values on aspect ratio switch. Find them via **Add Component → Aspect Switcher → Snapshots**.

| Field | Description |
|-------|-------------|
| **Switcher** | The `AspectRatioStateSwitcher` in the scene — the snapshot self-registers on Enable |
| **Target** | Component to read from / write to (auto-filled on Add Component) |
| **Transition Override** | Per-snapshot transition that overrides the global one |
| **State Entries** | One entry per state — each stores a snapshot of the target |

**To set up entries:**
1. Set the **Switcher** reference and confirm the **Target** is correct.
2. Click **+ Add Entry**, select the state from the dropdown.
3. Position or configure the object as it should look in that state.
4. Click **Capture** on the entry — this saves the current values.
5. Repeat for each state.
6. Use **Preview** to apply a snapshot in Edit Mode without entering Play Mode.
7. **Capture All** (on Snapshot) and **Preview State** (on Switcher) operate all entries at once.

> There is no manual list to maintain. A snapshot self-registers with its Switcher in `OnEnable` and unregisters in `OnDisable`.

---

## Snapshot types

| Component | Target | What is stored | Lerp support |
|-----------|--------|----------------|--------------|
| **UITransformSnapshot** | `RectTransform` | anchoredPosition, sizeDelta, anchorMin/Max, pivot | Position + size |
| **TransformSnapshot** | `Transform` | localPosition, localRotation, localScale | Full |
| **CanvasGroupSnapshot** | `CanvasGroup` | alpha, interactable, blocksRaycasts | Alpha only |
| **AnimatorParamSnapshot** | `Animator` | float / int / bool parameter | Float only |
| **ComponentFieldSnapshot** | Any `Component` | One public or private field (float/int/bool/string) | float + int |

> **UITransform note:** anchors and pivot are applied instantly even in Lerp mode. Lerping anchors while simultaneously lerping `anchoredPosition` causes layout feedback and visual jumps.

> **ComponentField note:** uses reflection at runtime. In IL2CPP builds, mark the target field with `[Preserve]`.

---

## Transitions

Set `mode` in **Global Transition** (on Switcher) or **Transition Override** (on Snapshot):

| Mode | Behaviour |
|------|-----------|
| **Instant** | Values snap immediately on state change (default) |
| **Lerp** | Linearly interpolates from the current value to the target over `Duration` seconds |
| **AnimationCurve** | Same as Lerp but `t` is remapped through the curve |

The override on a Snapshot takes effect only when its `mode` is not `Instant`. Otherwise the global setting is used.

If a new state arrives while a transition is already running, the current (mid-transition) values are captured as the new starting point — no pop.

---

## States enum

States are defined in `AspectState.cs` (inside the plugin's Runtime folder):

```csharp
public enum AspectState
{
    Portrait,
    Landscape,
    Wide,
}
```

Add or remove values here to match your layout needs. After editing the enum, open the `AspectStateConfig` asset and add corresponding range entries.

---

## Code API

```csharp
// Subscribe to state changes from code
AspectRatioStateSwitcher.OnStateChanged += OnStateChanged;

private void OnStateChanged(AspectState state)
{
    if (state == AspectState.Portrait) { /* ... */ }
}

// Read current state at any time (null before first evaluation)
AspectState? current = AspectRatioStateSwitcher.CurrentState;

// Force a state manually (also triggers all registered snapshots)
AspectRatioStateSwitcher.Instance.ForceState(AspectState.Portrait);
```

> `OnStateChanged` is a **static** event. Unsubscribe in `OnDestroy` to avoid memory leaks:
> ```csharp
> private void OnDestroy()
> {
>     AspectRatioStateSwitcher.OnStateChanged -= OnStateChanged;
> }
> ```

---

## Multiple switchers

Each `AspectRatioStateSwitcher` manages only the snapshots registered to it via their **Switcher** field. You can have multiple switchers in one scene (e.g. one per canvas), each with a different config, as long as the static `CurrentState` and `Instance` properties are not relied upon across them.

---

## Requirements

- Unity 6000.4.5f1 or later
- No external dependencies

---

## Limitations

- `ComponentFieldSnapshot` supports `float`, `int`, `bool`, `string` fields only. Properties (getters/setters) are not supported.
- Lerp on `UITransformSnapshot` animates `anchoredPosition` and `sizeDelta` only. Anchor changes are instant.
- Adding new states requires editing `AspectState.cs` — you cannot add states at runtime or from the Inspector without a code change.
- One `AspectStateConfig` should be shared between the Switcher and all of its Snapshots.
