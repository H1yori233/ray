import csv
import glob
import os
import re

import cv2


def parse_tags(path):
    name = os.path.splitext(os.path.basename(path))[0]
    match = re.match(r"episode(\d+)_(\d+)_([0-9]+)_([0-9]+)$", name)
    return match.group(1), match.group(2), match.group(3), match.group(4)


def format_input(value):
    try:
        bits = int(value)
    except ValueError:
        return value

    has_left = (bits & 1) != 0
    has_right = (bits & 2) != 0
    has_attack = (bits & 4) != 0

    parts = []
    if has_left and has_right:
        parts.append("L+R")
    elif has_left:
        parts.append("Left")
    elif has_right:
        parts.append("Right")

    if has_attack:
        parts.append("Attack")

    if not parts:
        return "None"

    return "+".join(parts)


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
    image_paths = glob.glob(os.path.join("recordings", "*.png"))
    if not image_paths:
        raise SystemExit("no png files found in recordings/")

    image_paths = sorted(
        image_paths, key=lambda p: (int(parse_tags(p)[0]), int(parse_tags(p)[1]))
    )

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
    csv_path = os.path.join("recordings", "recordings_actions.csv")
    with open(csv_path, "w", newline="") as csv_file:
        fieldnames = ["frame", "p1_action", "p2_action"]
        writer_csv = csv.DictWriter(csv_file, fieldnames=fieldnames)
        writer_csv.writeheader()

        for path in image_paths:
            frame = cv2.imread(path)
            if frame is None:
                continue
            episode, frame_id, p1, p2 = parse_tags(path)
            annotate(frame, frame_id, p1, p2)
            writer.write(frame)
            writer_csv.writerow(
                {
                    "frame": int(frame_id),
                    "p1_action": int(p1),
                    "p2_action": int(p2),
                }
            )
    writer.release()


if __name__ == "__main__":
    main()
