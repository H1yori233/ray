import os
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor, as_completed
from tqdm import tqdm

DATASETS_DIR = Path("datasets")
MERGED_DIR = Path("datasets_merged")
MAX_WORKERS = 14

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

def link_file(args):
    src_path, dst_path = args
    try:
        if dst_path.exists():
            dst_path.unlink()
        os.link(str(src_path), str(dst_path))
        return True
    except OSError as e:
        print(f"Error linking {src_path} -> {dst_path}: {e}")
        return False

def main():
    MERGED_DIR.mkdir(parents=True, exist_ok=True)
    round_dirs = sorted([d for d in DATASETS_DIR.iterdir() if d.is_dir() and d.name.isdigit()], 
                        key=lambda x: int(x.name))
    
    print(f"Found {len(round_dirs)} rounds")
    
    link_tasks = []
    global_episode_counter = 0
    
    for round_dir in round_dirs:
        round_num = int(round_dir.name)
        clean_dir = round_dir / "clean"
        
        # Process clean/*.png files
        if clean_dir.exists():
            episode_numbers = set()
            files_by_episode = {}
            
            for png_file in clean_dir.glob("*.png"):
                meta = parse_filename(png_file.name)
                if meta is None:
                    print(f"Skip (parse failed): {png_file}")
                    continue
                
                ep = meta['episode']
                episode_numbers.add(ep)
                if ep not in files_by_episode:
                    files_by_episode[ep] = []
                files_by_episode[ep].append((png_file, meta))
            
            if episode_numbers:
                min_ep = min(episode_numbers)
                max_ep = max(episode_numbers)
                num_episodes = len(episode_numbers)
                
                print(f"Round {round_num}: {num_episodes} episodes (original {min_ep}-{max_ep}) -> renumbering to {global_episode_counter}-{global_episode_counter + num_episodes - 1}")
                
                # Collect tasks for parallel processing
                for original_ep in sorted(episode_numbers):
                    new_ep = global_episode_counter
                    
                    for png_file, meta in files_by_episode[original_ep]:
                        new_filename = make_new_filename(meta, new_ep)
                        new_path = MERGED_DIR / new_filename
                        link_tasks.append((png_file, new_path))
                    
                    global_episode_counter += 1
    
    print(f"\nCreating {len(link_tasks)} hard links with {MAX_WORKERS} workers...")
    success_count = 0
    
    with ThreadPoolExecutor(max_workers=MAX_WORKERS) as executor:
        futures = [executor.submit(link_file, task) for task in link_tasks]
        for future in tqdm(as_completed(futures), total=len(link_tasks), desc="Linking"):
            if future.result():
                success_count += 1
    
    print("-" * 40)
    print(f"Linked to {MERGED_DIR}/: {success_count}/{len(link_tasks)}")
    print(f"Total episodes processed: {global_episode_counter}")

if __name__ == "__main__":
    main()
