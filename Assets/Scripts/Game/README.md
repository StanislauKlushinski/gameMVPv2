# Game Code Layout

This folder is the root for MVP gameplay code.

## Layers

- `Core`: pure C# economy models, value types, services, and utilities.
- `Data`: Unity `ScriptableObject` definitions that describe balance and content.
- `Runtime`: Unity-facing `MonoBehaviour` bridges, lifecycle wiring, saves, and managers.
- `UI`: UI Toolkit controllers and presentation adapters.

## Numeric Rule

All economy quantities must use `GameNumber` once it is introduced. Use `float` only for time, progress bars, and small configuration coefficients.

## Optional Assembly Boundaries

Create assembly definitions from the Unity Editor, not by hand:

- `Assets/Scripts/Game/Core/Game.Core.asmdef`
  - References: none.
- `Assets/Scripts/Game/Data/Game.Data.asmdef`
  - References: `Game.Core`.
- `Assets/Scripts/Game/Runtime/Game.Runtime.asmdef`
  - References: `Game.Core`, `Game.Data`.
- `Assets/Scripts/Game/UI/Game.UI.asmdef`
  - References: `Game.Core`, `Game.Data`, `Game.Runtime`.

After Unity creates the files, verify that each layer only references lower-level dependencies listed above.
