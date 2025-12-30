import cv2
import numpy as np
import os
from pathlib import Path


num_samples = 9
threshold = 500
src_path = Path("recordings")
images = [f for f in os.listdir(src_path) if f.endswith('.png')]

if not images:
    print("No images found!")
    exit()

images.sort()
samples = images[:num_samples]

vis_images = []    
for filename in samples:
    img_path = str(src_path / filename)
    img = cv2.imread(img_path)
    if img is None: continue
    
    h, w = img.shape[:2]
    y1, y2 = int(h * 0.08), int(h * 0.24)
    x1, x2 = int(w * 0.3), int(w * 0.7)

    roi = img[y1:y2, x1:x2]
    gray = cv2.cvtColor(roi, cv2.COLOR_BGR2GRAY)
    edges = cv2.Canny(gray, 30, 80)
    edge_count = np.count_nonzero(edges)

    edges_bgr = cv2.cvtColor(edges, cv2.COLOR_GRAY2BGR)
    # Color the edges (e.g., Yellow) to make them stand out
    edges_bgr[np.where((edges_bgr==[255,255,255]).all(axis=2))] = [0, 255, 255]

    cv2.rectangle(img, (x1, y1), (x2, y2), (0, 255, 0), 2)

    has_text = edge_count > threshold
    color = (0, 0, 255) if has_text else (0, 255, 0) # Red if filtered (Text), Green if kept
    status_text = f"SKIP ({edge_count})" if has_text else f"KEEP ({edge_count})"        
    cv2.putText(img, status_text, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.6, color, 2)

    roi_target = img[y1:y2, x1:x2]
    cv2.addWeighted(edges_bgr, 0.8, roi_target, 1.0, 0, roi_target)
    
    # Add border
    img = cv2.copyMakeBorder(img, 2, 2, 2, 2, cv2.BORDER_CONSTANT, value=color)
    vis_images.append(img)

# Create grid
if not vis_images: exit()

lines = []
current_line = []
cols = 3

for img in vis_images:
    current_line.append(img)
    if len(current_line) >= cols:
        lines.append(np.hstack(current_line))
        current_line = []

if current_line:
    # Pad with black
    while len(current_line) < cols:
        current_line.append(np.zeros_like(vis_images[0]))
    lines.append(np.hstack(current_line))
    
final_img = np.vstack(lines)
output_file = "cv_debug.jpg"
cv2.imwrite(output_file, final_img)
print(f"saved to {output_file}")
