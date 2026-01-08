pipeline for collecting footsies data

use `rllib\tuned_examples\ppo\multi_agent_footsies_ppo.py` to train agent
to get frames, we need linux_windowed version here.

use `run.sh` to run single 1 time, and `run_n.sh` to run n times
but you can also modify `num_games` in `record_footsies.py`, (I didn't turn they very large)

`footsies_unity_code/` contains hacked unity, to modify their code and build, use [dnSpy](https://github.com/dnSpy/dnSpy) to decompile and modify the code, then save and build within tool.