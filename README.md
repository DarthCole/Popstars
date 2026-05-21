<div align="center">

# 💎 Popstar Hub

**A game where pop star rivals clash for chart supremacy.**

[![Unity](https://img.shields.io/badge/Engine-Unity%206-black?logo=unity)](https://unity.com/)
[![Language](https://img.shields.io/badge/Language-C%23-purple?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Mac-blue)](#)
[![Branch](https://img.shields.io/badge/Active%20Branch-feature%2Fgem--board-pink)](https://github.com/DarthCole/Popstars/tree/feature/gem-board)
[![Status](https://img.shields.io/badge/Status-In%20Development-yellow)](#)
[![License](https://img.shields.io/badge/License-MIT-green)](#license)

<br/>

> *Match gems. Deal damage. Become the bigger star.*

<br/>

</div>

---

## 🎮 What is Popstar Hub?

Popstar Hub is a **PC party game** where players battle for chart dominance through arcade mini-game challenges.

Mini games include 1) a glittering 8×8 gem board, chaining combos to deal damage to their opponent's health bar. 2) a karaoke batte game 3) a trivia game, and so many more
Every match earns **StarCoins**, the in-game currency of fame, which can be spent in the **Shop** on outfits, stages, and songs that visually transform your popstar.

Progression isn't just numbers — it's your popstar evolving and gaining access to more of the world.

---

## ✨ Core Features

| Feature | Description | Status |
|---|---|---|
| 🏠 **Hub Navigation** | Home base — select challenges, view your evolving popstar identity | 🔧 WIP |
| 💎 **Gem Match Battle** | 8×8 board — match symbols to deal damage and chain combos | ✅ Complete |
| 🪙 **StarCoin Economy** | Performance score converts to spendable in-game currency | 🔧 WIP |
| 🛍️ **Shop & Unlocks** | Outfits, stages, songs — self-expression is the popstar fantasy | 🔧 WIP |
| 🌐 **LAN Multiplayer** | Head-to-head on a local network — no internet required | 🔧 Prototyping |

### Secondary Features 

- 🎵 **Karaoke** — performance layer, high technical risk
- 🧠 **Trivia** — entertaining side challenge
- 💃 **Popstar Life** — lifestyle minigame (overlaps with Shop)

---

## 🕹️ The 5-Step Gameplay Loop

```
1. Enter the Hub      →   Select a mini-game challenge on your path to stardom
2. Compete            →   Play Gem Match — match gems, chain combos, defeat your rival
3. Earn StarCoins     →   Your performance score converts to StarCoins (in-game fame currency)
4. Visit the Shop     →   Spend StarCoins on outfits, stages & songs
5. Return Transformed →   Back to the Hub, visually evolved — ready for the next challenge
```

---

## 🎤 Characters

<table>
<tr>
<td align="center" width="50%">

### 🕺 MikaelYackson
*The Smooth Operator*

- **Specialty:** High combo multiplier
- **Playstyle:** Chain-focused, reward patient players who set up long sequences

</td>
<td align="center" width="50%">

### 💃 Beyonslay
*The Fierce Diva*

- **Specialty:** Power-Up Specialist
- **Playstyle:** Aggressive, burst-damage oriented through rapid power-up activation

</td>
</tr>
</table>

---

## 🏗️ Architecture

Popstar Hub follows a **clean, component-based Unity architecture**. One class per file, `[SerializeField]` over public fields, events for callbacks, and coroutines for all timed logic.

### Script Map

```
Assets/
├── Scripts/
│   ├── GemMatch/
│   │   ├── GemBoard.cs           # Central coordinator — 8×8 grid, spawning, cascade logic
│   │   ├── Gem.cs                # Individual gem data and state
│   │   ├── GemAnimator.cs        # All gem animations (slide, fall, match flash, power-up burst)
│   │   ├── GemInputHandler.cs    # Click/swap input — decoupled from board logic
│   │   ├── MatchDetector.cs      # Detects 3+ horizontal/vertical matches, returns match groups
│   │   ├── PowerUpHandler.cs     # Row clear, bomb, stun — event-driven, fully modular
│   │   └── HintSystem.cs         # Highlights valid moves — timer-triggered for new players
│   │
│   ├── Battle/
│   │   ├── BattleManager.cs      # Orchestrates fighter health, AI turns, win/loss flow
│   │   ├── Fighter.cs            # Fighter data and health logic
│   │   ├── HealthBarUI.cs        # Health bar rendering and animation
│   │   ├── AIOpponent.cs         # AI decision-making for solo play
│   │   └── PopstarData.cs        # ScriptableObject — character stats and metadata
│   │
│   ├── Economy/
│   │   ├── ScoreManager.cs       # Tracks score, combo multiplier, StarCoin conversion
│   │   └── ScorePopup.cs         # Floating score text on match
│   │
│   ├── Game/
│   │   ├── GameManager.cs        # Scene flow, game state orchestration
│   │   └── GameTimer.cs          # Match countdown timer
│   │
│   └── Audio/
│       └── SoundManager.cs       # 100% procedural audio — no audio files required
│
└── ScriptableObjects/
    └── PopstarData/              # MikaelYackson.asset, Beyonslay.asset
```

### Key Design Decisions

- **`GemBoard.cs` is the single coordinator** — all other gem systems communicate through it
- **Events over direct calls** — systems raise events; dependent scripts subscribe
- **Coroutines for all timed logic** — no heavy `Update()` polling
- **`[SerializeField]` everywhere** — never `public` fields; Inspector-wired references
- **`Awake()` null-coalescing fallbacks** — `??=` guards protect against reference loss from scene reloads

---

## 🚀 Getting Started

### Prerequisites

- [Unity 6](https://unity.com/releases/unity-6) (6000.0.x or later)
- Git with LFS support
- Windows 10 / macOS 12 or later

### Installation

```bash
# Clone the repository
git clone https://github.com/DarthCole/Popstars.git
cd Popstars

# Switch to the active development branch
git checkout feature/gem-board
```

Then open the project folder in **Unity Hub** and let it import.

> ⚠️ **Important:** Always move files using Unity's **Project window** — never via Finder/Explorer. Moving scripts outside Unity breaks all `.meta` file links and loses every serialized Inspector reference.

### First Run

1. Open `Assets/Scenes/MainScene.unity` in the Unity Editor
2. Press **Play** to launch the gem board
3. Click a gem, then click an adjacent gem to swap — match 3 or more to score

---

## 🌿 Branch Structure

| Branch | Purpose |
|---|---|
| `main` | Stable, reviewed builds only |
| `feature/gem-board` | **Active development** — all current gem-match and battle work lives here |

The PR from `feature/gem-board` → `main` will be opened at final submission: [DarthCole/Popstars/pull/new/feature/gem-board](https://github.com/DarthCole/Popstars/pull/new/feature/gem-board)

### Git Workflow (End of Session)

```bash
cd ~/Desktop/Popstars
git add .
git commit -m "feat: describe your change here"
git push origin feature/gem-board
```

> ✅ Always track `.meta` files in Git — they preserve serialized Inspector references across machines.

---

## 🗺️ Roadmap

| Milestone | Target | Status |
|---|---|---|
| Project Kickoff + Repo Setup | Jan 2025 | ✅ Done |
| GemMatch Core (board, match detection, scoring) | Feb 2025 | ✅ Done |
| Battle System (fighters, health bars, AI) | Mar 2025 | ✅ Done |
| Shop & StarCoin Economy | Apr 2025 | 🔧 In Progress |
| UI Polish + LAN Multiplayer | May 2025 | 🔧 In Progress |
| **Final Submission (University Showcase)** | **Jun 2025** | ⏳ Upcoming |

---

## 👥 Team Popstars

| Name | Role |
|---|---|
| **Andre** | Lead Developer |
| **Jason** | UI / Battle System |
| **Sedem** | Backend & Architecture |
| **Elsie** | Art & Character Design |
| **Reno** | Game Design & Balance |
| **Gibby** | Gem Match / Mini-Games |

*University Final Project — 2025*

---

## 🛠️ Tech Stack

| Tool | Use |
|---|---|
| Unity 6 | Game engine |
| C# | All game logic |
| GitHub | Version control |
| Unity Netcode for GameObjects | LAN multiplayer (prototyping) |
| TextMeshPro | All in-game UI text |
| ScriptableObjects | Character data (`PopstarData`) |

---

## 📁 Project Structure

```
Popstars/
├── Assets/
│   ├── Scenes/
│   ├── Scripts/          # All C# source (see Architecture above)
│   ├── Prefabs/
│   ├── ScriptableObjects/
│   ├── Art/              # Placeholder art — final assets TBD
│   └── Audio/            # Procedural only — no audio files committed
├── Packages/
├── ProjectSettings/
└── README.md
```

---

## 🤝 Contributing

This is a closed university project. If you're a team member:

1. Always branch from `feature/gem-board`, not `main`
2. Keep commits small and descriptive (`feat:`, `fix:`, `refactor:`)
3. Never move scripts outside Unity's Project window
4. Add `Awake()` null-coalescing guards to any new script that uses Inspector references
5. Open a PR for review before merging anything to `main`

---

## 📄 License
 Source code is available for review and portfolio purposes under the [MIT License](LICENSE).

---

<div align="center">

💎 *Match gems. Deal damage. Become the bigger star.* 💎

**[github.com/DarthCole/Popstars](https://github.com/DarthCole/Popstars)**  ·  Branch: `feature/gem-board`

</div>
