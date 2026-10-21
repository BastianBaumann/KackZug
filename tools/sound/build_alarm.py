"""Builds the problem alarm tone: a shrill two-tone warble followed by fast high beeps.

The tone sits between 1 and 3 kHz, where phone speakers are loudest, and uses a band-limited
square wave (odd harmonics) so it cuts through instead of sounding like a soft ringtone.
Android repeats it while the alarm is insistent, so the file ends with a short pause.

Output: src/KackZug.Android/Resources/raw/kackzug_alarm.wav (16 bit, mono, 22.05 kHz)

Usage: python tools/sound/build_alarm.py
"""

import math
import pathlib
import struct
import wave

ROOT = pathlib.Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "src" / "KackZug.Android" / "Resources" / "raw" / "kackzug_alarm.wav"

RATE = 22050
HARMONICS = (1, 3, 5, 7)
EDGE_SECONDS = 0.004


def tone(frequency, seconds):
    count = int(RATE * seconds)
    edge = int(RATE * EDGE_SECONDS)
    samples = []
    for index in range(count):
        time = index / RATE
        value = 0.0
        for harmonic in HARMONICS:
            if frequency * harmonic < RATE / 2:
                value += math.sin(2 * math.pi * frequency * harmonic * time) / harmonic
        envelope = min(1.0, index / edge, (count - 1 - index) / edge)
        samples.append(value * envelope)
    return samples


def silence(seconds):
    return [0.0] * int(RATE * seconds)


def build():
    samples = []
    for _ in range(6):
        samples += tone(1000, 0.12)
        samples += tone(1500, 0.12)
    for _ in range(3):
        samples += tone(2200, 0.06)
        samples += silence(0.04)
    samples += silence(0.26)

    peak = max(abs(sample) for sample in samples)
    scale = 0.95 * 32767 / peak
    frames = b"".join(struct.pack("<h", int(sample * scale)) for sample in samples)

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(OUTPUT), "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(RATE)
        file.writeframes(frames)
    print(f"wrote {OUTPUT} ({len(samples) / RATE:.2f} s)")


if __name__ == "__main__":
    build()
