#!/usr/bin/env python3
import os
import random
import uuid
from PIL import Image, ImageDraw

BASE_DIR = "/root/IGROSTROY/L_A_B/Game-Lab/Assets"
RES_DIR = os.path.join(BASE_DIR, "Resources", "Sprites")

def ensure_dir(path):
    if not os.path.exists(path):
        os.makedirs(path)

def generate_guid():
    return uuid.uuid4().hex

def write_meta(file_path, ppu=64):
    meta_path = file_path + ".meta"
    if os.path.exists(meta_path):
        return
    guid = generate_guid()
    meta_template = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vRAMBudget: 1024
  sRGBTexture: 1
  ignoreMasterTextureLimit: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  applyFlags: 0
  platformSettings: []
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta_template.format(guid=guid, ppu=ppu))

def save_sprite(img, category, name, ppu=64):
    cat_dir = os.path.join(RES_DIR, category)
    ensure_dir(cat_dir)
    file_path = os.path.join(cat_dir, f"{name}.png")
    img.save(file_path, "PNG")
    write_meta(file_path, ppu)
    print(f"[Generated] {category}/{name}.png")

def clamp(val, min_val=0, max_val=255):
    return max(min_val, min(max_val, int(val)))

def blend_color(c1, c2, t):
    return (
        clamp(c1[0] + (c2[0] - c1[0]) * t),
        clamp(c1[1] + (c2[1] - c1[1]) * t),
        clamp(c1[2] + (c2[2] - c1[2]) * t),
        c1[3] if len(c1) > 3 else 255
    )

def add_noise(color, variance=15):
    r = clamp(color[0] + random.randint(-variance, variance))
    g = clamp(color[1] + random.randint(-variance, variance))
    b = clamp(color[2] + random.randint(-variance, variance))
    a = color[3] if len(color) > 3 else 255
    return (r, g, b, a)

def make_textured_tile(base_col, dark_col, light_col, noise_var=12, bevel=True):
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    pixels = img.load()
    for y in range(64):
        for x in range(64):
            c = add_noise(base_col, noise_var)
            if random.random() < 0.12:
                c = blend_color(c, dark_col, 0.4)
            elif random.random() < 0.08:
                c = blend_color(c, light_col, 0.4)
            pixels[x, y] = c
            
    if bevel:
        for x in range(64):
            pixels[x, 0] = blend_color(pixels[x, 0], light_col, 0.5)
            pixels[x, 1] = blend_color(pixels[x, 1], light_col, 0.25)
            pixels[x, 63] = blend_color(pixels[x, 63], dark_col, 0.6)
            pixels[x, 62] = blend_color(pixels[x, 62], dark_col, 0.3)
        for y in range(64):
            pixels[0, y] = blend_color(pixels[0, y], light_col, 0.5)
            pixels[1, y] = blend_color(pixels[1, y], light_col, 0.25)
            pixels[63, y] = blend_color(pixels[63, y], dark_col, 0.6)
            pixels[62, y] = blend_color(pixels[62, y], dark_col, 0.3)
    return img

def make_ore_tile(stone_img, vein_color, shine_color, num_veins=8):
    img = stone_img.copy()
    draw = ImageDraw.Draw(img)
    random.seed(sum(vein_color))
    for _ in range(num_veins):
        cx = random.randint(12, 52)
        cy = random.randint(12, 52)
        rx = random.randint(4, 10)
        ry = random.randint(3, 8)
        draw.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=add_noise(vein_color, 15), outline=blend_color(vein_color, (0,0,0,255), 0.5))
        for _ in range(3):
            fx = cx + random.randint(-rx+2, rx-2)
            fy = cy + random.randint(-ry+2, ry-2)
            draw.point((fx, fy), fill=shine_color)
    return img

def make_stairs_tile(is_up=True, is_both=False):
    img = make_textured_tile((110, 110, 115, 255), (60, 60, 65, 255), (160, 160, 165, 255))
    draw = ImageDraw.Draw(img)
    step_count = 6
    h = 64 // step_count
    for i in range(step_count):
        y = i * h
        step_col = blend_color((130, 130, 135, 255), (70, 70, 75, 255), i / float(step_count))
        draw.rectangle([4, y, 59, y + h - 1], fill=step_col, outline=(40, 40, 45, 255))
        draw.line([4, y, 59, y], fill=(180, 180, 185, 255))
        draw.line([4, y + h - 1, 59, y + h - 1], fill=(40, 40, 45, 255))
    
    if is_up or is_both:
        draw.polygon([(32, 10), (20, 24), (28, 24), (28, 38), (36, 38), (36, 24), (44, 24)], fill=(240, 210, 80, 255), outline=(30, 30, 30, 255))
    if not is_up or is_both:
        draw.polygon([(32, 54), (20, 40), (28, 40), (28, 26), (36, 26), (36, 40), (44, 40)], fill=(80, 160, 240, 255), outline=(30, 30, 30, 255))
    return img

def make_wall_tile(material_type="wood"):
    if material_type == "wood":
        img = make_textured_tile((130, 82, 45, 255), (85, 50, 25, 255), (175, 115, 65, 255), 15, bevel=False)
        draw = ImageDraw.Draw(img)
        for y in range(0, 64, 16):
            draw.line([0, y, 63, y], fill=(85, 50, 25, 255), width=2)
            draw.line([0, y+1, 63, y+1], fill=(175, 115, 65, 255), width=1)
            draw.ellipse([8, y+6, 12, y+10], fill=(40, 30, 20, 255))
            draw.ellipse([52, y+6, 56, y+10], fill=(40, 30, 20, 255))
        return img
    elif material_type == "stone":
        img = make_textured_tile((120, 122, 125, 255), (65, 66, 68, 255), (165, 168, 170, 255), 10, bevel=False)
        draw = ImageDraw.Draw(img)
        row_h = 16
        for row in range(4):
            y = row * row_h
            draw.line([0, y, 63, y], fill=(65, 66, 68, 255), width=2)
            offset = 16 if row % 2 == 1 else 0
            for col in range(3):
                x = col * 32 + offset
                if 0 <= x < 64:
                    draw.line([x, y, x, y + row_h], fill=(65, 66, 68, 255), width=2)
        return img
    elif material_type == "brick":
        img = make_textured_tile((165, 65, 48, 255), (95, 35, 25, 255), (205, 95, 75, 255), 10, bevel=False)
        draw = ImageDraw.Draw(img)
        row_h = 10
        for row in range(7):
            y = row * row_h
            draw.line([0, y, 63, y], fill=(180, 180, 175, 255), width=2)
            offset = 12 if row % 2 == 1 else 0
            for col in range(6):
                x = col * 24 + offset
                if 0 <= x < 64:
                    draw.line([x, y, x, y + row_h], fill=(180, 180, 175, 255), width=2)
        return img

