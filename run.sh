pkill -9 ray; pkill -9 footsies; pkill -9 python
rm -rf recordings; mkdir recordings
python record_footsies.py \
    --binary-download-dir ./footsies_binary \
    --binary-extract-dir ./footsies_binary | tee log.txt
# python recordings_to_video.py
python filter_dataset.py
# python visualize_filter.py

# Move recordings to datasets/N
mkdir -p datasets
N=$(ls -d datasets/[0-9]* 2>/dev/null | wc -l)
mv recordings "datasets/$N"
echo "Saved to datasets/$N"
