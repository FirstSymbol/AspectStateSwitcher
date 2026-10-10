# Aspect Ratio Switcher

Responsive layouts for Unity by aspect ratio. Describe aspect states once in a profile, then let objects store
per-state values (position, size, layout settings, text size, active state...) and switch between them
automatically — with tweened transitions, Edit Mode preview and a record mode that captures your edits.

- **Data-driven states** — named states with colors and ranges in an `AspectProfile` asset; no code changes to add one.
- **Smart matching** — states may overlap; the narrowest matching state wins and broader states are fallbacks.
- **One component per object** — `AspectResponsive` holds any number of modules; each module stores a value per state.
- **Works with prefabs** — components connect to the switcher in their parents or to the main switcher automatically.
- **Editor tooling** — profile diagram with draggable ranges, state chips, capture/apply, revertible scene preview,
  REC mode, Scene View overlay, overview matrix.
- No third-party dependencies (uGUI and TextMeshPro from `com.unity.ugui`).

## Installation

Package Manager → *Add package from git URL* → the repository URL, or copy the folder into `Assets/` or `Packages/`.

## Quick start

1. **Create a profile** — *Create → Aspect Ratio Switcher → Aspect Profile*. It starts with a default set of states:

   | State | Range | Meaning |
   | --- | --- | --- |
   | Portrait | < 1 | any portrait screen (phones 9:16, 9:19.5...) |
   | Landscape | ≥ 1 | any landscape screen (16:9, 21:9...) |
   | 4:3 | 1.15 – 1.45 | tablets in landscape |
   | 3:4 | 0.7 – 0.87 | tablets in portrait |
   | 1:1 | 0.87 – 1.15 | square-ish screens, foldables |

   Portrait and Landscape are broad defaults; the precise 4:3, 3:4 and 1:1 bands win where they match
   (their bounds are the geometric middle between neighbouring ratios). Add, rename or recolor states freely.

2. **Add an `AspectSwitcher`** to a scene object (usually the Canvas) and assign the profile.
3. **Add `AspectResponsive`** to an object, press **+ Add Module** (modules for components on the object are suggested).
4. **Select a state chip, arrange the object, press Capture** (or turn on **● REC** and just edit — every change of a
   module target is stored in the selected state). Repeat for other states.
5. Press **Play** and resize the Game view, or use **Preview** in the switcher / Scene View overlay in Edit Mode.

A demo scene lives in this repository's host project (`Assets/Scenes/AspectSwitcherDemo.unity`): a header, a sidebar
shown only in landscape, a bottom navigation shown only in portrait and a card grid whose columns and card sizes change
for Portrait, 3:4, 1:1, Landscape and 4:3. The title only has Portrait and Landscape values and shows the fallback.

## Matching rules

The active states for an aspect are all states whose range contains it, ordered from the most specific:
a range contained in another one wins, otherwise the narrower range wins, otherwise profile order.
Each module uses the **first active state it has a value for**. So with values for *Landscape* and *4:3*,
a 4:3 tablet uses *4:3* and a 16:9 screen uses *Landscape*. A module without a value for any active state
leaves its target untouched.

Ranges are half-open: `min ≤ aspect < max` where aspect = width / height.

## Components

### AspectSwitcher

| Setting | Description |
| --- | --- |
| Profile | States to resolve. |
| Source | `Screen`, `Camera` (pixel rect, `Camera.main` by default) or `RectTransform` (e.g. a split-screen panel). |
| Stabilization Time | Seconds the aspect must stay unchanged before switching (avoids flicker while resizing). |
| Default Transition | `Instant` or `Tween` (duration, curve, unscaled time). |
| Is Main | Responsive objects without a switcher in their parents use the main switcher. |
| On State Changed | UnityEvent with the name of the most specific active state. |

```csharp
switcher.ForceState("4:3");           // by name or id, plus broader fallbacks
switcher.ClearForcedState();
switcher.Refresh();                   // re-measure and re-apply everything
switcher.StateChanged += s => Debug.Log(s.PrimaryState?.name);
IReadOnlyList<AspectStateDefinition> active = switcher.ActiveStates;
AspectSwitcher.Main;                  // current main switcher
```

### AspectResponsive

Connects to: the assigned switcher → the nearest enabled switcher in parents → `AspectSwitcher.Main`.
When enabled it applies the current state immediately, so inactive objects and runtime-spawned prefabs are
always correct. Transition: inherit the switcher's, instant, or a custom one.

```csharp
var responsive = gameObject.AddComponent<AspectResponsive>();
var color = responsive.AddModule<GraphicColorModule>();
color.SetValue(profile.Find("Portrait").Id, Color.red);
color.SetValue(profile.Find("Landscape").Id, Color.green);
```

## Modules

| Category | Module | Interpolated |
| --- | --- | --- |
| Transform | Rect Transform (choose properties), Transform, Sibling Index, Parent | position / size / anchors / rotation / scale |
| Object | GameObject Active, Component Enabled (Behaviour / Renderer / Collider), Animator Parameter, State Events (UnityEvent) | animator floats |
| UI | Canvas Group, Graphic Color, Image, TextMeshPro Text | alpha, color, sizes, margins |
| Layout | Horizontal/Vertical Layout Group, Grid Layout Group, Layout Element, Content Size Fitter, Aspect Ratio Fitter, Canvas Scaler | spacing, padding, cell size, sizes, ratio, scale |

Discrete values (booleans, enums, references) switch at the start of a transition.

> **GameObject Active** on the object that holds the `AspectResponsive` would switch the component off together
> with the object — put the component on a parent instead (the inspector warns about it).

### Custom modules

```csharp
[Serializable, AspectModuleMenu("Custom/Outline Distance")]
public sealed class OutlineModule : AspectModule<Outline, Vector2>
{
    public override bool SupportsInterpolation => true;
    protected override Vector2 Read(Outline target) => target.effectDistance;
    protected override void Write(Outline target, Vector2 value) => target.effectDistance = value;
    protected override Vector2 Interpolate(Vector2 a, Vector2 b, float t) => Vector2.LerpUnclamped(a, b, t);
}
```

Any serializable subclass appears in the *Add Module* menu.

## Editor tools

- **Profile inspector** — diagram with two views (toggle in its corner, remembered):
  *Screens* draws every state as a screen thumbnail of its typical proportions with outlines of its narrowest and
  widest screens, plus the current Game view; *Bars* shows ranges on an aspect axis (overlaps on separate lanes),
  where the edges of the selected state can be dragged. Reorderable list and validation below.
- **Switcher inspector** — live aspect and active state, preview buttons (Edit Mode: applied to every responsive
  object using the profile, **Revert preview** restores the exact previous values; Play Mode: forces the state),
  list of connected objects.
- **Responsive inspector** — state chips (● = has values), Capture all / Apply / Preview scene / Clear, **● REC**,
  module cards with target, settings, the value of the selected state and per-module Capture / Apply.
- **Scene View overlay** *Aspect Preview* — `Window > Aspect Ratio Switcher > Scene View Preview Overlay`.
- **Overview** — `Window > Aspect Ratio Switcher > Overview`: objects × states matrix
  (● values, ○ uses a broader state, · nothing).

## Upgrading from 1.x

See [MIGRATION.md](MIGRATION.md).