def make_door_tile(material="wood"):
    img = make_wall_tile("stone")
    draw = ImageDraw.Draw(img)
    draw.rectangle([12, 4, 51, 63], fill=(30, 25, 20, 255))
    if material == "wood":
        draw.rectangle([16, 8, 47, 63], fill=(140, 85, 40, 255), outline=(75, 45, 20, 255))
        draw.line([26, 8, 26, 63], fill=(75, 45, 20, 255), width=2)
        draw.line([37, 8, 37, 63], fill=(75, 45, 20, 255), width=2)
        draw.rectangle([14, 16, 22, 20], fill=(40, 40, 40, 255))
        draw.rectangle([14, 48, 22, 52], fill=(40, 40, 40, 255))
        draw.ellipse([42, 34, 46, 38], fill=(220, 190, 60, 255), outline=(60, 50, 20, 255))
    else:
        draw.rectangle([16, 8, 47, 63], fill=(95, 100, 108, 255), outline=(45, 48, 52, 255))
        draw.line([16, 8, 47, 63], fill=(50, 52, 56, 255), width=2)
        draw.line([16, 63, 47, 8], fill=(50, 52, 56, 255), width=2)
        draw.ellipse([42, 34, 46, 38], fill=(190, 195, 200, 255), outline=(30, 30, 30, 255))
    return img

