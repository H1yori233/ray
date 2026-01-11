import argparse
import collections
import time
import csv
from pathlib import Path
from typing import Any, Dict, Optional
from datetime import datetime

import numpy as np
from scipy import special

from ray.rllib.core.columns import Columns
from ray.rllib.core.rl_module.rl_module import RLModule
from ray.rllib.examples.envs.classes.multi_agent.footsies.footsies_env import (
    FootsiesEnv,
)
from ray.rllib.examples.envs.classes.multi_agent.footsies.game.constants import (
    EnvActions,
)
from ray.rllib.utils.framework import try_import_torch
from ray.rllib.utils.numpy import convert_to_numpy

torch, _ = try_import_torch()

try:
    import pygame
except ImportError:
    pygame = None

MODEL_FRAME_SKIP = 12

MODULES = {
    # "p1": "human",
    # "p2": "random",
    # "p1": {
    #     # "checkpoint": "~/ray_results/PPO_2025-11-10_14-59-51/PPO_FootsiesEnv_f6ee1_00000_0_2025-11-10_14-59-51/checkpoint_000062",
    #     "checkpoint": "/root/ray_results/PPO_2025-11-11_09-47-55/PPO_FootsiesEnv_7f7d0_00000_0_2025-11-11_09-47-55/checkpoint_000081",
    #     "module_id": "lstm",
    # },
    # "p2": {
    #     # "checkpoint": "~/ray_results/PPO_2025-11-10_14-59-51/PPO_FootsiesEnv_f6ee1_00000_0_2025-11-10_14-59-51/checkpoint_000062",
    #     "checkpoint": "/root/ray_results/PPO_2025-11-11_09-47-55/PPO_FootsiesEnv_7f7d0_00000_0_2025-11-11_09-47-55/checkpoint_000081",
    #     "module_id": "lstm_v6",
    # },
    "p1": "sticky_random",
    # "p1": "~/ray_results/PPO_2025-11-11_09-47-55/PPO_FootsiesEnv_7f7d0_00000_0_2025-11-11_09-47-55/checkpoint_000081",
    # "p2": "~/ray_results/PPO_2025-11-11_09-47-55/PPO_FootsiesEnv_7f7d0_00000_0_2025-11-11_09-47-55/checkpoint_000080",
    "p2": "noop",
}

# Check if any player is human
if any(module_spec == "human" for module_spec in MODULES.values()):
    assert (
        pygame is not None
    ), "PyGame is required for human control. Install pygame with `pip install pygame`."
    pygame.init()
    screen = pygame.display.set_mode((300, 200))
    pygame.display.set_caption("Footsies")
    screen.fill((50, 50, 50))
    pygame.display.flip()


def get_human_action() -> int:
    """Get the current pressed key using PyGame."""
    pygame.event.pump()
    keys = pygame.key.get_pressed()

    # Movement only: no attack inputs
    if keys[pygame.K_a]:
        return EnvActions.BACK
    elif keys[pygame.K_d]:
        return EnvActions.FORWARD
    else:
        return EnvActions.NONE


class StickyRandomController:
    def __init__(self, min_duration=1, max_duration=3, attack_prob=0.05, attack_enabled=True):
        self.min_duration = min_duration
        self.max_duration = max_duration
        self.attack_prob = attack_prob
        self.attack_enabled = attack_enabled
        self.current_action = None
        self.frames_remaining = 0
        self.action_weights = [0.5, 0.25, 0.25]
        self.actions = [EnvActions.NONE, EnvActions.BACK, EnvActions.FORWARD]
    
    def get_action(self) -> int:
        if self.frames_remaining <= 0:
            self.current_action = np.random.choice(self.actions, p=self.action_weights)
            self.frames_remaining = np.random.randint(
                self.min_duration, self.max_duration + 1
            )
            if self.current_action == EnvActions.NONE and self.attack_enabled:
                if np.random.random() < self.attack_prob:
                    self.current_action = EnvActions.ATTACK
                    self.frames_remaining = 2
        
        self.frames_remaining -= 1
        return self.current_action
    
    def reset(self, attack_enabled=None):
        self.current_action = None
        self.frames_remaining = 0
        if attack_enabled is not None:
            self.attack_enabled = attack_enabled

