"""Procedural texture pack for TheLastHandover v0.6 — pure stdlib PNG writer."""
import zlib
import struct
import random

OUT = r"Y:\Codex\GameOff2026\TheLastHandover\Assets\Textures"
random.seed(0x1106)


def write_png(name, w, h, rows):
    """rows: list of h rows, each row list of (r, g, b) tuples 0-255."""
    raw = b"".join(b"\x00" + bytes(v for px in row for v in px) for row in rows)
    def chunk(tag, data):
        c = tag + data
        return struct.pack(">I", len(data)) + c + struct.pack(">I", zlib.crc32(c) & 0xFFFFFFFF)
    png = (b"\x89PNG\r\n\x1a\n"
           + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 2, 0, 0, 0))
           + chunk(b"IDAT", zlib.compress(raw, 9))
           + chunk(b"IEND", b""))
    with open(rf"{OUT}\{name}.png", "wb") as f:
        f.write(png)
    print("wrote", name, f"{w}x{h}")


def noise_grid(w, h, amp):
    return [[random.uniform(-amp, amp) for _ in range(w)] for _ in range(h)]


# ---------- 1. asphalt 沥青路面 512 ----------
W = H = 512
n = noise_grid(W, H, 11)
rows = []
for y in range(H):
    row = []
    for x in range(W):
        base = (38, 40, 44)
        v = n[y][x] + random.uniform(-6, 6)
        # 轮迹暗带
        for cx in (0.30, 0.70):
            d = abs(x / W - cx)
            if d < 0.07:
                v -= (0.07 - d) / 0.07 * 14
        # 大颗粒亮点
        if random.random() < 0.02:
            v += random.uniform(18, 40)
        # 横向裂纹
        row.append(tuple(max(0, min(255, int(c + v))) for c in base))
    rows.append(row)
for _ in range(6):  # 随机折线裂纹
    x, y = random.randint(0, W - 1), 0
    while y < H - 1:
        y += 1
        x += random.randint(-3, 3)
        for dx in (-1, 0, 1):
            if 0 <= x + dx < W:
                r, g, b = rows[y][x + dx]
                rows[y][x + dx] = (max(0, r - 26), max(0, g - 26), max(0, b - 26))
write_png("asphalt", W, H, rows)

# ---------- 2. concrete 混凝土 512 ----------
n = noise_grid(W, H, 9)
rows = []
for y in range(H):
    row = []
    for x in range(W):
        base = (118, 116, 110)
        v = n[y][x] + random.uniform(-5, 5)
        v -= max(0.0, (y / H - 0.75)) * 60          # 底部污渍
        v -= max(0.0, 0.15 - abs(x / W - 0.5)) * 8  # 中缝阴影
        streak = math_like = 0.0
        if random.random() < 0.05:
            v -= 10
        row.append(tuple(max(0, min(255, int(c + v))) for c in base))
    rows.append(row)
for _ in range(4):  # 裂纹
    x, y = random.randint(0, W - 1), 0
    while y < H - 1:
        y += 1
        x += random.randint(-4, 4)
        if 0 <= x < W:
            r, g, b = rows[y][x]
            rows[y][x] = (max(0, r - 40), max(0, g - 40), max(0, b - 40))
write_png("concrete", W, H, rows)

# ---------- 3. cabin_metal 车厢内饰 512 ----------
rows = []
for y in range(H):
    line_jitter = random.uniform(-7, 7)
    row = []
    for x in range(W):
        base = (30, 32, 34)
        v = line_jitter + random.uniform(-4, 4)
        if random.random() < 0.004:  # 划痕
            v += random.uniform(30, 70)
        row.append(tuple(max(0, min(255, int(c + v))) for c in base))
    rows.append(row)
write_png("cabin_metal", W, H, rows)