def make_floor_tile(material="wood"):
    if material == "wood":
        img = make_textured_tile((170, 120, 75, 255), (110, 75, 40, 255), (205, 155, 105, 255), 10, bevel=True)
        draw = ImageDraw.Draw(img)
        for y in range(0, 64, 16):
            draw.line([0, y, 63, y], fill=(110, 75, 40, 255), width=1)
            for x in range((y//16)%2 * 16, 64, 32):
                draw.line([x, y, x, y + 16], fill=(110, 75, 40, 255), width=1)
        return img
    else:
        img = make_textured_tile((150, 152, 155, 255), (90, 92, 95, 255), (190, 192, 195, 255), 8, bevel=True)
        draw = ImageDraw.Draw(img)
        for y in range(0, 64, 20):
            draw.line([0, y, 63, y], fill=(90, 92, 95, 255), width=1)
        for x in range(0, 64, 20):
            draw.line([x, 0, x, 63], fill=(90, 92, 95, 255), width=1)
        return img

def generate_all_tiles():
    print("Generating Tiles...")
    save_sprite(make_textured_tile((68, 142, 52, 255), (42, 95, 32, 255), (105, 185, 75, 255), 15), "Tiles", "Grass")
    save_sprite(make_textured_tile((128, 85, 52, 255), (82, 52, 30, 255), (165, 115, 75, 255), 18), "Tiles", "Dirt")
    save_sprite(make_textured_tile((215, 185, 120, 255), (170, 140, 85, 255), (242, 220, 160, 255), 12), "Tiles", "Sand")
    save_sprite(make_textured_tile((75, 55, 42, 255), (45, 32, 22, 255), (105, 80, 60, 255), 15), "Tiles", "Mud")
    save_sprite(make_textured_tile((45, 110, 185, 220), (25, 70, 135, 240), (85, 160, 235, 200), 10), "Tiles", "Water")
    save_sprite(make_textured_tile((35, 35, 40, 255), (18, 18, 22, 255), (58, 58, 65, 255), 12), "Tiles", "Bedrock")

    granite = make_textured_tile((135, 130, 132, 255), (85, 80, 82, 255), (180, 175, 178, 255), 16)
    save_sprite(granite, "Tiles", "Granite")
    save_sprite(make_textured_tile((205, 198, 175, 255), (150, 145, 125, 255), (235, 230, 210, 255), 12), "Tiles", "Limestone")
    save_sprite(make_textured_tile((52, 50, 58, 255), (28, 26, 32, 255), (80, 78, 90, 255), 14), "Tiles", "Basalt")

    marble = make_textured_tile((225, 228, 232, 255), (175, 178, 185, 255), (250, 252, 255, 255), 8)
    m_draw = ImageDraw.Draw(marble)
    for _ in range(4):
        m_draw.line([(random.randint(0, 63), 0), (random.randint(0, 63), 63)], fill=(160, 155, 140, 120), width=1)
    save_sprite(marble, "Tiles", "Marble")

    save_sprite(make_ore_tile(granite, (20, 20, 22, 255), (70, 70, 80, 255), num_veins=10), "Tiles", "Coal_Ore")
    save_sprite(make_ore_tile(granite, (165, 78, 45, 255), (230, 130, 85, 255), num_veins=8), "Tiles", "Iron_Ore")
    save_sprite(make_ore_tile(granite, (45, 145, 135, 255), (95, 215, 195, 255), num_veins=8), "Tiles", "Copper_Ore")
    save_sprite(make_ore_tile(granite, (230, 185, 35, 255), (255, 240, 120, 255), num_veins=9), "Tiles", "Gold_Ore")
    save_sprite(make_ore_tile(granite, (200, 225, 245, 255), (250, 255, 255, 255), num_veins=9), "Tiles", "Silver_Ore")

    save_sprite(make_stairs_tile(is_up=True), "Tiles", "Stairs_Up")
    save_sprite(make_stairs_tile(is_up=False), "Tiles", "Stairs_Down")
    save_sprite(make_stairs_tile(is_both=True), "Tiles", "Stairs_Both")

    save_sprite(make_wall_tile("wood"), "Tiles", "Wall_Wood")
    save_sprite(make_wall_tile("stone"), "Tiles", "Wall_Stone")
    save_sprite(make_wall_tile("brick"), "Tiles", "Wall_Brick")
    save_sprite(make_floor_tile("wood"), "Tiles", "Floor_Wood")
    save_sprite(make_floor_tile("stone"), "Tiles", "Floor_Stone")
    save_sprite(make_door_tile("wood"), "Tiles", "Door_Wood")
    save_sprite(make_door_tile("iron"), "Tiles", "Door_Iron")

def make_tree_oak():
    img = Image.new("RGBA", (64, 96), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.polygon([(26, 92), (38, 92), (35, 45), (29, 45)], fill=(105, 68, 38, 255), outline=(55, 35, 18, 255))
    draw.line([(26, 92), (20, 95)], fill=(55, 35, 18, 255), width=3)
    draw.line([(38, 92), (44, 95)], fill=(55, 35, 18, 255), width=3)
    blobs = [(32, 42, 24, 22), (22, 32, 18, 16), (42, 32, 18, 16), (32, 22, 20, 18), (26, 16, 14, 14), (38, 16, 14, 14), (32, 10, 12, 10)]
    for bx, by, rx, ry in blobs:
        draw.ellipse([bx - rx, by - ry, bx + rx, by + ry], fill=(35, 85, 30, 255))
    for bx, by, rx, ry in blobs:
        draw.ellipse([bx - rx + 2, by - ry + 2, bx + rx - 2, by + ry - 4], fill=(55, 125, 45, 255))
    for bx, by, rx, ry in blobs:
        draw.ellipse([bx - rx + 4, by - ry + 2, bx + rx - 6, by + ry - 8], fill=(85, 165, 65, 255))
    return img

def make_tree_pine():
    img = Image.new("RGBA", (64, 96), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rectangle([29, 70, 35, 94], fill=(85, 52, 28, 255), outline=(45, 25, 12, 255))
    layers = [(90, 70, 24), (72, 54, 20), (56, 40, 16), (42, 28, 12), (28, 16, 8), (16, 6, 4)]
    for bottom_y, top_y, half_w in layers:
        draw.polygon([(32 - half_w, bottom_y), (32 + half_w, bottom_y), (32, top_y)], fill=(25, 65, 40, 255), outline=(15, 40, 25, 255))
        draw.polygon([(32 - half_w + 3, bottom_y - 2), (32 + half_w - 3, bottom_y - 2), (32, top_y + 2)], fill=(40, 95, 55, 255))
        draw.polygon([(32 - half_w + 6, bottom_y - 4), (32, bottom_y - 4), (32, top_y + 4)], fill=(65, 135, 80, 255))
    return img

def make_tree_palm():
    img = Image.new("RGBA", (64, 96), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    pts = [(28, 94), (36, 94), (38, 70), (42, 45), (38, 30), (32, 30), (34, 45), (30, 70)]
    draw.polygon(pts, fill=(135, 95, 55, 255), outline=(75, 50, 25, 255))
    draw.arc([10, 15, 60, 55], start=180, end=360, fill=(45, 105, 35, 255), width=5)
    draw.arc([5, 20, 45, 65], start=160, end=340, fill=(75, 155, 55, 255), width=4)
    draw.arc([25, 20, 62, 65], start=200, end=380, fill=(75, 155, 55, 255), width=4)
    draw.arc([15, 5, 55, 45], start=190, end=350, fill=(105, 185, 75, 255), width=3)
    draw.ellipse([32, 32, 36, 36], fill=(85, 50, 25, 255))
    draw.ellipse([36, 33, 40, 37], fill=(85, 50, 25, 255))
    return img

def make_tree_dead():
    img = Image.new("RGBA", (64, 96), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.polygon([(28, 92), (36, 92), (34, 50), (30, 50)], fill=(80, 75, 70, 255), outline=(40, 38, 35, 255))
    draw.line([(32, 65), (18, 48), (12, 35)], fill=(80, 75, 70, 255), width=3)
    draw.line([(18, 48), (22, 32)], fill=(80, 75, 70, 255), width=2)
    draw.line([(32, 50), (46, 36), (52, 22)], fill=(80, 75, 70, 255), width=3)
    draw.line([(32, 50), (32, 28), (26, 14)], fill=(80, 75, 70, 255), width=3)
    return img

def make_berry_bush():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([10, 18, 54, 58], fill=(30, 80, 25, 255), outline=(18, 50, 15, 255))
    draw.ellipse([14, 20, 50, 54], fill=(45, 115, 40, 255))
    draw.ellipse([18, 22, 42, 42], fill=(70, 155, 60, 255))
    random.seed(42)
    for _ in range(14):
        bx = random.randint(16, 48)
        by = random.randint(22, 52)
        draw.ellipse([bx, by, bx+4, by+4], fill=(225, 30, 45, 255), outline=(130, 15, 25, 255))
        draw.point((bx+1, by+1), fill=(255, 160, 170, 255))
    return img

def make_crops():
    c0 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d0 = ImageDraw.Draw(c0)
    d0.ellipse([12, 45, 52, 60], fill=(95, 60, 35, 255))
    for x in [22, 32, 42]:
        d0.line([(x, 50), (x - 2, 40)], fill=(75, 175, 60, 255), width=2)
        d0.line([(x, 50), (x + 3, 38)], fill=(95, 195, 80, 255), width=2)

    c1 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d1 = ImageDraw.Draw(c1)
    d1.ellipse([12, 45, 52, 60], fill=(95, 60, 35, 255))
    for x in [20, 28, 36, 44]:
        d1.line([(x, 50), (x, 24)], fill=(85, 165, 50, 255), width=2)
        d1.line([(x, 34), (x-4, 28)], fill=(115, 195, 70, 255), width=2)
        d1.line([(x, 30), (x+4, 24)], fill=(115, 195, 70, 255), width=2)

    c2 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d2 = ImageDraw.Draw(c2)
    d2.ellipse([12, 45, 52, 60], fill=(95, 60, 35, 255))
    for x in [18, 26, 34, 42, 48]:
        d2.line([(x, 50), (x, 18)], fill=(205, 155, 45, 255), width=2)
        d2.ellipse([x - 3, 12, x + 3, 24], fill=(235, 190, 65, 255), outline=(160, 110, 25, 255))
        d2.line([(x-3, 14), (x-5, 10)], fill=(245, 215, 95, 255), width=1)
        d2.line([(x+3, 14), (x+5, 10)], fill=(245, 215, 95, 255), width=1)
    return c0, c1, c2

def make_cotton_plant():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.line([(32, 54), (32, 28)], fill=(65, 115, 45, 255), width=3)
    draw.line([(32, 44), (20, 36)], fill=(65, 115, 45, 255), width=2)
    draw.line([(32, 38), (44, 30)], fill=(65, 115, 45, 255), width=2)
    for bx, by in [(32, 24), (20, 34), (44, 28), (26, 40), (38, 36)]:
        draw.ellipse([bx - 6, by - 6, bx + 6, by + 6], fill=(245, 245, 250, 255), outline=(190, 190, 195, 255))
        draw.ellipse([bx - 4, by - 4, bx + 2, by + 2], fill=(255, 255, 255, 255))
    return img

def make_herb_plant():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.line([(32, 56), (32, 20)], fill=(55, 125, 65, 255), width=2)
    for fx, fy in [(32, 16), (24, 26), (40, 26), (28, 36), (36, 36)]:
        draw.ellipse([fx - 4, fy - 4, fx + 4, fy + 4], fill=(165, 85, 215, 255), outline=(95, 40, 135, 255))
        draw.ellipse([fx - 1, fy - 1, fx + 1, fy + 1], fill=(245, 215, 80, 255))
    return img

def make_mushroom():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rectangle([28, 38, 36, 56], fill=(225, 220, 205, 255), outline=(160, 155, 140, 255))
    draw.rectangle([18, 44, 24, 56], fill=(215, 210, 195, 255), outline=(150, 145, 130, 255))
    draw.pieslice([18, 18, 46, 44], 180, 360, fill=(195, 45, 35, 255), outline=(115, 25, 20, 255))
    draw.pieslice([12, 30, 30, 48], 180, 360, fill=(175, 55, 45, 255), outline=(105, 30, 25, 255))
    for sx, sy in [(26, 24), (34, 22), (38, 27), (24, 30)]:
        draw.ellipse([sx, sy, sx+3, sy+3], fill=(255, 255, 255, 255))
    return img

def make_natural_objects():
    boulder = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    db = ImageDraw.Draw(boulder)
    db.ellipse([10, 18, 54, 52], fill=(125, 128, 132, 255), outline=(65, 68, 72, 255))
    db.ellipse([14, 22, 46, 42], fill=(160, 164, 168, 255))
    db.line([(18, 32), (32, 38), (42, 34)], fill=(75, 78, 82, 255), width=2)
    
    log = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    dl = ImageDraw.Draw(log)
    dl.rectangle([8, 24, 52, 42], fill=(115, 75, 40, 255), outline=(65, 40, 20, 255))
    dl.ellipse([46, 24, 54, 42], fill=(155, 105, 60, 255), outline=(65, 40, 20, 255))
    dl.rectangle([14, 24, 26, 28], fill=(65, 135, 45, 255))
    return boulder, log

def generate_all_flora():
    print("Generating Flora & Natural Objects...")
    save_sprite(make_tree_oak(), "Flora", "Tree_Oak", ppu=64)
    save_sprite(make_tree_pine(), "Flora", "Tree_Pine", ppu=64)
    save_sprite(make_tree_palm(), "Flora", "Tree_Palm", ppu=64)
    save_sprite(make_tree_dead(), "Flora", "Tree_Dead", ppu=64)
    save_sprite(make_berry_bush(), "Flora", "Berry_Bush", ppu=64)
    c0, c1, c2 = make_crops()
    save_sprite(c0, "Flora", "Crop_Wheat_0", ppu=64)
    save_sprite(c1, "Flora", "Crop_Wheat_1", ppu=64)
    save_sprite(c2, "Flora", "Crop_Wheat_2", ppu=64)
    save_sprite(make_cotton_plant(), "Flora", "Crop_Cotton", ppu=64)
    save_sprite(make_herb_plant(), "Flora", "Plant_Herb", ppu=64)
    save_sprite(make_mushroom(), "Flora", "Mushroom", ppu=64)
    boulder, log = make_natural_objects()
    save_sprite(boulder, "Flora", "Stone_Boulder", ppu=64)
    save_sprite(log, "Flora", "Fallen_Log", ppu=64)

def make_workbench_base(table_color, outline_color, leg_color=(60, 40, 25, 255)):
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([6, 44, 58, 60], fill=(20, 20, 25, 110))
    draw.rectangle([10, 32, 16, 54], fill=leg_color, outline=(30, 20, 15, 255))
    draw.rectangle([48, 32, 54, 54], fill=leg_color, outline=(30, 20, 15, 255))
    draw.line([14, 46, 50, 46], fill=leg_color, width=3)
    draw.rectangle([6, 22, 58, 34], fill=table_color, outline=outline_color)
    draw.line([7, 23, 57, 23], fill=blend_color(table_color, (255, 255, 255, 255), 0.3), width=1)
    draw.line([7, 33, 57, 33], fill=blend_color(table_color, (0, 0, 0, 255), 0.4), width=1)
    return img

def generate_all_workbenches():
    print("Generating 25 Workbenches...")
    wood_table = (150, 95, 55, 255)
    wood_outline = (80, 45, 25, 255)
    stone_table = (130, 132, 138, 255)
    stone_outline = (65, 68, 72, 255)
    metal_table = (95, 100, 110, 255)
    metal_outline = (45, 48, 55, 255)

    wb1 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb1)
    d.line([(14, 16), (28, 20)], fill=(190, 195, 205, 255), width=3)
    d.rectangle([12, 14, 16, 18], fill=(90, 50, 25, 255))
    d.rectangle([34, 16, 44, 22], fill=(180, 120, 70, 255), outline=wood_outline)
    save_sprite(wb1, "Workbenches", "carpenter_bench")

    wb2 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb2)
    d.ellipse([22, 6, 42, 26], fill=(185, 190, 200, 255), outline=(90, 95, 105, 255))
    d.ellipse([30, 14, 34, 18], fill=(50, 50, 55, 255))
    save_sprite(wb2, "Workbenches", "sawmill")

    wb3 = make_workbench_base(stone_table, stone_outline, leg_color=(75, 78, 82, 255))
    d = ImageDraw.Draw(wb3)
    d.rectangle([16, 12, 32, 22], fill=(175, 178, 182, 255), outline=stone_outline)
    d.line([(36, 14), (44, 22)], fill=(170, 175, 185, 255), width=2)
    save_sprite(wb3, "Workbenches", "stonecutter_table")

    wb4 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb4)
    d.ellipse([8, 46, 56, 60], fill=(20, 20, 25, 120))
    d.polygon([(14, 52), (50, 52), (44, 14), (20, 14)], fill=(95, 90, 85, 255), outline=(45, 42, 40, 255))
    d.ellipse([24, 38, 40, 50], fill=(255, 100, 20, 255), outline=(140, 40, 10, 255))
    d.ellipse([28, 42, 36, 48], fill=(255, 230, 80, 255))
    save_sprite(wb4, "Workbenches", "smelter")

    wb5 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb5)
    d.ellipse([6, 46, 58, 60], fill=(20, 20, 25, 120))
    d.rectangle([18, 34, 46, 54], fill=(105, 68, 38, 255), outline=(55, 35, 18, 255))
    d.polygon([(12, 22), (24, 22), (24, 26), (28, 30), (36, 30), (40, 26), (50, 22), (54, 26), (46, 34), (18, 34)], fill=(90, 95, 105, 255), outline=(45, 48, 55, 255))
    d.line([(26, 21), (38, 21)], fill=(255, 120, 30, 255), width=3)
    save_sprite(wb5, "Workbenches", "blacksmith_anvil")

    wb6 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb6)
    d.polygon([(12, 18), (36, 18), (42, 20), (36, 22), (12, 22)], fill=(200, 205, 215, 255), outline=metal_outline)
    d.rectangle([42, 14, 54, 20], fill=(105, 108, 115, 255))
    save_sprite(wb6, "Workbenches", "weaponsmith_bench")

    wb7 = make_workbench_base(stone_table, stone_outline)
    d = ImageDraw.Draw(wb7)
    d.polygon([(24, 8), (40, 8), (44, 22), (20, 22)], fill=(175, 180, 192, 255), outline=(75, 80, 90, 255))
    save_sprite(wb7, "Workbenches", "armorsmith_forge")

    wb8 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb8)
    d.ellipse([8, 48, 56, 58], fill=(20, 20, 25, 110))
    d.rectangle([10, 10, 16, 54], fill=(120, 75, 40, 255), outline=(60, 35, 18, 255))
    d.rectangle([48, 10, 54, 54], fill=(120, 75, 40, 255), outline=(60, 35, 18, 255))
    d.rectangle([10, 10, 54, 16], fill=(120, 75, 40, 255), outline=(60, 35, 18, 255))
    d.rectangle([10, 48, 54, 54], fill=(120, 75, 40, 255), outline=(60, 35, 18, 255))
    d.polygon([(20, 18), (44, 18), (48, 32), (42, 46), (22, 46), (16, 32)], fill=(175, 125, 75, 255), outline=(105, 65, 30, 255))
    save_sprite(wb8, "Workbenches", "tanning_rack")

    wb9 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb9)
    d.ellipse([14, 14, 24, 22], fill=(145, 95, 50, 255), outline=(85, 50, 25, 255))
    d.line([(42, 16), (50, 16)], fill=(195, 200, 210, 255), width=2)
    save_sprite(wb9, "Workbenches", "leatherworker_bench")

    wb10 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb10)
    d.rectangle([12, 8, 18, 54], fill=(130, 85, 45, 255))
    d.rectangle([46, 8, 52, 54], fill=(130, 85, 45, 255))
    d.rectangle([12, 8, 52, 14], fill=(130, 85, 45, 255))
    d.rectangle([12, 46, 52, 52], fill=(130, 85, 45, 255))
    for x in range(20, 46, 4):
        d.line([(x, 14), (x, 46)], fill=(225, 225, 230, 255), width=1)
    d.rectangle([20, 30, 44, 46], fill=(55, 120, 195, 255))
    save_sprite(wb10, "Workbenches", "loom")

    wb11 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb11)
    d.rectangle([12, 12, 28, 22], fill=(185, 55, 70, 255), outline=(110, 30, 40, 255))
    d.line([(34, 14), (44, 20)], fill=(190, 195, 205, 255), width=2)
    d.line([(34, 20), (44, 14)], fill=(190, 195, 205, 255), width=2)
    save_sprite(wb11, "Workbenches", "tailor_bench")

    wb12 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb12)
    d.rectangle([12, 16, 52, 52], fill=(125, 110, 95, 255), outline=(65, 55, 45, 255))
    d.rectangle([38, 4, 48, 16], fill=(110, 95, 82, 255), outline=(65, 55, 45, 255))
    d.ellipse([24, 38, 40, 48], fill=(255, 130, 20, 255))
    d.ellipse([28, 42, 36, 46], fill=(255, 235, 80, 255))
    d.rectangle([18, 10, 32, 18], fill=(50, 52, 58, 255))
    save_sprite(wb12, "Workbenches", "cooking_stove")

    wb13 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb13)
    d.polygon([(12, 22), (28, 22), (32, 34), (28, 48), (12, 48), (8, 34)], fill=(135, 85, 45, 255), outline=(65, 40, 20, 255))
    d.ellipse([34, 18, 54, 48], fill=(195, 95, 50, 255), outline=(115, 50, 25, 255))
    save_sprite(wb13, "Workbenches", "brewery")

    wb14 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb14)
    d.ellipse([14, 28, 50, 50], fill=(145, 148, 155, 255), outline=(75, 78, 85, 255))
    d.ellipse([18, 20, 46, 38], fill=(175, 178, 185, 255), outline=(75, 78, 85, 255))
    d.line([(32, 26), (44, 16)], fill=(120, 75, 40, 255), width=3)
    save_sprite(wb14, "Workbenches", "mill")

    wb15 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb15)
    d.ellipse([14, 14, 24, 22], fill=(190, 195, 200, 255))
    d.rectangle([30, 10, 36, 20], fill=(85, 215, 125, 220))
    d.rectangle([40, 10, 46, 20], fill=(215, 65, 85, 220))
    save_sprite(wb15, "Workbenches", "apothecary_table")

    wb16 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb16)
    d.ellipse([14, 8, 26, 20], fill=(210, 235, 255, 180), outline=(215, 175, 55, 255))
    d.polygon([(32, 14), (36, 10), (40, 14), (36, 18)], fill=(235, 45, 65, 255))
    d.ellipse([50, 14, 56, 20], outline=(245, 200, 50, 255), width=2)
    save_sprite(wb16, "Workbenches", "jeweler_bench")

    wb17 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb17)
    d.ellipse([12, 34, 52, 52], fill=(95, 65, 40, 255))
    d.ellipse([18, 22, 46, 36], fill=(160, 165, 175, 255))
    d.polygon([(26, 24), (38, 24), (42, 14), (38, 10), (26, 10), (22, 14)], fill=(185, 95, 55, 255))
    save_sprite(wb17, "Workbenches", "potter_wheel")

    wb18 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb18)
    d.ellipse([10, 12, 54, 52], fill=(165, 65, 48, 255), outline=(85, 30, 20, 255))
    d.pieslice([22, 28, 42, 52], 180, 360, fill=(255, 140, 30, 255))
    save_sprite(wb18, "Workbenches", "kiln")

    wb19 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb19)
    d.rectangle([14, 16, 50, 52], fill=(110, 115, 125, 255), outline=(55, 60, 68, 255))
    d.ellipse([22, 24, 42, 44], fill=(255, 175, 40, 255))
    d.line([(8, 48), (32, 34)], fill=(195, 200, 210, 255), width=3)
    save_sprite(wb19, "Workbenches", "glassmaker_furnace")

    wb20 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb20)
    d.ellipse([14, 10, 28, 24], fill=(130, 85, 45, 255))
    d.line([(28, 17), (56, 17)], fill=(195, 165, 105, 255), width=4)
    save_sprite(wb20, "Workbenches", "ropewalk")

    wb21 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb21)
    d.rectangle([14, 12, 30, 22], fill=(175, 125, 75, 255), outline=(85, 55, 25, 255))
    d.rectangle([34, 10, 46, 16], fill=(110, 70, 35, 255))
    save_sprite(wb21, "Workbenches", "joiner_workshop")

    wb22 = make_workbench_base(stone_table, stone_outline)
    d = ImageDraw.Draw(wb22)
    d.rectangle([14, 12, 42, 22], fill=(125, 80, 45, 255))
    d.rectangle([46, 14, 56, 18], fill=(175, 65, 45, 255))
    save_sprite(wb22, "Workbenches", "brickmaker")

    wb23 = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(wb23)
    d.ellipse([12, 16, 52, 48], fill=(120, 75, 40, 255))
    d.ellipse([16, 20, 48, 44], fill=(75, 35, 145, 255))
    d.line([(42, 22), (54, 10)], fill=(150, 95, 50, 255), width=4)
    save_sprite(wb23, "Workbenches", "dye_workshop")

    wb24 = make_workbench_base(wood_table, wood_outline)
    d = ImageDraw.Draw(wb24)
    d.rectangle([14, 10, 36, 22], fill=(165, 115, 70, 255))
    d.polygon([(18, 12), (32, 12), (30, 18), (16, 18)], fill=(195, 45, 55, 255))
    d.rectangle([38, 8, 48, 16], fill=(190, 195, 205, 255))
    save_sprite(wb24, "Workbenches", "butcher_table")

    wb25 = make_workbench_base(metal_table, metal_outline)
    d = ImageDraw.Draw(wb25)
    d.ellipse([16, 10, 30, 24], fill=(215, 155, 55, 255), outline=(125, 85, 25, 255))
    d.line([(42, 20), (52, 10)], fill=(160, 165, 175, 255), width=3)
    save_sprite(wb25, "Workbenches", "mechanic_bench")