sticky_controllers: Dict[str, StickyRandomController] = {}


MAX_FPS = 60


def action_from_logits(logits: np.ndarray) -> int:
    action_probs = special.softmax(logits.reshape(-1))
    return np.random.choice(len(action_probs), p=action_probs)


def play_local_episode(
    env: FootsiesEnv,
    modules: Dict[str, Any],
    module_states: Dict[str, Any],
    max_fps: float,
    max_seq_len: int = 64,
) -> Dict[str, Any]:

    obs, _ = env.reset()
    result = {"p1_reward": 0, "p2_reward": 0}
    action_logs = []

    terminateds = {"__all__": False}
    truncateds = {"__all__": False}

    # Store last actions for non-human agents
    # Initialize with NONE action (0) instead of None
    last_actions = {agent_id: EnvActions.NONE for agent_id in MODULES.keys()}

    # Store observation sequences for LSTM processing
    obs_sequences = {agent_id: [] for agent_id in MODULES.keys()}

    frame = 0
    while not terminateds["__all__"] and not truncateds["__all__"]:
        actions = {}
        for agent_id, obs_single in obs.items():
            module_spec = MODULES[agent_id]

            # Check if it's a human player
            is_human = module_spec == "human"
            is_random = module_spec == "random"
            is_noop = module_spec == "noop"
            is_sticky_random = module_spec == "sticky_random"

            # For human agents, get action every frame
            if is_human:
                actions[agent_id] = get_human_action()

            else:
                # Accumulate observations
                obs_sequences[agent_id].append(obs_single)
                # Keep only the last max_seq_len observations
                if len(obs_sequences[agent_id]) > max_seq_len:
                    obs_sequences[agent_id].pop(0)

                if frame % MODEL_FRAME_SKIP == 0:
                    if is_random:
                        last_actions[agent_id] = np.random.randint(0, 3)
                    elif is_noop:
                        last_actions[agent_id] = EnvActions.NONE
                    elif is_sticky_random:
                        if agent_id not in sticky_controllers:
                            sticky_controllers[agent_id] = StickyRandomController()
                        last_actions[agent_id] = sticky_controllers[agent_id].get_action()
                    else:
                        rl_module = modules[agent_id]

                        # (T, obs_dim) -> (1, T, obs_dim)
                        obs_sequence = np.stack(obs_sequences[agent_id], axis=0)
                        obs_tensor = torch.from_numpy(obs_sequence).unsqueeze(0)
                        input_dict = {
                            Columns.OBS: obs_tensor,
                        }

                        if module_states[agent_id] is not None:
                            input_dict[Columns.STATE_IN] = module_states[agent_id]
                        elif hasattr(rl_module, "get_initial_state"):
                            initial_state = rl_module.get_initial_state()
                            module_states[agent_id] = {
                                k: torch.from_numpy(v).unsqueeze(0)
                                for k, v in initial_state.items()
                            }
                            input_dict[Columns.STATE_IN] = module_states[agent_id]

                        # Forward pass
                        with torch.no_grad():
                            output = rl_module.forward_inference(input_dict)

                        # Update state for next step
                        if Columns.STATE_OUT in output:
                            module_states[agent_id] = output[Columns.STATE_OUT]

                        # Get action - use sampling for stochastic policy (like during training)
                        action_logits = convert_to_numpy(
                            output[Columns.ACTION_DIST_INPUTS]
                        )

                        # different output shapes
                        if len(action_logits.shape) == 3:
                            # (B, T, num_actions), take the last timestep
                            logits_vec = action_logits[0, -1]
                        else:
                            # (B, num_actions)
                            logits_vec = action_logits[0]

                        # Sample
                        action = action_from_logits(logits_vec)
                        # Movement only: restrict to NONE=0, BACK=1, FORWARD=2
                        action = min(action, 2)
                        last_actions[agent_id] = int(action)

                actions[agent_id] = last_actions[agent_id]

        obs, reward, terminateds, truncateds, _ = env.step(actions)

        # Log InputBits AFTER env.step() to match Unity's recording
        # env.step() internally converts SPECIAL_CHARGE to ATTACK/BACK_ATTACK/FORWARD_ATTACK
        # and modifies the actions dict in place
        p1_bits = env.game.action_to_bits(actions["p1"], is_player_1=True)
        p2_bits = env.game.action_to_bits(actions["p2"], is_player_1=False)

        current_frame_log = {
            "frame": frame,
            "p1_action": int(p1_bits),
            "p2_action": int(p2_bits)
        }
        action_logs.append(current_frame_log)
        frame += 1
        result["p1_reward"] += reward["p1"]
        result["p2_reward"] += reward["p2"]
        result["p1_win"] = reward["p1"] >= 1
        result["p2_win"] = reward["p2"] >= 1

        if max_fps is not None and max_fps > 0:
            time.sleep(1 / max_fps)

    if terminateds["__all__"] or truncateds["__all__"]:
        print(f"\nEpisode ended at frame {frame}")
        # print(f"  p1 reward: {result['p1_reward']}, p2 reward: {result['p2_reward']}")
        # print(f"  p1 win: {result['p1_win']}, p2 win: {result['p2_win']}")
        time.sleep(3)

    result["action_logs"] = action_logs
    return result


