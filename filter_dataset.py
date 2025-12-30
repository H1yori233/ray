import cv2
import numpy as np
import os
import shutil
from pathlib import Path

SRC_DIR = "recordings"
DST_DIR = "recordings/clean"
THRESHOLD = 500

def parse_filename(filename):
    try:
        parts = os.path.splitext(filename)[0].split('_')
        return int(parts[4]) if len(parts) >= 6 else None
    except ValueError:
        return None

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
    p1_valid = parse_filename(filename)
    
    if p1_valid is None or p1_valid == 0 or has_text_overlay(filepath):
        count_skipped += 1
        continue
    
    shutil.move(filepath, dst_path / filename)
    count_moved += 1

print(f"Moved: {count_moved}, Skipped: {count_skipped}")