def make_character_frame(skin_color, hair_color, shirt_color, pants_color, hat_type, tool_type, frame="idle"):
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([20, 52, 44, 60], fill=(20, 20, 25, 110))

    leg_l = (25, 44, 29, 54)
    leg_r = (35, 44, 39, 54)
    arm_l = [(18, 28), (18, 40)]
    arm_r = [(46, 28), (46, 40)]
    body_y = 26

    if frame == "walk_0":
        leg_l = (23, 42, 27, 52)
        leg_r = (37, 46, 41, 56)
        arm_l = [(16, 26), (14, 36)]
        arm_r = [(48, 30), (50, 42)]
    elif frame == "walk_1":
        leg_l = (23, 46, 27, 56)
        leg_r = (37, 42, 41, 52)
        arm_l = [(16, 30), (14, 42)]
        arm_r = [(48, 26), (50, 36)]
    elif frame == "action":
        body_y = 25
        arm_r = [(46, 24), (52, 14)]

    draw.rectangle([leg_l[0], leg_l[1], leg_l[2], leg_l[3]], fill=pants_color, outline=(30, 25, 20, 255))
    draw.rectangle([leg_r[0], leg_r[1], leg_r[2], leg_r[3]], fill=pants_color, outline=(30, 25, 20, 255))
    draw.rectangle([leg_l[0]-1, leg_l[3]-4, leg_l[2]+1, leg_l[3]], fill=(65, 45, 28, 255))
    draw.rectangle([leg_r[0]-1, leg_r[3]-4, leg_r[2]+1, leg_r[3]], fill=(65, 45, 28, 255))

    draw.rectangle([22, body_y, 42, body_y + 18], fill=shirt_color, outline=(35, 30, 25, 255))
    draw.line([22, body_y + 15, 42, body_y + 15], fill=(75, 45, 25, 255), width=3)
    draw.rectangle([30, body_y + 14, 34, body_y + 17], fill=(225, 195, 60, 255))

    draw.line(arm_l, fill=shirt_color, width=4)
    draw.line(arm_r, fill=shirt_color, width=4)
    draw.ellipse([arm_l[1][0]-2, arm_l[1][1]-2, arm_l[1][0]+2, arm_l[1][1]+2], fill=skin_color)
    draw.ellipse([arm_r[1][0]-2, arm_r[1][1]-2, arm_r[1][0]+2, arm_r[1][1]+2], fill=skin_color)

    head_rect = [24, body_y - 14, 40, body_y + 2]
    draw.ellipse(head_rect, fill=skin_color, outline=(130, 90, 60, 255))
    draw.point((28, body_y - 8), fill=(30, 30, 40, 255))
    draw.point((36, body_y - 8), fill=(30, 30, 40, 255))
    if hair_color:
        draw.polygon([(26, body_y - 4), (38, body_y - 4), (35, body_y + 6), (29, body_y + 6)], fill=hair_color)
        draw.arc([22, body_y - 16, 42, body_y - 4], 180, 360, fill=hair_color, width=3)

    if hat_type == "helmet_iron":
        draw.pieslice([22, body_y - 18, 42, body_y - 2], 180, 360, fill=(160, 165, 175, 255), outline=(60, 65, 75, 255))
        draw.line([32, body_y - 10, 32, body_y - 4], fill=(160, 165, 175, 255), width=2)
    elif hat_type == "miner_cap":
        draw.pieslice([22, body_y - 16, 42, body_y - 2], 180, 360, fill=(185, 125, 45, 255), outline=(75, 45, 15, 255))
        draw.ellipse([30, body_y - 12, 34, body_y - 8], fill=(255, 240, 100, 255))
    elif hat_type == "hood_green":
        draw.arc([22, body_y - 16, 42, body_y - 2], 180, 360, fill=(45, 115, 55, 255), width=4)

    hx, hy = arm_r[1][0], arm_r[1][1]
    if tool_type == "sword":
        if frame == "action":
            draw.line([(hx, hy), (hx + 14, hy - 14)], fill=(215, 220, 230, 255), width=3)
        else:
            draw.line([(hx, hy), (hx, hy + 14)], fill=(205, 210, 220, 255), width=2)
            draw.line([(hx - 3, hy + 2), (hx + 3, hy + 2)], fill=(215, 175, 55, 255), width=2)
    elif tool_type == "pickaxe":
        if frame == "action":
            draw.line([(hx, hy), (hx + 12, hy - 10)], fill=(120, 75, 40, 255), width=3)
            draw.arc([hx + 6, hy - 18, hx + 18, hy - 4], 45, 225, fill=(180, 185, 195, 255), width=3)
        else:
            draw.line([(hx, hy), (hx, hy + 14)], fill=(120, 75, 40, 255), width=2)
            draw.arc([hx - 6, hy + 8, hx + 6, hy + 20], 45, 225, fill=(180, 185, 195, 255), width=3)
    elif tool_type == "hammer":
        draw.line([(hx, hy), (hx, hy + 12)], fill=(120, 75, 40, 255), width=2)
        draw.rectangle([hx - 3, hy + 10, hx + 4, hy + 14], fill=(70, 75, 85, 255))
    elif tool_type == "axe":
        draw.line([(hx, hy), (hx, hy + 14)], fill=(120, 75, 40, 255), width=2)
        draw.polygon([(hx + 1, hy + 2), (hx + 7, hy - 1), (hx + 6, hy + 6)], fill=(180, 185, 195, 255))
    return img

