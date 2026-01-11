import os
import glob
import cv2
import json
import numpy as np
import argparse
from tqdm import tqdm
from collections import defaultdict
from multiprocessing import Pool, cpu_count
import functools

def parse_filename(fname):
    parts = fname.replace(".png", "").split("_")
    return {
        "episode": int(parts[0]),
        "step": int(parts[1]),
        "action": int(parts[2]),
        "fname": fname,
        "path": os.path.join(args.image_dir, fname)
    }

def process_episode(episode_data):
    episode_id, frames_info = episode_data
    
    frames_info.sort(key=lambda x: x['step'])
    if not frames_info:
        return []

    # check continuity
    segment_start_idx = 0 
    for i in range(len(frames_info) - 2, -1, -1):
        if frames_info[i+1]['step'] != frames_info[i]['step'] + 1:
            segment_start_idx = i + 1
            break
    
    # Ensure segment starts at a frame that is a multiple of 12
    while segment_start_idx < len(frames_info) and frames_info[segment_start_idx]['step'] % 12 != 0:
        segment_start_idx += 1
    
    valid_frames = frames_info[segment_start_idx:]
    if len(valid_frames) < 96:
        return []

    results = []
    chunk_size = 96
    video_length = 77
    offset = 11
    
    num_chunks = len(valid_frames) // chunk_size
    
    for part_idx in range(num_chunks):
        chunk_start = part_idx * chunk_size
        chunk_end = chunk_start + chunk_size
        extract_start = chunk_start + offset
        extract_end = extract_start + video_length

        print(f"Processing episode {episode_id}, part {part_idx}: {extract_start} - {extract_end}")
        if extract_end > len(valid_frames):
            break  # Not enough frames
        
        chunk_frames = valid_frames[extract_start:extract_end]
        
        if len(chunk_frames) != 77:
            continue

        output_video_path = os.path.join(args.output_dir, "videos", f"episode_{episode_id:03d}_part_{part_idx:03d}.mp4")
        output_action_path = os.path.join(args.output_dir, "videos", f"episode_{episode_id:03d}_part_{part_idx:03d}_actions.npy")
        
        actions = np.array([f['action'] for f in chunk_frames], dtype=np.int8)
        np.save(output_action_path, actions)
        print(f"Action: {actions[0]}, {actions[1:5]}, {actions[6:10]} ... {actions[-4:]}")

        first_img = cv2.imread(chunk_frames[0]['path'])
        height, width, layers = first_img.shape
        fourcc = cv2.VideoWriter_fourcc(*'mp4v') 
        video = cv2.VideoWriter(output_video_path, fourcc, args.fps, (width, height))
        
        for f_info in chunk_frames:
            img = cv2.imread(f_info['path'])
            video.write(img)        
        video.release()

        results.append({
            "video_path": os.path.relpath(output_video_path, args.output_dir),
            "action_path": os.path.relpath(output_action_path, args.output_dir),
            "num_frames": len(chunk_frames),
            "width": width,
            "height": height,
            "episode_id": episode_id,
            "part_id": part_idx
        })

    return results

def main():
    os.makedirs(os.path.join(args.output_dir, "videos"), exist_ok=True)
    print("Scanning files...")
    files = glob.glob(os.path.join(args.image_dir, "*.png"))
    
    episodes = defaultdict(list)
    print("Parsing filenames...")
    for f in tqdm(files):
        fname = os.path.basename(f)
        info = parse_filename(fname)
        episodes[info['episode']].append(info)
        
    print(f"Found {len(episodes)} episodes.")
    episodes_list = sorted(episodes.items())
    global process_episode_worker
    process_episode_worker = functools.partial(process_episode)
    
    metadata = []
    num_workers = max(1, cpu_count() - 2)
    print(f"Processing with {num_workers} workers...")
    
    with Pool(num_workers) as p:
        results = list(tqdm(p.imap(process_episode, episodes_list), total=len(episodes_list)))
    metadata = []
    for r in results:
        metadata.extend(r)

    metadata_path = os.path.join(args.output_dir, "metadata.json")
    with open(metadata_path, 'w') as f:
        json.dump(metadata, f, indent=2)        
    
    # Generate videos2caption.json format
    videos2caption = []
    for m in metadata:
        video_filename = os.path.basename(m['video_path'])
        action_filename = os.path.basename(m['action_path'])
        duration = m['num_frames'] / args.fps
        videos2caption.append({
            "path": video_filename,
            "cap": [""],
            "action_path": action_filename,
            "resolution": {
                "width": m['width'],
                "height": m['height']
            },
            "num_frames": m['num_frames'],
            "fps": args.fps,
            "duration": duration
        })
    
    videos2caption_path = os.path.join(args.output_dir, "videos2caption.json")
    with open(videos2caption_path, 'w') as f:
        json.dump(videos2caption, f, indent=4)
    
    print(f"Done! Processed {len(metadata)} valid episodes.")
    print(f"Metadata saved to {metadata_path}")
    print(f"Videos2caption saved to {videos2caption_path}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--image_dir", type=str, required=True, help="Directory containing PNG images")
    parser.add_argument("--output_dir", type=str, required=True, help="Output directory for videos and metadata")
    parser.add_argument("--fps", type=int, default=25, help="Frame rate for output videos")
    parser.add_argument("--max_frames", type=int, default=96, help="Maximum frames per video segment")
    
    args = parser.parse_args()
    main()
