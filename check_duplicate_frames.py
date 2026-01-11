import os
from pathlib import Path
from collections import defaultdict

def parse_filename(filename):
    """Parse filename: episode{N}_{frame:06d}_{p1_action}_{p2_action}_{p1_valid}_{p2_valid}.png"""
    if not filename.endswith('.png'):
        return None
    
    # Remove extension
    name = filename[:-4]
    
    # Split by underscore
    parts = name.split('_')
    
    if len(parts) != 6:
        return None
    
    try:
        episode = parts[0]  # e.g., "episode1"
        frame_count = int(parts[1])
        p1_action = int(parts[2])
        p2_action = int(parts[3])
        p1_valid = int(parts[4])
        p2_valid = int(parts[5])
        
        return {
            'episode': episode,
            'frame_count': frame_count,
            'p1_action': p1_action,
            'p2_action': p2_action,
            'p1_valid': p1_valid,
            'p2_valid': p2_valid,
            'filename': filename
        }
    except (ValueError, IndexError):
        return None

def check_frames(directory):
    """Check for duplicate and missing frame numbers."""
    directory = Path(directory)
    
    if not directory.exists():
        print(f"Directory does not exist: {directory}")
        return
    
    # Group files by (episode, frame_count)
    frame_groups = defaultdict(list)
    # Also group by episode for continuity check
    episode_frames = defaultdict(set)
    
    for filepath in directory.glob('*.png'):
        parsed = parse_filename(filepath.name)
        if parsed:
            key = (parsed['episode'], parsed['frame_count'])
            frame_groups[key].append(parsed)
            episode_frames[parsed['episode']].add(parsed['frame_count'])
    
    total_frames = sum(len(files) for files in frame_groups.values())
    print(f"Total frames: {total_frames}")
    print(f"Total episodes: {len(episode_frames)}")
    print()
    
    # ============ Check duplicates ============
    duplicates = []
    for key, files in frame_groups.items():
        if len(files) > 1:
            duplicates.append((key, files))
    
    if not duplicates:
        print(f"✓ No duplicate frames found")
    else:
        print(f"⚠ Found {len(duplicates)} duplicate frame numbers")
        for (episode, frame_count), files in sorted(duplicates)[:10]:
            print(f"  {episode} frame {frame_count:06d}: {len(files)} files")
            for f in files:
                print(f"    - p1={f['p1_action']}, p2={f['p2_action']} | {f['filename']}")
        if len(duplicates) > 10:
            print(f"  ... and {len(duplicates) - 10} more")
    print()
    
    # ============ Check missing frames (gaps) ============
    print("=" * 60)
    print("Checking for missing frames (gaps) and continuous segments per episode...")
    print("=" * 60)
    
    total_missing = 0
    episodes_with_gaps = 0
    
    for episode in sorted(episode_frames.keys()):
        frames = sorted(episode_frames[episode])
        if not frames:
            continue
        
        min_frame = frames[0]
        max_frame = frames[-1]
        expected_count = max_frame - min_frame + 1
        actual_count = len(frames)
        missing_count = expected_count - actual_count
        
        # Find continuous segments and gaps
        segments = []  # List of (start, end) tuples for continuous segments
        gaps = []      # List of (start, end) tuples for gaps
        
        segment_start = frames[0]
        for i in range(len(frames) - 1):
            if frames[i+1] - frames[i] > 1:
                # End current segment
                segments.append((segment_start, frames[i]))
                # Record gap
                gap_start = frames[i] + 1
                gap_end = frames[i+1] - 1
                gaps.append((gap_start, gap_end))
                # Start new segment
                segment_start = frames[i+1]
        # Add last segment
        segments.append((segment_start, frames[-1]))
        
        if missing_count > 0:
            episodes_with_gaps += 1
            total_missing += missing_count
        
        print(f"\n{episode}: {actual_count} frames, {missing_count} missing (range: {min_frame}-{max_frame})")
        
        # Show continuous segments
        print(f"  Continuous segments ({len(segments)}):")
        for seg_start, seg_end in segments[:5]:
            seg_len = seg_end - seg_start + 1
            print(f"    [{seg_start:06d}-{seg_end:06d}] ({seg_len} frames)")
        if len(segments) > 5:
            print(f"    ... and {len(segments) - 5} more segments")
        
        # Show gaps
        if gaps:
            print(f"  Gaps ({len(gaps)}):")
            for gap_start, gap_end in gaps[:5]:
                if gap_start == gap_end:
                    print(f"    [{gap_start:06d}] (1 missing)")
                else:
                    print(f"    [{gap_start:06d}-{gap_end:06d}] ({gap_end - gap_start + 1} missing)")
            if len(gaps) > 5:
                print(f"    ... and {len(gaps) - 5} more gaps")
    
    print()
    print("=" * 60)
    print(f"Summary: {episodes_with_gaps} episodes with gaps, {total_missing} total missing frames")
    print("=" * 60)
    return episodes_with_gaps, total_missing, episode_frames