def generate_all_characters():
    print("Generating Squad Characters...")
    classes = [
        ("warrior", (235, 190, 160, 255), (145, 95, 45, 255), (65, 105, 175, 255), (55, 60, 70, 255), "helmet_iron", "sword"),
        ("miner", (225, 180, 150, 255), (75, 45, 25, 255), (155, 95, 45, 255), (65, 65, 75, 255), "miner_cap", "pickaxe"),
        ("builder", (230, 185, 155, 255), (185, 135, 55, 255), (185, 55, 50, 255), (55, 75, 115, 255), None, "hammer"),
        ("medic", (240, 200, 170, 255), (180, 120, 60, 255), (55, 135, 80, 255), (120, 105, 90, 255), "hood_green", None),
        ("smith", (215, 170, 140, 255), (55, 55, 60, 255), (115, 65, 40, 255), (45, 45, 50, 255), None, "axe")
    ]
    for cname, skin, hair, shirt, pants, hat, tool in classes:
        for f in ["idle", "walk_0", "walk_1", "action"]:
            c_img = make_character_frame(skin, hair, shirt, pants, hat, tool, f)
            save_sprite(c_img, "Characters", f"char_{cname}_{f}")

def make_wolf(action="idle"):
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([16, 46, 50, 56], fill=(20, 20, 25, 110))
    wolf_gray = (105, 110, 118, 255)
    head_x = 42 if action != "attack" else 46
    head_y = 24 if action != "attack" else 22
    for lx in [22, 28, 38, 44]:
        draw.line([(lx, 38), (lx, 52)], fill=wolf_gray, width=3)
    draw.ellipse([18, 26, 44, 40], fill=wolf_gray, outline=(55, 58, 65, 255))
    draw.ellipse([30, 28, 42, 38], fill=(160, 165, 175, 255))
    draw.line([(18, 30), (8, 38)], fill=wolf_gray, width=4)
    draw.polygon([(head_x - 8, head_y), (head_x + 8, head_y + 4), (head_x, head_y - 8)], fill=wolf_gray)
    draw.point((head_x + 2, head_y - 2), fill=(245, 45, 45, 255))
    return img

