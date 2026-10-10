# Changelog

## [2.0.0]

Full rewrite. See [MIGRATION.md](MIGRATION.md).

### Added
- `AspectProfile` with data-driven states (name, color, range, stable id) and a default device set.
- Specificity-based matching with fallbacks; `ForceState` with fallback chain.
- `AspectSwitcher` sources: Screen, Camera, RectTransform; realtime stabilization; main switcher.
- `AspectResponsive` component with 18 built-in modules and a generic base for custom ones.
- Tween transitions stepped by the switcher (no coroutines, unscaled time), per-object override.
- Editor: profile diagram with draggable ranges, switcher and responsive inspectors, revertible
  Edit Mode preview, record mode, Scene View overlay, overview window.
- EditMode tests for ranges, matching, fallbacks and modules.
- `package.json`, package-level `.gitignore`.

### Fixed (compared to 1.x)
- Lerp/curve transitions snapping instantly.
- Snapshots inside prefabs never registering.
- Compound states hiding precise ones.
- No updates without a camera; camera assignment lost on entering Play Mode.
- Collection modified while notifying; `sceneLoaded` never unsubscribed; statics not reset.
- Non-serializable `ComponentEnabled` data and invalid casts; irreversible destroy/remove actions.

### Removed
- `AspectState` enum, `AspectStateConfig`, `AspectRatioMonitor`, all snapshot components.
