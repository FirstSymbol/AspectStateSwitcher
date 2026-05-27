# AspectRatioStateSwitcher

A Unity plugin for defining named states and automatically switching between them when the screen aspect ratio changes. Useful for supporting multiple layouts — portrait, landscape, tablet, ultrawide — without writing any switch logic by hand.

---

## How it works

You define states in a **config asset** (e.g. `Portrait`, `Landscape`, `Wide`) and assign aspect ratio ranges to each. The **Switcher** checks the screen every frame and notifies a list of **Containers** when the active state changes. Each Container stores a snapshot of a component's values for every state and applies the correct one on switch.

```
AspectStateConfig
  Portrait   [-∞ → 0.75]
  Landscape  [0.75 → 1.78]
  Wide       [1.78 → +∞]

AspectRatioStateSwitcher  ← one per scene
  config  → AspectStateConfig
  targets → [ PanelContainer, ButtonContainer, ... ]

AspectSnapshotContainer  ← one per object that reacts
  config  → AspectStateConfig  (same asset)
  type    → UITransform
  entries:
    Portrait  → anchoredPos (0, -300), sizeDelta (400, 200)
    Landscape → anchoredPos (200, 0),  sizeDelta (600, 80)
```

---

## Setup

### 1 — Create a State Config

Right-click in the Project window → **Create → ARSS → Aspect State Config**.

Open the asset and add your states. Each state has a **name** and an **aspect ratio range**:

| State     | Min   | Max   | Typical device         |
|-----------|-------|-------|------------------------|
| Portrait  | -∞    | 0.75  | Phone held vertically  |
| Landscape | 0.75  | 1.78  | Tablet, most phones    |
| Wide      | 1.78  | +∞    | Desktop, ultrawide     |

The range diagram at the top of the asset inspector shows overlaps and gaps at a glance. Click **∞** to toggle a bound to infinity.

> **Priority:** if ranges overlap, the first matching state wins (top of the list).

---

### 2 — Add AspectRatioStateSwitcher to the scene

Add the component to any persistent GameObject (e.g. UIManager).

| Field | Description |
|-------|-------------|
| **State Config** | The config asset you created |
| **Controlled Containers** | Drag `AspectSnapshotContainer` components here |
| **Global Transition** | Default transition applied to all containers |
| **Check Interval** | Seconds between checks (`0` = every frame) |
| **Apply On Start** | Immediately apply the correct state on scene load |
| **On State Changed** | UnityEvent fired with the new state name |

---

### 3 — Add AspectSnapshotContainer to each reacting object

Add the component to a GameObject that should change its appearance or values on aspect ratio switch.

| Field | Description |
|-------|-------------|
| **State Config** | Must be the same asset as on the Switcher |
| **Type** | What kind of data to snapshot (see types below) |
| **Target** | Component to read from / write to (auto-filled on type change) |
| **Transition Override** | Per-container transition that overrides the global one |
| **State Entries** | One entry per state — each stores a snapshot of the target |

**To set up entries:**
1. Set **Type** and confirm the **Target** is correct.
2. Click **+ Add Entry**, select the state from the dropdown.
3. Position or configure the object as it should look in that state.
4. Click **Capture** on the entry — this saves the current values.
5. Repeat for each state.
6. Use **Preview** to apply a snapshot in Edit Mode without entering Play Mode.
7. **Capture All** / **Preview State** (on Switcher) operate all entries/containers at once.

> **Don't forget:** drag this container into the Switcher's **Controlled Containers** list.

---

## Snapshot types

| Type | Target component | What is stored | Lerp support |
|------|-----------------|----------------|--------------|
| **UITransform** | `RectTransform` | anchoredPosition, sizeDelta, anchorMin/Max, pivot | Position + size |
| **Transform** | `Transform` | localPosition, localRotation, localScale | Full |
| **CanvasGroup** | `CanvasGroup` | alpha, interactable, blocksRaycasts | Alpha only |
| **AnimatorParam** | `Animator` | float / int / bool parameter | Float only |
| **ComponentField** | Any `Component` | One public or private field (float/int/bool/string) | float + int |

> **UITransform note:** anchors and pivot are applied instantly even in Lerp mode. Lerping anchors while simultaneously lerping `anchoredPosition` causes layout feedback and visual jumps.

> **ComponentField note:** uses reflection at runtime. In IL2CPP builds, mark the target field with `[Preserve]`.

---

## Transitions

Set `mode` in **Global Transition** (on Switcher) or **Transition Override** (on Container):

| Mode | Behaviour |
|------|-----------|
| **Instant** | Values snap immediately on state change (default) |
| **Lerp** | Linearly interpolates from the current value to the target over `Duration` seconds |
| **AnimationCurve** | Same as Lerp but `t` is remapped through the curve |

The override on a Container takes effect only when its `mode` is not `Instant`. Otherwise the global setting is used.

If a new state arrives while a transition is already running, the current (mid-transition) values are captured as the new starting point, so there is no pop.

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

Add or remove values here to match your layout needs. After editing the enum, open the `AspectStateConfig` asset — new values are added automatically with a default full range.

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

// Force a state manually (also triggers all containers)
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

Each `AspectRatioStateSwitcher` manages only the containers in its own **Controlled Containers** list. You can have multiple switchers in one scene (e.g. one per canvas), each with a different config, as long as the static `CurrentState` and `Instance` properties are not relied upon across them.

---

## Requirements

- Unity 2021.3 or later (uses `[SerializeReference]` and C# 8 switch expressions)
- No external dependencies

---

## Limitations

- `ComponentField` supports `float`, `int`, `bool`, `string` fields only. Properties (getters/setters) are not supported.
- Lerp on `UITransform` animates `anchoredPosition` and `sizeDelta` only. Anchor changes are instant.
- Adding new states requires editing `AspectState.cs` — you cannot add states at runtime or from the Inspector without a code change.
- One `AspectStateConfig` should be shared between the Switcher and all of its Containers.