def make_bear(action="idle"):
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([12, 48, 54, 58], fill=(20, 20, 25, 120))
    bear_brown = (95, 60, 38, 255)
    draw.rectangle([18, 38, 26, 54], fill=bear_brown)
    draw.rectangle([38, 38, 46, 54], fill=bear_brown)
    draw.ellipse([14, 20, 48, 44], fill=bear_brown, outline=(50, 30, 18, 255))
    hx = 46 if action != "attack" else 50
    draw.ellipse([hx - 8, 20, hx + 8, 34], fill=bear_brown)
    draw.ellipse([hx + 4, 25, hx + 10, 32], fill=(135, 95, 65, 255))
    draw.point((hx, 23), fill=(20, 20, 20, 255))
    return img

def make_spider(action="idle"):
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([14, 44, 50, 56], fill=(20, 20, 25, 90))
    spider_dark = (90, 12, 15, 255)
    for pair in [([(26, 34), (12, 20), (6, 42)], [(38, 34), (52, 20), (58, 42)]), ([(26, 38), (16, 34), (12, 54)], [(38, 38), (48, 34), (52, 54)])]:
        draw.line(pair[0], fill=spider_dark, width=2)
        draw.line(pair[1], fill=spider_dark, width=2)
    draw.ellipse([20, 26, 36, 44], fill=(165, 25, 30, 255), outline=spider_dark)
    draw.ellipse([28, 28, 44, 40], fill=(120, 18, 22, 255), outline=spider_dark)
    for ep in [(40, 32), (42, 31), (42, 34)]:
        draw.point(ep, fill=(255, 230, 50, 255))
    return img

