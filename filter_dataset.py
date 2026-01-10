import cv2
import numpy as np
import os
import shutil
from pathlib import Path

SRC_DIR = "recordings"
DST_DIR = "recordings/clean"
THRESHOLD = 500
FRAME_SKIP = 12

def parse_filename(filename):
    # episode{ep}_{frame}_{p1In}_{p2In}_{p1Val}_{p2Val}.png
    try:
        parts = os.path.splitext(filename)[0].split('_')
        if len(parts) >= 6:
            return {
                'episode': parts[0],
                'frame': int(parts[1]),
                'p1_input': parts[2],
                'p2_input': parts[3],
                'p1_valid': int(parts[4]),
                'p2_valid': parts[5]
            }
        return None
    except ValueError:
        return None

def make_new_filename(meta):
    # episode2 -> 0, episode3 -> 1
    episode_num = int(meta['episode'].replace('episode', '')) - 2
    return f"{episode_num}_{meta['frame']:06d}_{meta['p1_input']}_{meta['p2_input']}_{meta['p1_valid']}_{meta['p2_valid']}.png"

def has_text_overlay(image_path):
    img = cv2.imread(str(image_path))
    if img is None:
        return True
    h, w = img.shape[:2]
    roi = img[int(h * 0.08):int(h * 0.24), int(w * 0.3):int(w * 0.7)]
    edges = cv2.Canny(cv2.cvtColor(roi, cv2.COLOR_BGR2GRAY), 30, 80)
    return np.count_nonzero(edges) > THRESHOLD

src_path = Path(SRC_DIR)
dst_path = Path(DST_DIR)

if not src_path.exists():
    print(f"Source directory {src_path} does not exist!")
    exit()

dst_path.mkdir(parents=True, exist_ok=True)

files = [f for f in os.listdir(src_path) if f.endswith('.png')]
print(f"Found {len(files)} images")

count_moved = 0
count_skipped = 0

for filename in files:
    filepath = src_path / filename
    meta = parse_filename(filename)
    
    # if meta is None or meta['p1_valid'] == 0 or has_text_overlay(filepath):
    if meta is None or has_text_overlay(filepath):
        count_skipped += 1
        continue
    
    new_filename = make_new_filename(meta)
    shutil.move(filepath, dst_path / new_filename)
    count_moved += 1

print(f"Moved: {count_moved}, Skipped: {count_skipped}")
