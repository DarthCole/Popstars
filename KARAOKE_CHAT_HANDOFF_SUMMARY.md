# PopstarHub Karaoke Handoff Summary

## Project Context
- Unity project: PopstarHub (school project, non-publishing demo scope).
- Branch strategy discussed:
  - Shop work on feature/shop.
  - Karaoke work started on feature/karaoke from feature/shop.
- Shop system is already complete and pushed previously.

## Core Product Decisions Locked During This Chat
- Karaoke is a separate minigame flow, not merged into shop panel logic.
- Microphone input approach: Unity Microphone API.
- Pitch detection approach: autocorrelation in pure C# (no AI, no external runtime plugin).
- Lyric correctness detection: deferred. For now lyrics are visual guidance only.
- Scoring model chosen:
  - Perfect (±15 Hz): +10/frame
  - Good (±35 Hz): +6/frame
  - Acceptable (±60 Hz): +2/frame
  - Off-key (>60 Hz): -4/frame
  - Silence (noise gate): 0
- Combo model chosen:
  - 1x (0-5)
  - 1.5x (6-15)
  - 2x (16+)
  - reset on off-key
- Star thresholds chosen:
  - 90-100 => 3 stars
  - 70-89 => 2 stars
  - 50-69 => 1 star
  - <50 => 0 stars

