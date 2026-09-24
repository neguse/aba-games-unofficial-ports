import math
import struct


def bitmap(path, expected):
    data = path.read_bytes()
    offset = struct.unpack_from('<I', data, 10)[0]
    width, height, planes, bits, compression = struct.unpack_from('<iiHHI', data, 18)
    if data[:2] != b'BM' or (width, height, bits) != expected or planes != 1 or compression != 0:
        raise ValueError(f'Unexpected title bitmap: {path}')
    stride = ((width * bits + 31) // 32) * 4
    palette = 14 + struct.unpack_from('<I', data, 14)[0]
    pixels = []
    for y in range(height):
        for x in range(width):
            p = offset + (height - 1 - y) * stride + x * (bits // 8)
            if bits == 8:
                p = palette + data[p] * 4
            b, g, r = data[p:p + 3]
            pixels.append((r, g, b, 255))
    return pixels


def mipmaps(pixels, width, height):
    # gluBuild2DMipmaps chooses the nearest power of two before box filtering.
    new_width = 1 << int(math.log2(width / 1.5) + 1)
    new_height = 1 << int(math.log2(height / 1.5) + 1)
    if (new_width, new_height) != (width, height):
        resized = []
        for y in range(new_height):
            top, bottom = y * height / new_height, (y + 1) * height / new_height
            for x in range(new_width):
                left, right = x * width / new_width, (x + 1) * width / new_width
                total = [0.0] * 4
                # GLU clamps the final source row before calculating its box coverage.
                clipped_bottom = min(math.floor(bottom), height - 1) + bottom % 1
                for sy in range(math.floor(top), math.ceil(clipped_bottom)):
                    for sx in range(math.floor(left), math.ceil(right)):
                        weight = (min(sx + 1, right) - max(sx, left)) * (min(sy + 1, clipped_bottom) - max(sy, top))
                        for c in range(4):
                            total[c] += pixels[sy * width + sx][c] * weight
                resized.append(tuple(int(v / ((right - left) * (bottom - top))) for v in total))
        pixels, width, height = resized, new_width, new_height
    levels = [(width, height, pixels)]
    while width > 1 or height > 1:
        nx, ny = min(width, 2), min(height, 2)
        reduced = []
        for y in range(max(1, height // 2)):
            for x in range(max(1, width // 2)):
                reduced.append(tuple((sum(pixels[(y * ny + dy) * width + x * nx + dx][c]
                                          for dy in range(ny) for dx in range(nx))
                                      + (2 if nx * ny == 4 else 0)) // (nx * ny) for c in range(4)))
        pixels, width, height = reduced, max(1, width // 2), max(1, height // 2)
        levels.append((width, height, pixels))
    return levels


def image(name, pixels, width, height, mipmapped=False):
    levels = mipmaps(pixels, width, height) if mipmapped else [(width, height, pixels)]
    width, height, _ = levels[0]
    # lub exposes level zero only; stack the GLU mip levels in one texture.
    atlas = []
    for w, h, level in levels:
        for y in range(h):
            atlas.extend(value for pixel in level[y * w:(y + 1) * w] for value in pixel)
            atlas.extend([0] * ((width - w) * 4))
    return ('new DrawImage { key = "title-' + name + '", width = ' + str(width)
            + ', height = ' + str(height) + ', atlasHeight = ' + str(sum(h for _, h, _ in levels))
            + ', levels = ' + str(len(levels)) + ', pixels = new System.Collections.Generic.List<int> {'
            + ','.join(map(str, atlas)) + '} }')
