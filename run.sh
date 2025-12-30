pkill -9 ray; pkill -9 footsies; pkill -9 python
rm -rf recordings; mkdir recordings
python record_footsies.py \
    --binary-download-dir ./footsies_binary \
    --binary-extract-dir ./footsies_binary | tee log.txt
python recordings_to_video.py