def load_module(module_spec, agent_id: str) -> Optional[RLModule]:
    """Load a module based on the specification.

    Args:
        module_spec: Can be:
            - A string: "human", "random", "noop", "sticky_random"
            - A dict: {"checkpoint": "path", "module_id": "lstm_v0"}
        agent_id: The agent ID this module is for

    Returns:
        RLModule if a checkpoint is loaded, None otherwise
    """
    if isinstance(module_spec, str) and module_spec in ["human", "random", "noop", "sticky_random"]:
        return None

    # Handle dict format: {"checkpoint": "...", "module_id": "..."}
    if isinstance(module_spec, dict):
        checkpoint_str = module_spec["checkpoint"]
        module_id = module_spec["module_id"]
    else:
        # treat as checkpoint path string
        checkpoint_str = module_spec
        module_id = "lstm"

    # Expand ~ to home directory
    checkpoint_path = Path(checkpoint_str).expanduser()
    if not checkpoint_path.exists():
        raise ValueError(f"Checkpoint path does not exist: {checkpoint_path}")

    # Load RLModule only
    rl_module_path = (
        checkpoint_path / "learner_group" / "learner" / "rl_module" / module_id
    )

    if not rl_module_path.exists():
        raise ValueError(
            f"RLModule '{module_id}' not found in checkpoint at {rl_module_path}"
        )

    print(f"Loading RLModule '{module_id}' for {agent_id} from {rl_module_path}")
    rl_module = RLModule.from_checkpoint(str(rl_module_path))

    # Set to evaluation mode (no exploration)
    rl_module.eval()
    return rl_module


