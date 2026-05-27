# AspectRatioStateSwitcher

A Unity plugin for defining named states and automatically switching between them when the screen aspect ratio changes. Useful for supporting multiple layouts — portrait, landscape, tablet, ultrawide — without writing any switch logic by hand.

---

## How it works

You define states in a **config asset** and assign aspect ratio ranges to each. The **Switcher** monitors the screen every frame and notifies registered **Snapshots** when the active state changes. Each Snapshot is a separate MonoBehaviour component that stores the values of one target component for every state and applies the correct one on switch.

```
AspectStateConfig
  Portrait  [-∞ → 0.75]
  Tall      [0.75 → 1.0]
  Compact   [1.0 → 1.333]
  Landscape [1.333 → 1.78]
  Wide      [1.78 → +∞]

AspectRatioStateSwitcher  ← one per scene
  config → AspectStateConfig

UITransformSnapshot       ← one per object that reacts
  switcher → AspectRatioStateSwitcher  (self-registers on Enable)
  target   → RectTransform
  entries:
    Portrait  Tall → anchoredPos (0, -300), sizeDelta (400, 200)
    Landscape      → anchoredPos (200, 0),  sizeDelta (600, 80)
```

---

## Setup

### 1 — Create a State Config

Right-click in the Project window → **Create → ARSS → Aspect State Config**.

A new asset is pre-populated with the full default state set. Open it to adjust ranges.

Each state has a **name**, an **aspect ratio range**, and a **visibility checkbox** that toggles whether it is shown on the diagram strip.

| State | Min | Max | Covers |
|---|---|---|---|
| `Portrait` | -∞ | 0.75 | Narrow portrait phones (9:16 and narrower) |
| `Tall` | 0.75 | 1.0 | Portrait tablets, wide phones (3:4 – 1:1) |
| `Compact` | 1.0 | 1.333 | Squarish landscape (1:1 – 4:3) |
| `Tablet` | 0.75 | 1.333 | Any tablet ratio — compound of Tall + Compact |
| `PortraitTall` | -∞ | 1.0 | Everything portrait — compound of Portrait + Tall |
| `CompactLandscape` | 1.0 | 1.78 | Not portrait, not ultra-wide — compound of Compact + Landscape |
| `Landscape` | 1.333 | 1.78 | Standard landscape (4:3 – 16:9) |
| `Wide` | 1.78 | +∞ | Ultra-wide, desktop (16:9+) |

**Precise states** (Portrait, Tall, Compact, Landscape, Wide) form non-overlapping bands. **Compound states** (Tablet, PortraitTall, CompactLandscape) span multiple bands — use them in snapshots when several adjacent states should share the same data.

The range diagram at the top of the asset inspector shows all visible states at a glance. Uncheck the checkbox next to a state to hide it from the diagram without removing it from the config.

> **Priority:** if ranges overlap, the first matching state in the list wins. Compound states placed below precise states get picked up when no precise entry exists in a snapshot.

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

The inspector also shows all registered snapshots grouped by type, with a button to ping each one.

**Preview State** buttons apply snapshots in Edit Mode without entering Play Mode. Clicking a state activates it along with all states whose ranges are fully contained within it — e.g. clicking `CompactLandscape` also applies `Compact` and `Landscape` entries.

---

### 3 — Add a Snapshot component to each reacting object

Add a snapshot component (e.g. **UITransformSnapshot**) to a GameObject that should change its appearance or values on aspect ratio switch. Find them via **Add Component → Aspect Switcher → Snapshots**.

| Field | Description |
|-------|-------------|
| **Switcher** | The `AspectRatioStateSwitcher` in the scene — the snapshot self-registers on Enable |
| **Target** | Component to read from / write to (auto-filled on Add Component) |
| **Transition Override** | Per-snapshot transition that overrides the global one |
| **State Entries** | One or more entries — each stores snapshot data for one or more states |

**To set up entries:**
1. Set the **Switcher** reference and confirm the **Target** is correct.
2. Click **+ Add Entry**. The entry shows a state dropdown and a **+ State** button.
3. Add as many states as needed to the entry — all of them will use the same data.
4. Position or configure the object as it should look in those states.
5. Click **Capture** on the entry — this saves the current values.
6. Repeat for each distinct visual state.
7. Use **Preview** to apply a single entry in Edit Mode. **Capture All** saves all entries at once.

> There is no manual list to maintain. A snapshot self-registers with its Switcher in `OnEnable` and unregisters in `OnDisable`.

---

## Multiple states per entry

An entry can map several states to the same data, avoiding duplicate entries when adjacent states should look identical:

```
Entry 1:  Portrait  Tall  →  anchoredPos (0, -300)
Entry 2:  Compact         →  anchoredPos (100, 0)
Entry 3:  Landscape Wide  →  anchoredPos (200, 0)
```

Alternatively, use a compound state (`PortraitTall`, `Tablet`, `CompactLandscape`) as a single entry that covers a broader range without listing individual states.

---

## Matching logic

When the aspect ratio changes the Switcher builds the list of **all** config states whose ranges include the current aspect, in config priority order. Each snapshot finds the first state from that list for which it has an entry and applies it. If the resolved state has not changed since the last switch, the snapshot does nothing.

This means a snapshot with an entry for `Landscape` will correctly activate even if a compound state like `CompactLandscape` is first in the config — the Switcher passes the full list and the snapshot finds `Landscape` as its first match.

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
    Portrait         = 0,
    Tall             = 1,
    Compact          = 2,
    PortraitTall     = 9,
    Tablet           = 10,
    CompactLandscape = 11,
    Landscape        = 20,
    Wide             = 30,
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

// Force a specific state (also triggers all registered snapshots)
AspectRatioStateSwitcher.Instance.ForceState(AspectState.Landscape);
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
