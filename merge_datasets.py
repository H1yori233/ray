import os
import shutil
from pathlib import Path

DATASETS_DIR = Path("datasets")
EPISODES_PER_ROUND = 3  # episode 0, 1, 2 per round

def parse_filename(filename):
    """Parse: {episode}_{step}_{p1}_{p2}_{v1}_{v2}.png"""
    try:
        parts = os.path.splitext(filename)[0].split('_')
        if len(parts) >= 6:
            return {
                'episode': int(parts[0]),
                'step': parts[1],
                'p1': parts[2],
                'p2': parts[3],
                'v1': parts[4],
                'v2': parts[5]
            }
    except ValueError:
        pass
    return None

def make_new_filename(meta, new_episode):
    return f"{new_episode}_{meta['step']}_{meta['p1']}_{meta['p2']}_{meta['v1']}_{meta['v2']}.png"

def main():
    # Get all round directories (0, 1, 2, ..., 99)
    round_dirs = sorted([d for d in DATASETS_DIR.iterdir() if d.is_dir() and d.name.isdigit()], 
                        key=lambda x: int(x.name))
    
    print(f"Found {len(round_dirs)} rounds")
    
    total_moved = 0
    total_deleted_csv = 0
    total_deleted_png = 0
    
    for round_dir in round_dirs:
        round_num = int(round_dir.name)
        clean_dir = round_dir / "clean"
        
        # 1. Delete all CSV files in round dir
        for csv_file in round_dir.glob("*.csv"):
            print(f"Delete CSV: {csv_file}")
            csv_file.unlink()
            total_deleted_csv += 1
        
        # 2. Delete all PNG files in round dir (not in clean/)
        for png_file in round_dir.glob("*.png"):
            print(f"Delete PNG: {png_file}")
            png_file.unlink()
            total_deleted_png += 1
        
        # 3. Move clean/*.png to datasets/ with new episode number
        if clean_dir.exists():
            for png_file in clean_dir.glob("*.png"):
                meta = parse_filename(png_file.name)
                if meta is None:
                    print(f"Skip (parse failed): {png_file}")
                    continue
                
                # New episode = original_episode + round_num * 3
                new_episode = meta['episode'] + round_num * EPISODES_PER_ROUND
                new_filename = make_new_filename(meta, new_episode)
                new_path = DATASETS_DIR / new_filename
                
                shutil.move(str(png_file), str(new_path))
                total_moved += 1
            
            # 4. Remove empty clean directory
            if not any(clean_dir.iterdir()):
                clean_dir.rmdir()
        
        # 5. Remove empty round directory
        if not any(round_dir.iterdir()):
            round_dir.rmdir()
            print(f"Removed empty dir: {round_dir}")
    
    print("-" * 40)
    print(f"Deleted CSV: {total_deleted_csv}")
    print(f"Deleted PNG (unfiltered): {total_deleted_png}")
    print(f"Moved to datasets/: {total_moved}")

if __name__ == "__main__":
    main()
