N=${1:-1}

for i in $(seq 1 $N); do
    echo "========== Run $i / $N =========="
    bash run.sh
    echo ""
done

echo "Done! Completed $N runs."
