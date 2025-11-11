import argparse
import collections
import time
from pathlib import Path
from typing import Any, Dict, Optional

import numpy as np
from scipy import special

from ray.rllib.algorithms.algorithm import Algorithm
from ray.rllib.examples.envs.classes.multi_agent.footsies.footsies_env import (
    FootsiesEnv,
    env_creator,
)
from ray.rllib.examples.envs.classes.multi_agent.footsies.game.constants import (
    EnvActions,
)
from ray.tune.registry import register_env

try:
    import pygame
except ImportError:
    pygame = None

MODEL_FRAME_SKIP = 4

# Configuration for players
# Options:
# - "human": Human player using keyboard controls
# - "random": Random action selection
# - "noop": No-op (stand still)
# - Path to checkpoint: Load a trained model from checkpoint directory
MODULES = {
    "p1": "human",  # human must be p1 for correct control mapping
    "p2": "random",  # Can be "noop", "random", or path to checkpoint
}

if "human" in MODULES.values():
    assert (
        pygame is not None
    ), "PyGame is required for human control. Install pygame with `pip install pygame`."
    pygame.init()
    screen = pygame.display.set_mode((300, 200))
    pygame.display.set_caption("Footsies Control - Press keys here")
    screen.fill((50, 50, 50))
    pygame.display.flip()

def get_human_action() -> int:
    """Get the current pressed key using PyGame."""
    pygame.event.pump()
    keys = pygame.key.get_pressed()

    if keys[pygame.K_a] and keys[pygame.K_SPACE]:
        return EnvActions.BACK_ATTACK
    elif keys[pygame.K_d] and keys[pygame.K_SPACE]:
        return EnvActions.FORWARD_ATTACK
    elif keys[pygame.K_a]:
        return EnvActions.BACK
    elif keys[pygame.K_d]:
        return EnvActions.FORWARD
    elif keys[pygame.K_SPACE]:
        return EnvActions.ATTACK
    else:
        return EnvActions.NONE


MAX_FPS = 30


def action_from_logits(logits: np.ndarray) -> int:
    action_probs = special.softmax(logits.reshape(-1))
    return np.random.choice(len(action_probs), p=action_probs)


def play_local_episode(
    env: FootsiesEnv,
    modules: Dict[str, Any],
) -> Dict[str, Any]:

    obs, _ = env.reset()
    result = {"p1_reward": 0, "p2_reward": 0}

    terminateds = {"__all__": False}
    truncateds = {"__all__": False}

    # Store last actions for non-human agents
    last_actions = {agent_id: None for agent_id in MODULES.keys()}
    frame = 0
    while not terminateds["__all__"] and not truncateds["__all__"]:
        actions = {}
        for agent_id, obs in obs.items():
            # For human agents, get action every frame
            if MODULES[agent_id] == "human":
                actions[agent_id] = get_human_action()

            else:
                if frame % MODEL_FRAME_SKIP == 0:
                    if MODULES[agent_id] == "random":
                        last_actions[agent_id] = env.action_space[
                            agent_id
                        ].sample()
                    elif MODULES[agent_id] == "noop":
                        last_actions[agent_id] = EnvActions.NONE
                    else:
                        # Use the loaded algorithm to compute action
                        policy = modules[agent_id].get_policy(agent_id)
                        action = policy.compute_single_action(obs)[0]
                        last_actions[agent_id] = action
                actions[agent_id] = last_actions[agent_id]
        frame += 1

        obs, reward, terminateds, truncateds, _ = env.step(actions)
        result["p1_reward"] += reward["p1"]
        result["p2_reward"] += reward["p2"]
        result["p1_win"] = reward["p1"] >= 1
        result["p2_win"] = reward["p2"] >= 1

        if MAX_FPS is not None:
            time.sleep(1 / MAX_FPS)

    if terminateds["__all__"] or truncateds["__all__"]:
        time.sleep(3)

    return result


def load_module(module_spec: str, agent_id: str) -> Optional[Algorithm]:
    """Load a module based on the specification.

    Args:
        module_spec: Either "human", "random", "noop", or a path to a checkpoint
        agent_id: The agent ID this module is for

    Returns:
        Algorithm if a checkpoint is loaded, None otherwise
    """
    if module_spec in ["human", "random", "noop"]:
        return None

    # Treat as checkpoint path
    checkpoint_path = Path(module_spec)
    if not checkpoint_path.exists():
        raise ValueError(f"Checkpoint path does not exist: {checkpoint_path}")

    print(f"Loading checkpoint for {agent_id} from {checkpoint_path}")
    algo = Algorithm.from_checkpoint(str(checkpoint_path))
    return algo


def main():
    # Register the environment before loading checkpoint
    register_env(name="FootsiesEnv", env_creator=env_creator)

    # Parse command line arguments
    parser = argparse.ArgumentParser(
        description="Run local inference with Footsies environment"
    )
    parser.add_argument(
        "--checkpoint",
        type=str,
        default=None,
        help="Path to checkpoint for p2 agent (optional)",
    )
    parser.add_argument(
        "--port",
        type=int,
        default=50051,
        help="Port for the Footsies game server (default: 50051)",
    )
    parser.add_argument(
        "--binary-to-download",
        type=str,
        default="linux_windowed",
        choices=["linux_server", "linux_windowed", "mac_headless", "mac_windowed"],
        help="Which Footsies binary to download (default: linux_server)",
    )
    parser.add_argument(
        "--binary-download-dir",
        type=Path,
        default=Path("/tmp/ray/binaries/footsies"),
        help="Directory to download Footsies binaries (default: /tmp/ray/binaries/footsies)",
    )
    parser.add_argument(
        "--binary-extract-dir",
        type=Path,
        default=Path("/tmp/ray/binaries/footsies"),
        help="Directory to extract Footsies binaries (default: /tmp/ray/binaries/footsies)",
    )

    args = parser.parse_args()
    
    # Update MODULES config if checkpoint is provided
    if args.checkpoint:
        MODULES["p2"] = args.checkpoint
        print(f"Loading p2 agent from checkpoint: {args.checkpoint}")

    modules = {}
    for agent_id, module_spec in MODULES.items():
        modules[agent_id] = load_module(module_spec, agent_id)

    # FootsiesEnv requires config and port as separate arguments
    config = {
        "frame_skip": 4,
        "observation_delay": 16,
        "max_t": 4000,
        "reward_guard_break": True,
        "host": "localhost",  # Required by FootsiesGame
        "binary_to_download": args.binary_to_download,
        "binary_download_dir": str(args.binary_download_dir),
        "binary_extract_dir": str(args.binary_extract_dir),
    }

    env = FootsiesEnv(config=config, port=args.port)

    cumulative_results = collections.defaultdict(lambda: 0)
    num_games = 0
    while num_games < 10:
        num_games += 1
        episode_results = play_local_episode(env, modules)
        for k, v in episode_results.items():
            cumulative_results[k] += v

        print(
            f"{num_games} games played. {MODULES['p1']} winrate: {np.round(cumulative_results['p1_win'] / num_games, 2)}"
        )


if __name__ == "__main__":
    main()