def make_deer():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([14, 48, 50, 56], fill=(20, 20, 25, 100))
    deer_tan = (165, 115, 65, 255)
    for lx in [20, 26, 38, 44]:
        draw.line([(lx, 38), (lx, 52)], fill=(105, 65, 30, 255), width=2)
    draw.ellipse([18, 26, 44, 40], fill=deer_tan)
    draw.ellipse([40, 12, 48, 18], fill=deer_tan)
    draw.line([(42, 12), (38, 4)], fill=(110, 85, 60, 255), width=2)
    draw.line([(44, 12), (48, 4)], fill=(110, 85, 60, 255), width=2)
    return img

def make_rabbit():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([20, 48, 44, 56], fill=(20, 20, 25, 90))
    fur = (175, 165, 155, 255)
    draw.ellipse([24, 34, 40, 48], fill=fur)
    draw.ellipse([34, 30, 44, 40], fill=fur)
    draw.ellipse([34, 16, 38, 32], fill=fur)
    draw.ellipse([38, 18, 42, 32], fill=fur)
    return img

def generate_all_animals():
    print("Generating Wildlife Sprites...")
    for action in ["idle", "walk", "attack"]:
        save_sprite(make_wolf(action), "Animals", f"animal_wolf_{action}")
        save_sprite(make_bear(action), "Animals", f"animal_bear_{action}")
        save_sprite(make_spider(action), "Animals", f"animal_spider_{action}")
    save_sprite(make_deer(), "Animals", "animal_deer_idle")
    save_sprite(make_rabbit(), "Animals", "animal_rabbit_idle")

def make_item_base():
    return Image.new("RGBA", (48, 48), (0, 0, 0, 0))

def generate_all_items():
    print("Generating Items...")
    i_pick = make_item_base()
    d = ImageDraw.Draw(i_pick)
    d.line([(10, 38), (36, 12)], fill=(130, 80, 40, 255), width=3)
    d.arc([16, 4, 44, 32], 135, 315, fill=(185, 190, 200, 255), width=4)
    save_sprite(i_pick, "Items", "item_pickaxe_miner", ppu=48)

    i_axe = make_item_base()
    d = ImageDraw.Draw(i_axe)
    d.line([(12, 38), (34, 14)], fill=(130, 80, 40, 255), width=3)
    d.polygon([(26, 10), (40, 8), (38, 22), (28, 16)], fill=(185, 190, 200, 255), outline=(70, 75, 85, 255))
    save_sprite(i_axe, "Items", "item_axe_woodcutter", ppu=48)

    i_shov = make_item_base()
    d = ImageDraw.Draw(i_shov)
    d.line([(12, 38), (32, 16)], fill=(130, 80, 40, 255), width=3)
    d.polygon([(30, 14), (40, 6), (44, 16), (36, 22)], fill=(180, 185, 195, 255), outline=(70, 75, 85, 255))
    save_sprite(i_shov, "Items", "item_shovel", ppu=48)

    i_rod = make_item_base()
    d = ImageDraw.Draw(i_rod)
    d.line([(6, 42), (38, 10)], fill=(195, 155, 65, 255), width=2)
    d.line([(38, 10), (40, 30)], fill=(220, 220, 230, 255), width=1)
    save_sprite(i_rod, "Items", "item_fishing_rod", ppu=48)

    i_sword = make_item_base()
    d = ImageDraw.Draw(i_sword)
    d.line([(10, 38), (38, 10)], fill=(215, 220, 230, 255), width=4)
    d.line([(14, 30), (22, 38)], fill=(215, 175, 55, 255), width=3)
    save_sprite(i_sword, "Items", "item_sword_iron", ppu=48)

    i_mace = make_item_base()
    d = ImageDraw.Draw(i_mace)
    d.line([(10, 38), (32, 16)], fill=(110, 65, 30, 255), width=4)
    d.ellipse([28, 8, 42, 22], fill=(95, 100, 110, 255))
    save_sprite(i_mace, "Items", "item_mace_heavy", ppu=48)

    i_bow = make_item_base()
    d = ImageDraw.Draw(i_bow)
    d.arc([10, 6, 38, 42], 45, 225, fill=(145, 90, 45, 255), width=3)
    d.line([(14, 10), (14, 38)], fill=(225, 225, 230, 255), width=1)
    save_sprite(i_bow, "Items", "item_bow_wood", ppu=48)

    i_plate = make_item_base()
    d = ImageDraw.Draw(i_plate)
    d.polygon([(14, 10), (34, 10), (40, 36), (24, 42), (8, 36)], fill=(185, 190, 205, 255), outline=(65, 70, 80, 255))
    save_sprite(i_plate, "Items", "item_armor_plate", ppu=48)

    i_helm = make_item_base()
    d = ImageDraw.Draw(i_helm)
    d.ellipse([12, 10, 36, 34], fill=(175, 180, 192, 255), outline=(60, 65, 75, 255))
    save_sprite(i_helm, "Items", "item_helmet_iron", ppu=48)

    i_pack = make_item_base()
    d = ImageDraw.Draw(i_pack)
    d.rectangle([12, 12, 36, 38], fill=(145, 95, 50, 255), outline=(75, 45, 20, 255))
    save_sprite(i_pack, "Items", "item_backpack", ppu=48)

    def make_ingot(col, highlight):
        img = make_item_base()
        d = ImageDraw.Draw(img)
        d.polygon([(10, 28), (18, 16), (36, 16), (42, 28), (34, 36), (14, 36)], fill=col, outline=blend_color(col, (0,0,0,255), 0.5))
        d.line([(18, 16), (36, 16)], fill=highlight, width=2)
        return img

    save_sprite(make_ingot((190, 195, 205, 255), (250, 252, 255, 255)), "Items", "item_ingot_iron", ppu=48)
    save_sprite(make_ingot((205, 115, 60, 255), (245, 165, 105, 255)), "Items", "item_ingot_copper", ppu=48)
    save_sprite(make_ingot((245, 205, 45, 255), (255, 245, 145, 255)), "Items", "item_ingot_gold", ppu=48)
    save_sprite(make_ingot((220, 235, 245, 255), (255, 255, 255, 255)), "Items", "item_ingot_silver", ppu=48)

    i_log = make_item_base()
    d = ImageDraw.Draw(i_log)
    d.rectangle([8, 16, 40, 32], fill=(125, 80, 42, 255), outline=(65, 40, 20, 255))
    save_sprite(i_log, "Items", "item_wood_log", ppu=48)

    i_plank = make_item_base()
    d = ImageDraw.Draw(i_plank)
    d.rectangle([6, 18, 42, 30], fill=(185, 135, 85, 255), outline=(105, 70, 35, 255))
    save_sprite(i_plank, "Items", "item_wood_plank", ppu=48)

    i_coal = make_item_base()
    d = ImageDraw.Draw(i_coal)
    d.polygon([(12, 32), (18, 14), (32, 12), (40, 26), (34, 38), (18, 38)], fill=(32, 32, 36, 255), outline=(10, 10, 12, 255))
    save_sprite(i_coal, "Items", "item_coal", ppu=48)

    i_stone = make_item_base()
    d = ImageDraw.Draw(i_stone)
    d.polygon([(10, 30), (16, 16), (34, 14), (42, 28), (32, 38), (14, 36)], fill=(145, 148, 155, 255), outline=(75, 78, 85, 255))
    save_sprite(i_stone, "Items", "item_stone_rough", ppu=48)

    i_sblock = make_item_base()
    d = ImageDraw.Draw(i_sblock)
    d.rectangle([10, 14, 38, 34], fill=(160, 165, 172, 255), outline=(75, 78, 85, 255))
    save_sprite(i_sblock, "Items", "item_stone_block", ppu=48)

    i_bread = make_item_base()
    d = ImageDraw.Draw(i_bread)
    d.ellipse([10, 16, 38, 32], fill=(215, 155, 65, 255), outline=(125, 80, 30, 255))
    save_sprite(i_bread, "Items", "item_bread", ppu=48)

    i_stew = make_item_base()
    d = ImageDraw.Draw(i_stew)
    d.ellipse([10, 18, 38, 36], fill=(130, 85, 50, 255), outline=(65, 40, 20, 255))
    save_sprite(i_stew, "Items", "item_meat_stew", ppu=48)

    i_ale = make_item_base()
    d = ImageDraw.Draw(i_ale)
    d.rectangle([14, 16, 32, 38], fill=(145, 95, 50, 255), outline=(65, 40, 20, 255))
    save_sprite(i_ale, "Items", "item_ale_mug", ppu=48)

    i_band = make_item_base()
    d = ImageDraw.Draw(i_band)
    d.ellipse([12, 14, 36, 34], fill=(240, 240, 235, 255), outline=(170, 170, 165, 255))
    d.line([(18, 24), (30, 24)], fill=(215, 45, 45, 255), width=3)
    d.line([(24, 18), (24, 30)], fill=(215, 45, 45, 255), width=3)
    save_sprite(i_band, "Items", "item_bandage", ppu=48)

