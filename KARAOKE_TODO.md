# Karaoke Todo

## Status
Karaoke feature is implemented end-to-end with editor builders + auto-wiring scripts.

## Completed
- [x] Song pipeline set up (separation + lyric/pitch data extraction)
- [x] SongData model implemented
- [x] JSON import pipeline implemented and working
- [x] Core karaoke runtime implemented
- [x] Full karaoke gameplay UI one-click builder implemented
- [x] One-click karaoke scene wirer implemented
- [x] Song Select screen builder implemented
- [x] Song Select scene wirer implemented
- [x] Song Select runtime flow implemented (pick song -> open karaoke panel)
- [x] Back navigation flow implemented:
	- Karaoke back -> Song Select panel
	- Song Select back -> Hub scene
- [x] Cover art support added:
	- Song cards show per-song cover art
	- Karaoke mic/avatar square shows selected song cover art
- [x] Mic pitch detection tuned for practical input levels
- [x] Results panel flow (score/stars/coins) connected

## Implemented Scripts

### Runtime
- Assets/SongData.cs
- Assets/MicPitchDetector.cs
- Assets/LyricsSyncer.cs
- Assets/PitchScorer.cs
- Assets/KaraokeManager.cs
- Assets/KaraokeUI.cs
- Assets/KaraokeResultsPanel.cs
- Assets/SongSelectUI.cs

### Editor / Builder / Wiring
- Assets/Editor/SongDataImporter.cs
- Assets/Editor/KaraokeUIBuilder.cs
- Assets/Editor/KaraokeSceneWirer.cs
- Assets/Editor/SongSelectUIBuilder.cs
- Assets/Editor/SongSelectSceneWirer.cs
- Assets/Editor/MainHubUIBuilder.cs

### Data Tooling
- Tools/convert_song_data.py

## Current Scene Setup Flow (Authoritative)
Run these in order when setting up or recovering the scene:

1. Karaoke -> Import All Songs from JSON
2. Karaoke -> Build Full Karaoke UI
3. Karaoke -> Wire Scene
4. PopstarHub -> Build Song Select UI
5. Karaoke -> Wire Song Select

Notes:
- Re-running builders replaces their generated panel trees.
- Re-run both wirers after rebuilding UI panels.

## Song Asset Requirements
For each SongData asset in Assets/SongData:
- Backing Track must be assigned (instrumental/no_vocals clip)
- Cover Art sprite should be assigned

If Backing Track is missing, Song Select warns and song will not start.

## Gameplay Behavior Summary
- Record button starts selected song when manager is Idle.
- Mic pitch is captured via Unity Microphone API and autocorrelation.
- Score logic:
	- Perfect (<= 15 Hz): +10
	- Good (<= 35 Hz): +6
	- Acceptable (<= 60 Hz): +2
	- Off-key (> 60 Hz): -4 and combo reset
	- Silence: 0
- Multiplier:
	- 1x (0-5 combo frames)
	- 1.5x (6-15)
	- 2x (16+)

## Remaining QA / Polish Tasks
- [ ] Verify progress bar behavior across all songs and resolutions
- [ ] Validate lyric timing and target pitch quality for each track
- [ ] Confirm all SongData assets have both Backing Track + Cover Art
- [ ] Final pass on mic sensitivity/noise-gate tuning by environment
- [ ] Optional: Persist StarCoins / purchases into profile save data
