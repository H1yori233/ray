import time
from ray.rllib.examples.envs.classes.multi_agent.footsies.footsies_env import (
    FootsiesEnv,
)


def test_windowed_env():
    env_config = {
        "binary_to_download": "linux_windowed",
        "binary_download_dir": "/tmp/ray/binaries/footsies",
        "binary_extract_dir": "/tmp/ray/binaries/footsies",
        "host": "localhost",
        "max_t": 1000,
        "frame_skip": 4,
        "observation_delay": 16,
    }

    port = 46000
    env = FootsiesEnv(config=env_config, port=port)

    obs, info = env.reset()
    for i in range(1000):
        # Take random actions
        actions = {
            "p1": env.action_space["p1"].sample(),
            "p2": env.action_space["p2"].sample(),
        }
        obs, rewards, terminateds, truncateds, infos = env.step(actions)

        if terminateds["__all__"] or truncateds["__all__"]:
            print(f"Episode ended at step {i+1}")
            obs, info = env.reset()

        time.sleep(0.01)  # Small delay to observe the window

    env.close()


if __name__ == "__main__":
    test_windowed_env()