def generate_all_ui():
    print("Generating UI Sprites...")
    def make_cursor_icon(draw_func):
        img = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.ellipse([4, 4, 44, 44], fill=(35, 38, 45, 230), outline=(205, 175, 75, 255), width=2)
        draw_func(d)
        return img

    save_sprite(make_cursor_icon(lambda d: [
        d.line([(14, 34), (32, 16)], fill=(130, 80, 40, 255), width=3),
        d.arc([18, 8, 38, 28], 135, 315, fill=(220, 225, 235, 255), width=3)
    ]), "UI", "ui_cursor_dig", ppu=48)

    save_sprite(make_cursor_icon(lambda d: [
        d.line([(14, 34), (30, 18)], fill=(130, 80, 40, 255), width=3),
        d.polygon([(24, 14), (36, 12), (34, 24)], fill=(220, 225, 235, 255))
    ]), "UI", "ui_cursor_chop", ppu=48)

    save_sprite(make_cursor_icon(lambda d: [
        d.arc([12, 12, 36, 36], 180, 360, fill=(75, 185, 65, 255), width=3),
        d.ellipse([22, 22, 26, 26], fill=(235, 45, 45, 255))
    ]), "UI", "ui_cursor_gather", ppu=48)

    save_sprite(make_cursor_icon(lambda d: [
        d.ellipse([14, 14, 34, 34], outline=(225, 45, 45, 255), width=3),
        d.line([(18, 18), (30, 30)], fill=(225, 45, 45, 255), width=3)
    ]), "UI", "ui_cursor_cancel", ppu=48)

    btn_up = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(btn_up)
    d.rectangle([4, 4, 60, 60], fill=(45, 50, 60, 255), outline=(215, 180, 65, 255), width=2)
    d.polygon([(32, 14), (16, 36), (26, 36), (26, 50), (38, 50), (38, 36), (48, 36)], fill=(245, 210, 75, 255))
    save_sprite(btn_up, "UI", "ui_btn_z_up", ppu=64)

    btn_down = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(btn_down)
    d.rectangle([4, 4, 60, 60], fill=(45, 50, 60, 255), outline=(215, 180, 65, 255), width=2)
    d.polygon([(32, 50), (16, 28), (26, 28), (26, 14), (38, 14), (38, 28), (48, 28)], fill=(85, 170, 245, 255))
    save_sprite(btn_down, "UI", "ui_btn_z_down", ppu=64)

    body = Image.new("RGBA", (128, 160), (0, 0, 0, 0))
    d = ImageDraw.Draw(body)
    d.ellipse([52, 12, 76, 36], fill=(100, 185, 100, 255), outline=(30, 70, 30, 255), width=2)
    d.rectangle([46, 40, 82, 95], fill=(100, 185, 100, 255), outline=(30, 70, 30, 255), width=2)
    d.rectangle([24, 42, 42, 92], fill=(100, 185, 100, 255), outline=(30, 70, 30, 255), width=2)
    d.rectangle([86, 42, 104, 92], fill=(100, 185, 100, 255), outline=(30, 70, 30, 255), width=2)
    d.rectangle([46, 100, 62, 152], fill=(100, 185, 100, 255), outline=(30, 70, 30, 255), width=2)
    d.rectangle([66, 100, 82, 152], fill=(100, 185, 100, 255), outline=(30, 70, 30, 255), width=2)
    save_sprite(body, "UI", "ui_body_silhouette", ppu=64)

    ring = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(ring)
    d.ellipse([4, 18, 60, 46], outline=(65, 235, 95, 255), width=3)
    save_sprite(ring, "UI", "ui_selection_ring", ppu=64)

if __name__ == "__main__":
    generate_all_tiles()
    generate_all_flora()
    generate_all_workbenches()
    generate_all_characters()
    generate_all_animals()
    generate_all_items()
    generate_all_ui()
    print("All sprites generated successfully!")
