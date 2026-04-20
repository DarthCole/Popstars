import csv
import json
import re
import os
from pathlib import Path

# ── Paths ────────────────────────────────────────────────────────────────────
DESKTOP   = Path(r"C:\Users\Aaakr\OneDrive\Desktop")
DOWNLOADS = Path(r"C:\Users\Aaakr\Downloads")
OUTPUT    = Path(r"C:\Users\Aaakr\OneDrive\Documents\Unity\PopstarHub\Assets\SongData")

# ── Song mapping: (songName, artist, csvFilename, lrcFilename) ────────────────
SONGS = [
    ("Viva La Vida",    "Coldplay",                         "VivaLaVida.csv",    "Viva La Vida - Coldplay.lrc"),
    ("Blinding Lights", "The Weeknd",                       "BlindingLights.csv","Blinding Lights - The Weeknd.lrc"),
    ("Beat It",         "Michael Jackson",                  "BeatIt.csv",        "Beat It - Michael Jackson.lrc"),
    ("Dance Monkey",    "Tones And I",                      "DanceMonkey.csv",   "Dance Monkey - Tones And I.lrc"),
    ("Mofe",            "Mavo",                             "Mofe.csv",          "Mofe - Mavo.lrc"),
    ("HOW FAR",              "NO11, Ayjay bobo, Monochrome",  "HowFar.csv",              "HOW FAR - NO11, Ayjay bobo, Monochrome.lrc"),
    ("Poker Face",            "Lady Gaga",                     "PokerFace.csv",            "Poker Face - Lady Gaga.lrc"),
    ("Somebody That I Used To Know", "Gotye ft. Kimbra",       "SomebodyThatIUsedToKnow.csv", "Somebody That I Used To Know - Gotye, Kimbra.lrc"),
    ("Someone Like You",      "Adele",                         "SomeoneLikeYou.csv",       "Someone Like You - Adele.lrc"),
]

# ── Helpers ───────────────────────────────────────────────────────────────────

def load_pitch_csv(path):
    """Returns list of (time_seconds, hz) — skips zeros/silence."""
    pitches = []
    with open(path, 'r', encoding='utf-8') as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            parts = line.split(',')
            if len(parts) < 2:
                continue
            try:
                t  = float(parts[0])
                hz = float(parts[1])
                if hz > 0:
                    pitches.append((t, hz))
            except ValueError:
                continue  # skip header row if present
    return pitches


def parse_lrc(path):
    """Returns sorted list of (time_seconds, text) from an LRC file."""
    lines = []
    pattern = re.compile(r'\[(\d{1,2}):(\d{2}\.\d+)\](.*)')
    with open(path, 'r', encoding='utf-8') as f:
        for line in f:
            match = pattern.match(line.strip())
            if match:
                minutes = int(match.group(1))
                seconds = float(match.group(2))
                text = match.group(3).strip()
                if text:
                    lines.append((minutes * 60 + seconds, text))
    return sorted(lines, key=lambda x: x[0])


def avg_pitch(pitch_data, start, end):
    """Average non-zero Hz values between start and end seconds."""
    values = [hz for t, hz in pitch_data if start <= t < end]
    if not values:
        return 0.0
    return round(sum(values) / len(values), 2)


def process(song_name, artist, csv_name, lrc_name):
    csv_path = DESKTOP   / csv_name
    lrc_path = DOWNLOADS / lrc_name

    if not csv_path.exists():
        print(f"  [SKIP] CSV not found: {csv_path}")
        return None
    if not lrc_path.exists():
        print(f"  [SKIP] LRC not found: {lrc_path}")
        return None

    pitch_data   = load_pitch_csv(csv_path)
    lyric_lines  = parse_lrc(lrc_path)

    if not lyric_lines:
        print(f"  [SKIP] No lyric lines parsed from {lrc_name}")
        return None

    result_lines = []
    for i, (ts, text) in enumerate(lyric_lines):
        end_ts = lyric_lines[i + 1][0] if i + 1 < len(lyric_lines) else ts + 5.0
        result_lines.append({
            "timestamp":   round(ts, 3),
            "text":        text,
            "targetPitch": avg_pitch(pitch_data, ts, end_ts),
        })

    return {"songName": song_name, "artist": artist, "lines": result_lines}


# ── Run ───────────────────────────────────────────────────────────────────────

OUTPUT.mkdir(parents=True, exist_ok=True)

for song_name, artist, csv_name, lrc_name in SONGS:
    print(f"Processing: {song_name} ...")
    data = process(song_name, artist, csv_name, lrc_name)
    if data:
        safe = song_name.replace(" ", "").replace("'", "").replace(",", "")
        out  = OUTPUT / f"{safe}.json"
        with open(out, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        print(f"  OK  {len(data['lines'])} lines  →  {out}")

print("\nAll done.")
