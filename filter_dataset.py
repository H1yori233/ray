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

# Check existing episodes in destination to avoid overwriting
existing_episodes = set()
if dst_path.exists():
    for existing_file in dst_path.glob("*.png"):
        try:
            # Parse format: {episode}_{frame:06d}_...
            parts = existing_file.stem.split('_')
            if parts and parts[0].isdigit():
                existing_episodes.add(int(parts[0]))
        except (ValueError, IndexError):
            continue

max_existing_episode = max(existing_episodes) if existing_episodes else -1
print(f"Existing episodes in destination: {len(existing_episodes)} (max: {max_existing_episode})")

files = [f for f in os.listdir(src_path) if f.endswith('.png')]
print(f"Found {len(files)} images in source")
episode_numbers = set()
valid_files = []

for filename in files:
    meta = parse_filename(filename)
    if meta is not None:
        episode_num = int(meta['episode'].replace('episode', ''))
        episode_numbers.add(episode_num)
        valid_files.append((filename, meta))

if not episode_numbers:
    print("No valid episodes found!")
    exit()

min_episode = min(episode_numbers)
print(f"Source episode range: {min_episode} to {max(episode_numbers)}")

# Calculate offset: new numbering starts from max_existing + 1
offset = max_existing_episode + 1 - min_episode
print(f"Will renumber: episode{min_episode} -> {max_existing_episode + 1} (offset: {offset:+d})")

count_moved = 0
count_skipped = 0
for filename, meta in valid_files:
    filepath = src_path / filename
    
    if has_text_overlay(filepath):
        count_skipped += 1
        continue
    episode_num = int(meta['episode'].replace('episode', '')) + offset
    new_filename = f"{episode_num}_{meta['frame']:06d}_{meta['p1_input']}_{meta['p2_input']}_{meta['p1_valid']}_{meta['p2_valid']}.png"
    
    shutil.move(filepath, dst_path / new_filename)
    count_moved += 1

print(f"Moved: {count_moved}, Skipped: {count_skipped}")
