# Trivia Scene Handoff

Date: 2026-04-19

## Goal
Build and maintain Trivia UI with the same methodology as Main Hub:
- Editor script builds the full TriviaCanvas hierarchy.
- Runtime script handles only game logic, reference lookup, and button callback wiring.
- No runtime canvas rebuild.
- No DestroyImmediate in runtime flow.
- Canvas remains fully editable in Inspector after build.

## What Was Discussed and Decided
1. The core issue was button clicks not working when a scene had a builder-created canvas that was not wired.
2. The required architecture is strict separation:
- Editor builder script creates all visual UI objects and names.
- Runtime MonoBehaviour only finds existing objects and wires listeners.
3. Runtime should not regenerate panels or replace the canvas.
4. Runtime may create only dynamic rows/items that are gameplay data dependent, like player name rows and result rows.

## Current Project State
- Runtime logic script exists at [Assets/Scripts/TriviaGameUI.cs](Assets/Scripts/TriviaGameUI.cs).
- Editor UI builder exists at [Assets/Editor/TriviaUIBuilder.cs](Assets/Editor/TriviaUIBuilder.cs).
- Scene builder exists at [Assets/Editor/TriviaSceneBuilder.cs](Assets/Editor/TriviaSceneBuilder.cs).

Important note:
- [Assets/Editor/TriviaSceneBuilder.cs](Assets/Editor/TriviaSceneBuilder.cs) still states that TriviaGameUI builds UI at runtime in its comment, but current runtime behavior is find and wire on a prebuilt canvas.

## Required TriviaCanvas Contract
The builder must create TriviaCanvas with this exact naming contract so runtime lookup succeeds.

### Panels
- ModePanel
- PartySetup
- PartyCount
- PartyGame
- PartyReveal
- SoloGame
- Results

### Party Paths
- PartySetup/NameList
- PartyCount/Slider
- PartyCount/Pill/CountLbl
- PartyGame/QNumTxt
- PartyGame/QCard/QuestionTxt
- PartyReveal/RevealQTxt
- PartyReveal/RevealAnsTxt
- PartyReveal/PlayerBtns
- PartyReveal/BoardTxt

### Solo Paths
- SoloGame/SoloQNumTxt
- SoloGame/SoloCoinsTxt
- SoloGame/QCard/SoloQTxt
- SoloGame/AnswerGrid/Ans0
- SoloGame/AnswerGrid/Ans1
- SoloGame/AnswerGrid/Ans2
- SoloGame/AnswerGrid/Ans3
- SoloGame/AnswerGrid/Ans0/AnsLbl
- SoloGame/AnswerGrid/Ans1/AnsLbl
- SoloGame/AnswerGrid/Ans2/AnsLbl
- SoloGame/AnswerGrid/Ans3/AnsLbl

### Results Paths
- Results/ResultsTitle
- Results/ResultsSub
- Results/ResultsCoins
- Results/ResultsList

### Buttons That Must Exist
- ModePanel/PartyCard/Btn_PartyPlay
- ModePanel/SoloCard/Btn_SoloPlay
- ModePanel/Btn_BackToHub
- PartySetup/Btn_AddPlayer
- PartySetup/Btn_Continue
- PartySetup/Btn_BackMode
- PartyCount/Btn_LetsGo
- PartyCount/Btn_BackSetup
- PartyGame/Btn_Reveal
- PartyGame/Btn_QuitGame
- PartyReveal/Btn_NextQ
- PartyReveal/Btn_QuitReveal
- SoloGame/Btn_QuitSolo
- Results/Btn_PlayAgain
- Results/Btn_BackToHub2

## Runtime Responsibilities
TriviaGameUI Start should do only this:
1. Init audio clips and load question bank.
2. Build tiny runtime sprites used by dynamic elements.
3. Find TriviaCanvas and resolve all references by name.
4. Wire button callbacks and slider onValueChanged.
5. Clear placeholder rows in NameList and PlayerBtns.
6. Show ModePanel.

Runtime should not do this:
- Create or destroy the root canvas.
- Build any static panel layout.
- Rename contract objects expected by the logic.

## Builder Responsibilities
TriviaUIBuilder should do only this:
1. Rebuild TriviaCanvas in editor via menu command.
2. Create all static hierarchy and visual styling.
3. Create Button components but no gameplay callback assignments.
4. Keep object names stable to satisfy runtime Find paths.

## Scene Setup Checklist
1. Open Trivia scene.
2. Run menu PopstarHub > Build Trivia UI.
3. Ensure an object with TriviaGameUI exists in scene, usually TriviaManager.
4. Press Play and verify:
- Mode screen appears.
- Party and Solo buttons work.
- Back buttons route to MainMenuScene.
5. Save the scene once verified.

## Common Failure Modes and Fixes
1. NullReference on Start:
- Cause: missing or renamed object path.
- Fix: rebuild UI with builder and keep contract names exact.

2. Buttons visible but no actions:
- Cause: wrong hierarchy names or missing TriviaGameUI host object.
- Fix: ensure TriviaManager with TriviaGameUI exists and paths match contract.

3. UI not editable after exiting Play:
- Cause: runtime rebuilding architecture.
- Fix: keep build logic only in editor script.

## Prompt To Paste In A New Chat
Use this prompt exactly to ask for a fresh or updated builder script.

I need you to create or update my Unity editor script that builds TriviaCanvas using this exact architecture:
- Editor script builds full static UI in edit mode from a menu command.
- Runtime script TriviaGameUI must remain logic only and should not rebuild or destroy canvas.
- Keep all object names and paths exactly as listed below because runtime Find depends on them.

Required panel names:
ModePanel, PartySetup, PartyCount, PartyGame, PartyReveal, SoloGame, Results

Required object paths:
PartySetup/NameList
PartyCount/Slider
PartyCount/Pill/CountLbl
PartyGame/QNumTxt
PartyGame/QCard/QuestionTxt
PartyReveal/RevealQTxt
PartyReveal/RevealAnsTxt
PartyReveal/PlayerBtns
PartyReveal/BoardTxt
SoloGame/SoloQNumTxt
SoloGame/SoloCoinsTxt
SoloGame/QCard/SoloQTxt
SoloGame/AnswerGrid/Ans0..Ans3 each with child AnsLbl
Results/ResultsTitle
Results/ResultsSub
Results/ResultsCoins
Results/ResultsList

Required button paths:
ModePanel/PartyCard/Btn_PartyPlay
ModePanel/SoloCard/Btn_SoloPlay
ModePanel/Btn_BackToHub
PartySetup/Btn_AddPlayer
PartySetup/Btn_Continue
PartySetup/Btn_BackMode
PartyCount/Btn_LetsGo
PartyCount/Btn_BackSetup
PartyGame/Btn_Reveal
PartyGame/Btn_QuitGame
PartyReveal/Btn_NextQ
PartyReveal/Btn_QuitReveal
SoloGame/Btn_QuitSolo
Results/Btn_PlayAgain
Results/Btn_BackToHub2

Please output the full editor script at Assets/Editor/TriviaUIBuilder.cs and keep naming stable.

## Related Files
- [Assets/Scripts/TriviaGameUI.cs](Assets/Scripts/TriviaGameUI.cs)
- [Assets/Editor/TriviaUIBuilder.cs](Assets/Editor/TriviaUIBuilder.cs)
- [Assets/Editor/TriviaSceneBuilder.cs](Assets/Editor/TriviaSceneBuilder.cs)
