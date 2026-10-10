# Migrating from 1.x to 2.0

2.0 is a full rewrite. Serialized data of 1.x components cannot be loaded by 2.0: remove the old components
before updating (or re-create the layouts afterwards — the editor tools make it quick).

## Concepts

| 1.x | 2.0 |
| --- | --- |
| `AspectState` enum (code change to add a state) | `AspectStateDefinition` entries in the profile (name, color, range, stable id) |
| `AspectStateConfig` | `AspectProfile` (*Create → Aspect Ratio Switcher → Aspect Profile*) |
| `AspectRatioStateSwitcher` (+ static `Instance`, `CurrentState`, `OnStateChanged`) | `AspectSwitcher` (`Main`, `ActiveStates`, `PrimaryState`, `StateChanged`) |
| `AspectRatioMonitor` + camera context menu | `AspectSwitcher.Source` (Screen / Camera / RectTransform) |
| First matching state in config order | Most specific matching state; broader states are fallbacks |
| One snapshot component per data type, entries with state lists | One `AspectResponsive` with modules, one value per state |
| `TransitionSettings` (Instant / Lerp / AnimationCurve) | `TransitionSettings` (Instant / Tween with curve); per object: Inherit / Instant / Custom |
| `ForceState(enum)` | `ForceState(nameOrId)` / `ClearForcedState()` |
| Namespaces `AspectSwitcher`, `UnityEngine.UI.Snapshots` | `FirstSymbol.AspectRatioSwitcher`, `FirstSymbol.AspectRatioSwitcher.Modules` |
| Assemblies `AspectRatioSwitcher.Runtime/Editor` | `FirstSymbol.AspectRatioSwitcher` (+ `.Editor`) |

## Snapshots → modules

| 1.x snapshot | 2.0 module |
| --- | --- |
| UITransformSnapshot | Rect Transform |
| TransformSnapshot | Transform |
| GameObjectActiveSnapshot | GameObject Active |
| ComponentEnabledSnapshot | Component Enabled |
| CanvasGroupSnapshot | Canvas Group |
| ImageSnapshot | Image (+ Graphic Color for color) |
| Horizontal/Vertical/HorizontalOrVertical layout snapshots | Horizontal or Vertical Layout Group |
| GridLayoutSnapshot | Grid Layout Group |
| LayoutElementSnapshot | Layout Element |
| ContentSizeFitterSnapshot | Content Size Fitter |
| AspectRatioFitterSnapshot | Aspect Ratio Fitter |
| CanvasScalerSnapshot | Canvas Scaler |
| CanvasSnapshot | Component Enabled (Canvas) or State Events |
| SiblingIndexSnapshot | Sibling Index |
| ReparentSnapshot | Parent |
| AnimatorParamSnapshot | Animator Parameter |
| UnityEventSnapshot | State Events |
| ComponentFieldSnapshot (reflection) | a small custom module (see README) or State Events |
| DestroyGameObjectSnapshot, Create/Remove component actions | removed — use GameObject Active / Component Enabled (reversible) |

## Behaviour fixes you get for free

- Tweens interpolate from the current values (1.x snapped instantly because the start values were never captured).
- Objects in prefabs and objects spawned at runtime connect and receive the current state.
- Compound states no longer hide precise ones regardless of their order in the profile.
- Works without a camera (Screen source) and keeps animating with `timeScale = 0` (unscaled time).