## Song and Data Pipeline Progress
- Song scope moved from 5 to 6 active songs (God's Plan dropped).
- Vocal/instrumental separation done externally (Demucs workflow confirmed).
- Sonic Visualiser setup done, including installing Vamp plugins to unlock pYin.
- pYin transform used: Smoothed Pitch Track.
- Pitch CSV export workflow validated (green dotted pitch line appeared and exported).
- LRC timestamped lyric files were obtained online (lrclib route suggested and used).

## Files Created/Updated in This Chat

### Runtime gameplay scripts
- Assets/SongData.cs
- Assets/MicPitchDetector.cs
- Assets/LyricsSyncer.cs
- Assets/PitchScorer.cs
- Assets/KaraokeManager.cs
- Assets/KaraokeUI.cs
- Assets/KaraokeResultsPanel.cs

### Data pipeline scripts
- Tools/convert_song_data.py
- Assets/Editor/SongDataImporter.cs

### UI builder scripts
- Assets/Editor/KaraokeUIBuilder.cs

## What Each Script Is Intended To Do

### Assets/SongData.cs
- Defines SongData ScriptableObject with:
  - songName
  - artist
  - BPM
  - backingTrack
  - LyricLine[] lines
- LyricLine holds timestamp, text, targetPitch.

### Assets/MicPitchDetector.cs
- Starts microphone recording.
- Reads live buffers via GetData.
- Computes pitch via autocorrelation.
- Applies confidence threshold and smoothing.
- Applies noise gate and exposes current pitch/volume.

### Assets/LyricsSyncer.cs
- Accepts SongData and audio time updates.
- Finds previous/current/next lyric lines by timestamp.
- Exposes line progress for lyric highlighting.
- Emits lyric-changed events.

### Assets/PitchScorer.cs
- Compares mic pitch to current lyric target pitch.
- Applies zone scoring + combo multipliers + off-key reset.
- Tracks per-line consistency and total score.
- Emits song completion event with score/stars.

### Assets/KaraokeManager.cs
- Orchestrates audio source + mic detector + lyric sync + scoring.
- Tracks state machine (idle/playing/paused/complete).
- Exposes runtime values for UI binding.

### Assets/KaraokeUI.cs
- Updates visual UI values each frame:
  - top score
  - lyric rows
  - highlighted current word
  - pitch bar and note labels
  - progress bar and timestamps
  - mic indicator and waveform
- Handles record/pause/resume/restart/back button interactions.
- Triggers results panel at completion.

### Assets/KaraokeResultsPanel.cs
- Animated end-of-song panel:
  - score count-up
  - stars reveal
  - coin reward count-up
  - play again/back to hub actions

### Tools/convert_song_data.py
- Converts exported pitch CSV + LRC lyrics into JSON song data.
- Maps time windows to average pitch values.
- Writes per-song JSON files under Assets/SongData.

### Assets/Editor/SongDataImporter.cs
- Menu action to import JSON files into SongData assets.
- Creates/updates .asset objects and fills lyric lines.

### Assets/Editor/KaraokeUIBuilder.cs
- Editor menu builder for generating karaoke UI hierarchy automatically.
- Evolved over chat from partial builder to full page builder attempt.
- Includes controls, bars, waveform, top area, results overlay generation.

## Unity Scene/Setup Progress
- SampleScene rename to ShopScene was discussed and performed safely in Unity Project view.
- New KaraokeScene creation was discussed and performed.
- Guidance given to keep karaoke in separate scene from shop.
- UI construction started manually and also via builder script attempts.

## Issues Encountered and Resolutions

### 1) Spleeter install failure via pip
- Cause: dependency constraints with modern Python (3.13) and old numpy requirement.
- Workaround suggested: install via conda-forge when needed.
- Pipeline continued with Demucs + Sonic Visualiser path instead.

### 2) Sonic Visualiser missing pYin transform
- Cause: plugin not installed.
- Fix: installed Vamp plugin pack.
- Outcome: pYin options appeared and were usable.

### 3) UI control icons rendering as squares
- Cause: missing glyphs in selected TMP font asset for symbols.
- Workaround explained:
  - use image icons instead of Unicode glyphs,
  - or choose font asset with glyph coverage.

### 4) Buttons appeared square instead of circular
- Cause: Image Source Sprite unset (None).
- Fix explained:
  - assign round sprite (e.g., built-in Knob),
  - add ring/outline child image for white border effect.

## Current Workspace Status (at handoff)
- No compile errors currently reported by tooling.
- Karaoke runtime scripts exist in Assets.
- Converter/importer scripts exist for song data pipeline.
- Karaoke UI Builder script exists and has been iterated.
- Manual UI work has also been done in scene and may coexist with builder output.

## Pending Work (Next Chat Should Continue Here)

### A) Finalize UI parity with target mockup
- Cleanly finalize one source of truth:
  - either fully builder-generated UI,
  - or manual scene UI with only minor helper scripts.
- Replace placeholder icon glyph labels with actual icon images.
- Ensure circular button sprites and white outline ring are consistent.
- Confirm fonts and colors match concept exactly.

### B) Wire scene references in KaraokeUI
- Ensure all serialized fields are connected in Inspector:
  - title/artist/score text
  - lyric labels
  - pitch/progress fills and labels
  - controls
  - waveform bars
  - results panel references

### C) Complete song asset data import and assignment
- Run converter script for available CSV + LRC files.
- Run SongData importer in Unity.
- Assign backingTrack (no_vocals.wav) to each SongData asset.

### D) Functional test pass
- Mic permission and detection live test.
- Lyric sync timing validation.
- Scoring behavior validation (including off-key penalties and combo resets).
- End-of-song results and rewards validation.

### E) Hub/shop integration
- Add navigation from shop/hub into KaraokeScene with selected song.
- Add return path from karaoke back to hub.

## Practical Commands/Actions Mentioned
- Python version check:
  - py --version
- Data conversion run:
  - python Tools/convert_song_data.py
- Unity menu actions:
  - Karaoke/Import All Songs from JSON
  - Karaoke/Build Full Karaoke UI (current builder script)

## Notes For Continuation
- If visual mismatch persists, fastest stable path is:
  1) finalize hierarchy manually in KaraokeScene,
  2) keep builder script as optional utility,
  3) focus on wiring + gameplay testing next.
- There was also a late request to build a separate concept landing page with 4 mode cards + 1 shop card; this was requested but not yet implemented before summary request.

---
Generated as a handoff document from the current chat so work can continue in a new thread without losing context.
