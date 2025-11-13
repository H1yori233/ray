import glob
import os
import re

import cv2


def parse_tags(path):
    name = os.path.splitext(os.path.basename(path))[0]
    match = re.match(r"(\d+)_([0-9]+)_([0-9]+)$", name)
    if match:
        return match.group(1), match.group(2), match.group(3)
    match = re.match(r"frame_(\d+)_([0-9]+)_([0-9]+)$", name)
    if match:
        return match.group(1), match.group(2), match.group(3)
    parts = name.split("_")
    if len(parts) == 3:
        return parts[0], parts[1], parts[2]
    if len(parts) == 2:
        return parts[0], parts[1], "0"
    return parts[0] if parts else "0", "0", "0"


def format_input(value):
    try:
        action = int(value)
    except ValueError:
        return value

    action_names = {
        0: "None",
        1: "Back",
        2: "Forward",
        3: "Attack",
        4: "Back+Attack",
        5: "Forward+Attack",
        6: "Special"
    }
    return action_names.get(action, f"Unknown({action})")


def annotate(img, frame_id, p1_input, p2_input):
    h, w = img.shape[:2]
    bar_height = max(32, h // 20)
    cv2.rectangle(img, (0, 0), (w, bar_height), (0, 0, 0), -1)
    font = cv2.FONT_HERSHEY_SIMPLEX
    scale = bar_height / 40.0
    thickness = max(1, int(scale * 2))
    baseline = int(bar_height * 0.75)

    def draw_text(text, x, align="left"):
        size = cv2.getTextSize(text, font, scale, thickness)[0]
        if align == "center":
            x -= size[0] // 2
        elif align == "right":
            x -= size[0]
        cv2.putText(img, text, (x, baseline), font, scale, (255, 255, 255), thickness, cv2.LINE_AA)

    p1_action = format_input(p1_input)
    p2_action = format_input(p2_input)

    draw_text(f"P1: {p1_action}", 10, "left")
    draw_text(f"Frame {frame_id}", w // 2, "center")
    draw_text(f"P2: {p2_action}", w - 10, "right")


def main():
    image_paths = sorted(glob.glob(os.path.join("recordings", "*.png")))
    if not image_paths:
        raise SystemExit("no png files found in recordings/")
    first = cv2.imread(image_paths[0])
    if first is None:
        raise SystemExit(f"failed to read {image_paths[0]}")
    height, width = first.shape[:2]
    fps = 15
    writer = cv2.VideoWriter(
        os.path.join("recordings", "recordings.mp4"),
        cv2.VideoWriter_fourcc(*"mp4v"),
        fps,
        (width, height),
    )
    for path in image_paths:
        frame = cv2.imread(path)
        if frame is None:
            continue
        frame_id, p1, p2 = parse_tags(path)
        annotate(frame, frame_id, p1, p2)
        writer.write(frame)
    writer.release()


if __name__ == "__main__":
    main()