def main():
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
        "--p1-module-id",
        type=str,
        default=None,
        help="RLModule ID for p1 (e.g., lstm, lstm_v0, lstm_v1)",
    )
    parser.add_argument(
        "--p2-module-id",
        type=str,
        default=None,
        help="RLModule ID for p2 (e.g., lstm, lstm_v0, lstm_v1)",
    )
    parser.add_argument(
        "--p1-checkpoint",
        type=str,
        default=None,
        help="Checkpoint path for p1 agent",
    )
    parser.add_argument(
        "--p2-checkpoint",
        type=str,
        default=None,
        help="Checkpoint path for p2 agent",
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

    # Update MODULES config
    if args.p1_checkpoint or args.p1_module_id:
        if isinstance(MODULES["p1"], dict):
            if args.p1_checkpoint:
                MODULES["p1"]["checkpoint"] = args.p1_checkpoint
            if args.p1_module_id:
                MODULES["p1"]["module_id"] = args.p1_module_id
        else:
            # Create dict format
            checkpoint = (
                args.p1_checkpoint if args.p1_checkpoint else MODULES.get("p1", "")
            )
            module_id = args.p1_module_id if args.p1_module_id else "lstm"
            MODULES["p1"] = {"checkpoint": checkpoint, "module_id": module_id}

    if args.p2_checkpoint or args.p2_module_id:
        if isinstance(MODULES["p2"], dict):
            if args.p2_checkpoint:
                MODULES["p2"]["checkpoint"] = args.p2_checkpoint
            if args.p2_module_id:
                MODULES["p2"]["module_id"] = args.p2_module_id
        else:
            # Create dict format
            checkpoint = (
                args.p2_checkpoint if args.p2_checkpoint else MODULES.get("p2", "")
            )
            module_id = args.p2_module_id if args.p2_module_id else "lstm"
            MODULES["p2"] = {"checkpoint": checkpoint, "module_id": module_id}

    # for p2 checkpoitn
    if args.checkpoint:
        if isinstance(MODULES["p2"], dict):
            MODULES["p2"]["checkpoint"] = args.checkpoint
        else:
            MODULES["p2"] = {"checkpoint": args.checkpoint, "module_id": "lstm"}
        print(f"Loading p2 agent from checkpoint: {args.checkpoint}")

    modules = {}
    module_states = {}
    for agent_id, module_spec in MODULES.items():
        rl_module = load_module(module_spec, agent_id)
        modules[agent_id] = rl_module

        # Initialize state
        if rl_module is not None and hasattr(rl_module, "get_initial_state"):
            initial_state = rl_module.get_initial_state()
            # (lstm_cell_size,) -> (1, lstm_cell_size)
            module_states[agent_id] = {
                k: torch.from_numpy(v).unsqueeze(0) for k, v in initial_state.items()
            }
        else:
            module_states[agent_id] = None

    # Use the same config as training for consistency
    config = {
        "frame_skip": 1,
        "observation_delay": 12,
        "max_t": 1000,
        "reward_guard_break": True,
        "host": "localhost",
        "binary_to_download": args.binary_to_download,
        "binary_download_dir": str(args.binary_download_dir),
        "binary_extract_dir": str(args.binary_extract_dir),
    }

    env = FootsiesEnv(config=config, port=args.port)

    # Create recordings directory
    recordings_dir = Path("./recordings")
    recordings_dir.mkdir(exist_ok=True)

    # Create session timestamp for grouping files
    session_timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    max_fps = MAX_FPS

    cumulative_results = collections.defaultdict(lambda: 0)
    num_games = 0
    all_action_logs = []

    try:
        while num_games < 12:
            num_games += 1
            # print()
            print('=' * 60)
            # print()

            # Reset module states for new episode
            for agent_id in module_states:
                if modules[agent_id] is not None and hasattr(
                    modules[agent_id], "get_initial_state"
                ):
                    # Reset to initial state for recurrent models
                    initial_state = modules[agent_id].get_initial_state()
                    module_states[agent_id] = {
                        k: torch.from_numpy(v).unsqueeze(0)
                        for k, v in initial_state.items()
                    }
                else:
                    module_states[agent_id] = None

            attack_enabled = (num_games % 3 != 0)
            for controller in sticky_controllers.values():
                controller.reset(attack_enabled=attack_enabled)

            # Play the episode
            episode_results = play_local_episode(env, modules, module_states, max_fps)
            action_logs = episode_results.pop("action_logs", [])
            all_action_logs.extend(action_logs)

            # Update cumulative results
            for k, v in episode_results.items():
                cumulative_results[k] += v

            # Get player names for display
            def get_player_name(module_spec):
                if isinstance(module_spec, dict):
                    return module_spec.get("module_id", "lstm")
                return module_spec

            p1_name = get_player_name(MODULES["p1"])
            p2_name = get_player_name(MODULES["p2"])
            p1_winrate = np.round(cumulative_results["p1_win"] / num_games, 2)

            print(
                f"\n{num_games} games played. {p1_name} vs {p2_name} | "
                f"{p1_name} winrate: {p1_winrate}"
            )
    finally:
        # Save all action logs to a single CSV file
        csv_filename = f"footsies_{session_timestamp}.csv"
        csv_path = recordings_dir / csv_filename
        with open(csv_path, "w", newline="") as csv_file:
            csv_writer = csv.DictWriter(csv_file, fieldnames=["frame", "p1_action", "p2_action"])
            csv_writer.writeheader()
            for log in all_action_logs:
                csv_writer.writerow({
                    "frame": log["frame"],
                    "p1_action": log["p1_action"],
                    "p2_action": log["p2_action"]
                })
        print(f"CSV: {csv_filename}")
        env.close()


if __name__ == "__main__":
    main()
