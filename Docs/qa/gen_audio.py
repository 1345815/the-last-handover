"""Generate procedural horror-bus audio WAVs (16-bit mono 22050 Hz) for TheLastHandover."""
import wave
import struct
import math
import random

SR = 22050
OUT = r"Y:\Codex\GameOff2026\TheLastHandover\Assets\Audio"


def write_wav(name, samples):
    peak = max(1e-9, max(abs(s) for s in samples))
    if peak > 0.98:
        k = 0.98 / peak
        samples = [s * k for s in samples]
    path = rf"{OUT}\{name}.wav"
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(b"".join(
            struct.pack("<h", int(max(-1.0, min(1.0, s)) * 32767)) for s in samples))
    print("wrote", path, f"{len(samples)/SR:.2f}s")


def lowpass(samples, alpha):
    out = []
    y = 0.0
    for x in samples:
        y += alpha * (x - y)
        out.append(y)
    return out


def highpass(samples, alpha):
    lp = lowpass(samples, alpha)
    return [x - y for x, y in zip(samples, lp)]


def loop_crossfade(samples, fade_s=0.25):
    n = len(samples)
    f = int(SR * fade_s)
    for i in range(f):
        w = i / f
        samples[i] = samples[i] * w + samples[n - f + i] * (1.0 - w)
    return samples[: n - f]


random.seed(20261106)

# 1. engine_loop 4s —— 低频隆隆，分量频率取 0.25Hz 整数倍保证 4 秒周期无缝循环
T = 4.0
n = int(SR * T)
eng = []
for i in range(n):
    t = i / SR
    v = (0.45 * math.sin(2 * math.pi * 57.0 * t)
         + 0.18 * math.sin(2 * math.pi * 114.0 * t + 0.7)
         + 0.07 * math.sin(2 * math.pi * 171.0 * t + 1.9)
         + 0.06 * math.sin(2 * math.pi * 28.5 * t + 2.6))
    v *= 1.0 + 0.14 * math.sin(2 * math.pi * 0.25 * t)      # 慢速呼吸
    v *= 1.0 + 0.05 * math.sin(2 * math.pi * 1.0 * t + 1.0)  # 怠速抖动
    eng.append(v)
rumble = loop_crossfade(lowpass([random.uniform(-1, 1) for _ in range(n)], 0.06), 0.2)
eng = [e + 0.5 * r for e, r in zip(eng, rumble)]
write_wav("engine_loop", eng)

# 2. radio_static 4s —— 闷掉的静电噪声 + 随机爆点
stat = [random.uniform(-1, 1) for _ in range(n)]
stat = lowpass(stat, 0.10)
for _ in range(14):
    pos = random.randint(0, n - 800)
    amp = random.uniform(0.5, 1.0)
    for j in range(500):
        stat[pos + j] += amp * math.exp(-j / 60.0) * random.uniform(-1, 1)
stat = loop_crossfade(stat, 0.25)
write_wav("radio_static", stat)

# 3. gear_clunk 0.28s —— 低频闷响 + 金属噪声脉冲
m = int(SR * 0.28)
clunk = []
for i in range(m):
    t = i / SR
    v = 0.9 * math.sin(2 * math.pi * 88.0 * t) * math.exp(-t * 26.0)
    if t < 0.03:
        v += 0.6 * random.uniform(-1, 1) * math.exp(-t * 130.0)
    clunk.append(v)
write_wav("gear_clunk", clunk)

# 4. door_hiss 0.9s —— 高频气声快起慢衰
m = int(SR * 0.9)
hiss = [random.uniform(-1, 1) for _ in range(m)]
hiss = highpass(hiss, 0.35)
hiss = [h * math.exp(-(i / SR) * 5.5) * min(1.0, (i / SR) / 0.02) for i, h in enumerate(hiss)]
write_wav("door_hiss", hiss)

# 5. broadcast_chime 1.4s —— 两音提示 + 轻回声
m = int(SR * 1.4)
chime = [0.0] * m
for i in range(m):
    t = i / SR
    if t < 0.5:
        chime[i] += 0.7 * math.sin(2 * math.pi * 660.0 * t) * math.exp(-t * 7.0)
    if t > 0.35:
        t2 = t - 0.35
        chime[i] += 0.6 * math.sin(2 * math.pi * 880.0 * t2) * math.exp(-t2 * 6.0)
echo = [0.0] * m
d = int(SR * 0.13)
for i in range(m - d):
    echo[i + d] = chime[i] * 0.3
write_wav("broadcast_chime", [c + e for c, e in zip(chime, echo)])

# 6. ending_surge 2.5s —— 静电渐强压场 + 低频 drone，尾部收掉
m = int(SR * 2.5)
surge = []
noise = [random.uniform(-1, 1) for _ in range(m)]
noise = lowpass(noise, 0.08)
for i in range(m):
    t = i / SR
    rise = min(1.0, t / 1.8)
    tail = 1.0 if t < 2.0 else max(0.0, 1.0 - (t - 2.0) / 0.5)
    v = 0.8 * rise * noise[i] * tail
    v += 0.4 * min(1.0, t / 1.2) * math.sin(2 * math.pi * 41.0 * t) * tail
    surge.append(v)
write_wav("ending_surge", surge)

print("all audio generated")
