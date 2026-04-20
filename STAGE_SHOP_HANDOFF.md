# Stage Shop Handoff Summary

## Goal
Add a new Stage Shop flow so players can buy stage themes in the shop, equip one stage, and then enter StageScene with the equipped stage background.

## What Was Implemented

### 1) New Stage data model
- Added `StageData` ScriptableObject.
- File: `Assets/StageData.cs`
- Fields:
  - `stageID`
  - `stageName`
  - `previewIcon` (shop thumbnail)
  - `backgroundSprite` (full stage background used in StageScene)
  - `price`
  - `isDefault`

### 2) Stage ownership + equip manager
- Added singleton `StageOwnershipManager`.
- File: `Assets/Scripts/StageOwnershipManager.cs`
- Responsibilities:
  - Track owned stage IDs in PlayerPrefs (`PopstarHub_OwnedStages`)
  - Track equipped stage ID in PlayerPrefs (`PopstarHub_EquippedStage`)
  - Purchase stages with `CoinManager.TrySpend(...)`
  - Equip owned stages
  - Expose `GetEquippedStage()` for stage loading
- Events:
  - `OnOwnershipChanged`
  - `OnEquipChanged`

### 3) Stage shop card runtime button logic
- Added `ShopStageButton` component.
- File: `Assets/Scripts/ShopStageButton.cs`
- Button states:
  - `BUY` (not owned)
  - `EQUIP` (owned but not equipped)
  - `EQUIPPED` (currently active, button disabled)
- Handles purchase/equip click behavior and visual refresh.

### 4) Stage background application in StageScene
- Added `StageBackgroundController`.
- File: `Assets/Scripts/StageBackgroundController.cs`
- Reads `StageSessionData.backgroundSprite` on Start and applies it to `StageBackground` SpriteRenderer.

### 5) Stage session payload extension
- Updated `StageSessionData`.
- File: `Assets/Scripts/StageSessionData.cs`
- Added field:
  - `backgroundSprite`

### 6) Avatar -> Stage load bridge update
- Updated `EnterStageHandler`.
- File: `Assets/Scripts/EnterStageHandler.cs`
- Before loading `StageScene`, it now:
  - Gets currently equipped stage from `StageOwnershipManager`
  - Writes `data.backgroundSprite = equippedStage.backgroundSprite`

### 7) Shop builder integration
- Updated `ShopUIBuilder` to support data-driven stages from `StageData` assets.
- File: `Assets/Editor/ShopUIBuilder.cs`
- Changes made:
  - Load all `StageData` assets (`LoadAllStageData()`)
  - Build stages page from those assets (`BuildStagesPageFromData(...)`)
  - Build stage cards (`BuildStageDataCard(...)`) and wire `ShopStageButton`
  - Landing page stage count now uses actual `StageData` count

## What You Were Doing In This Chat
- You already had stage movement working.
- Then you requested integrating stages into the shop using your stage sprites.
- We implemented the full shop-side system (data, ownership, UI button states, and stage background handoff).
- You saw a big batch of temporary compiler errors in `ShopUIBuilder.cs` while edits were in progress; final code state now has no errors in the modified stage-shop files.

## Required Unity Setup (Manual)

### A) Create StageData assets
Create one `StageData` asset per stage sprite:
- Base stage
- Glass stage
- Golden stage
- Metal stage

Set:
- unique `stageID`
- `stageName`
- `previewIcon`
- `backgroundSprite`
- `price`
- `isDefault = true` only for base stage

### B) Add StageOwnershipManager in startup flow
- Place `StageOwnershipManager` on a persistent object in your initial scene.
- Fill the `allStages` array with all `StageData` assets.

### C) Add StageBackgroundController in StageScene
- On `StageBackground` GameObject, add `StageBackgroundController`.
- Ensure `StageBackground` has a SpriteRenderer.

### D) Rebuild shop UI
- Run menu: `PopstarHub -> Build Shop UI`

## Expected Player Flow
1. Player opens Shop -> Stages tab.
2. Player buys stage (if not owned) or equips an owned stage.
3. Player goes to Avatar and taps Enter Stage.
4. `EnterStageHandler` passes equipped stage background into `StageSessionData`.
5. `StageBackgroundController` applies that background in `StageScene`.

## Notes for Next Chat
If continuing in another chat, ask to:
1. Verify StageOwnershipManager exists in boot scene and persists.
2. Verify StageData assets are created and assigned.
3. Verify ShopUIBuilder-created stage cards have `ShopStageButton` wired.
4. Verify `StageBackgroundController` is attached to StageBackground.
5. Add optional polish: lock icon, tooltip for insufficient coins, selected-stage badge, equip sound.