def plot_distributions(all_dir_data, output_file="frame_distribution.png"):
    """Plot the distribution of existing frames for each directory."""
    import matplotlib.pyplot as plt
    import numpy as np

    n_dirs = len(all_dir_data)
    if n_dirs == 0:
        return

    # Dynamic height
    fig, axes = plt.subplots(n_dirs, 1, figsize=(15, 3 * n_dirs), sharex=True)
    if n_dirs == 1:
        axes = [axes]
    
    max_frame_global = 1000

    for i, (dir_name, episode_frames) in enumerate(all_dir_data.items()):
        ax = axes[i]
        total_episodes = len(episode_frames)
        
        # Determine max frame for this dir
        max_f = 0
        for frames in episode_frames.values():
            if frames:
                max_f = max(max_f, max(frames))
        max_frame_global = max(max_frame_global, max_f)

        # Count existence of each frame index
        frame_counts = np.zeros(max_f + 1, dtype=int)
        
        for frames in episode_frames.values():
            for f in frames:
                frame_counts[f] += 1
        
        # Plot area
        ax.plot(frame_counts, color='tab:blue', linewidth=1)
        ax.fill_between(range(len(frame_counts)), frame_counts, alpha=0.3, color='tab:blue')
        
        ax.set_title(f"{dir_name} (Episodes: {total_episodes})", fontsize=10, pad=5)
        ax.set_ylabel("Count", fontsize=9)
        ax.set_ylim(0, total_episodes + 2)
        ax.grid(True, linestyle='--', alpha=0.5)
        
    axes[-1].set_xlabel("Frame Index", fontsize=10)
    plt.tight_layout()
    plt.savefig(output_file, dpi=150)
    print(f"\nSaved distribution plot to: {Path(output_file).absolute()}")

if __name__ == '__main__':
    root_datasets_dir = Path(__file__).parent / 'datasets'
    clean_dirs = sorted(list(root_datasets_dir.glob('*/clean')))
    
    if not clean_dirs:
        print(f"No 'clean' directories found in {root_datasets_dir}")
        pass
    
    grand_total_gap_episodes = 0
    grand_total_missing_frames = 0
    dirs_with_issues = 0

    all_dir_data = {}  # { dir_name: episode_frames_dict }

    for clean_dir in clean_dirs:
        print("\n" + "#" * 80)
        print(f"Checking directory: {clean_dir}")
        print("#" * 80)
        
        try:
            dir_label = clean_dir.relative_to(root_datasets_dir.parent)
        except ValueError:
            dir_label = clean_dir.name

        gap_episodes, missing_frames, episode_frames = check_frames(clean_dir)
        
        all_dir_data[str(dir_label)] = episode_frames
        grand_total_gap_episodes += gap_episodes
        grand_total_missing_frames += missing_frames
        if gap_episodes > 0 or missing_frames > 0:
            dirs_with_issues += 1
            
    print("\n" + "=" * 80)
    print("GRAND TOTAL SUMMARY REPORT")
    print("=" * 80)
    print(f"Total directories checked: {len(clean_dirs)}")
    print(f"Directories with issues:   {dirs_with_issues}")
    print(f"Total episodes with gaps:  {grand_total_gap_episodes}")
    print(f"Total missing frames:      {grand_total_missing_frames}")
    if grand_total_missing_frames == 0 and dirs_with_issues == 0:
        print("\nPERFECT RUN: No missing frames found in any directory!")
    else:
        print("\nISSUES FOUND: Please check the detailed logs above.")
    print("=" * 80)

    plot_distributions(all_dir_data)
