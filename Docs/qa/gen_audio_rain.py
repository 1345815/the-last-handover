"""Append: rain_loop.wav for TheLastHandover (v0.5)."""
import wave
import struct
import math
import random

SR = 22050
OUT = r"Y:\Codex\GameOff2026\TheLastHandover\Assets\Audio"


def write_wav(name, samples):
    peak = max(1e-9, max(abs(s) for s in samples))
    if peak > 0.98:
        samples = [s * (0.98 / peak) for s in samples]
    with wave.open(rf"{OUT}\{name}.wav", "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(b"".join(
            struct.pack("<h", int(max(-1.0, min(1.0, s)) * 32767)) for s in samples))
    print("wrote", name, f"{len(samples)/SR:.2f}s")


def lowpass(samples, alpha):
    out, y = [], 0.0
    for x in samples:
        y += alpha * (x - y)
        out.append(y)
    return out


def loop_crossfade(samples, fade_s=0.3):
    n, f = len(samples), int(SR * fade_s)
    for i in range(f):
        w = i / f
        samples[i] = samples[i] * w + samples[n - f + i] * (1.0 - w)
    return samples[: n - f]


random.seed(2605)

# rain_loop 4s：远处闷雨 + 近处雨点敲击 + 偶发滴答
n = int(SR * 4.0)
far = lowpass([random.uniform(-1, 1) for _ in range(n)], 0.045)
near = lowpass([random.uniform(-1, 1) for _ in range(n)], 0.35)
rain = [0.5 * f + 0.35 * nn for f, nn in zip(far, near)]
for _ in range(26):  # 雨点滴答
    pos = random.randint(0, n - 200)
    amp = random.uniform(0.25, 0.55)
    for j in range(120):
        rain[pos + j] += amp * math.exp(-j / 18.0) * math.sin(2 * math.pi * (900 + random.uniform(-150, 150)) * j / SR)
# 4 秒慢起伏（雨势呼吸）
rain = [r * (0.85 + 0.15 * math.sin(2 * math.pi * 0.25 * i / SR)) for i, r in enumerate(rain)]
rain = loop_crossfade(rain, 0.3)
write_wav("rain_loop", rain)
print("done")