# ---------- 4. rubber 橡胶 256 ----------
W2 = H2 = 256
rows = []
for y in range(H2):
    row = []
    for x in range(W2):
        base = (17, 17, 19)
        v = random.uniform(-4, 4) + (random.uniform(8, 20) if random.random() < 0.01 else 0)
        row.append(tuple(max(0, min(255, int(c + v))) for c in base))
    rows.append(row)
write_png("rubber", W2, H2, rows)

# ---------- 5. sign_face 站牌面 256 ----------
W2 = H2 = 256
rows = [[(22, 32, 46)] * W2 for _ in range(H2)]
for band in range(3):  # 三条白色站名条
    y0 = 52 + band * 62
    for y in range(y0, min(H2, y0 + 30)):
        for x in range(24, W2 - 24):
            erode = random.random() < 0.10
            if not erode:
                rows[y][x] = (min(255, 170 + random.randint(-14, 14)),
                              min(255, 174 + random.randint(-14, 14)),
                              min(255, 168 + random.randint(-14, 14)))
for y in range(H2):
    for x in range(W2):
        if x < 6 or x > W2 - 7 or y < 6 or y > H2 - 7:
            rows[y][x] = (200, 202, 196)
write_png("sign_face", W2, H2, rows)

# ---------- 6. billboard 广告灯箱 512 ----------
rows = []
palette = [(70, 26, 24), (26, 58, 62), (58, 50, 20), (40, 30, 58)]
zone = [palette[random.randrange(4)] for _ in range(8)]
for y in range(H):
    zi = min(7, y * 8 // H)
    base = zone[zi]
    row = []
    for x in range(W):
        v = random.uniform(-10, 10)
        r, g, b = base
        # 白色文字条
        if 90 < y < 130 or 200 < y < 236 or 330 < y < 360:
            if 0.18 < x / W < 0.82 and random.random() > 0.08:
                r, g, b = 225, 220, 205
        row.append(tuple(max(0, min(255, int(c + v))) for c in (r, g, b)))
    rows.append(row)
write_png("billboard", W, H, rows)

# ---------- 7. raincoat 雨衣 256 ----------
rows = []
for y in range(H2):
    row = []
    for x in range(W2):
        stripe = 12 if (x // 10) % 2 == 0 else -6
        base = (166, 156, 118)
        v = stripe + random.uniform(-8, 8)
        if random.random() < 0.03:
            v -= 25  # 污渍
        row.append(tuple(max(0, min(255, int(c + v))) for c in base))
    rows.append(row)
write_png("raincoat", W2, H2, rows)

# ---------- 8. building 建筑剪影窗格 512 ----------
rows = []
cell_w, cell_h = 40, 50
lit = {}
for gy in range(H // cell_h):
    for gx in range(W // cell_w):
        lit[(gx, gy)] = random.random() < 0.06
for y in range(H):
    row = []
    for x in range(W):
        gx, gy = x // cell_w, y // cell_h
        fx, fy = (x % cell_w) / cell_w, (y % cell_h) / cell_h
        if lit.get((gx, gy)) and 0.15 < fx < 0.85 and 0.15 < fy < 0.85:
            c = (168, 128, 62) if random.random() < 0.97 else (150, 60, 40)
            c = tuple(max(0, c[i] + random.randint(-12, 12)) for i in range(3))
        else:
            v = random.uniform(-3, 3)
            c = tuple(max(0, min(255, int(14 + v))) for _ in range(3))
        row.append(c)
    rows.append(row)
write_png("building", W, H, rows)

# ---------- 9. lampglow 路灯灯头 128 ----------
W2 = H2 = 128
rows = []
for y in range(H2):
    row = []
    for x in range(W2):
        d = math_hypot = ((x / W2 - 0.5) ** 2 + (y / H2 - 0.5) ** 2) ** 0.5
        v = max(0.0, 1.0 - d * 1.8)
        row.append((int(60 + 195 * v), int(52 + 170 * v), int(30 + 120 * v)))
    rows.append(row)
write_png("lampglow", W2, H2, rows)

print("all textures generated")
