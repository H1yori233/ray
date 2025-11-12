using System;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

namespace Footsies
{
	// Token: 0x0200003B RID: 59
	public class FootsiesGameServiceImpl : FootsiesGameService.FootsiesGameServiceBase
	{
		// Token: 0x060001DD RID: 477 RVA: 0x00009AE4 File Offset: 0x00007CE4
		public override Task<Empty> StartGame(Empty request, ServerCallContext context)
		{
			Task<Empty> result;
			try
			{
				this.EnqueueToMainThread(delegate
				{
					if (Singleton<GameManager>.Instance == null)
					{
						Debug.LogError("GameManager instance is null");
						return;
					}
					Singleton<GameManager>.Instance.StartGame();
				});
				result = Task.FromResult<Empty>(new Empty());
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("StartGame exception: {0}", arg));
				throw new RpcException(new Status(StatusCode.Unknown, "Exception was thrown by handler."));
			}
			return result;
		}

		// Token: 0x060001DE RID: 478 RVA: 0x00009B58 File Offset: 0x00007D58
		public override Task<Empty> ResetGame(Empty request, ServerCallContext context)
		{
			Task<Empty> result;
			try
			{
				this.EnqueueToMainThread(delegate
				{
					Singleton<GameManager>.Instance.ResetGame();
					this.battleCore = null;
				});
				result = Task.FromResult<Empty>(new Empty());
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("ResetGame exception: {0}", arg));
				throw new RpcException(new Status(StatusCode.Unknown, "Exception was thrown by handler."));
			}
			return result;
		}

		// Token: 0x060001DF RID: 479 RVA: 0x00009BB8 File Offset: 0x00007DB8
		public override Task<BoolValue> IsReady(Empty request, ServerCallContext context)
		{
			Task<BoolValue> task;
			try
			{
				TaskCompletionSource<BoolValue> taskCompletionSource = new TaskCompletionSource<BoolValue>();
				this.EnqueueToMainThread(delegate
				{
					if (this.battleCore == null)
					{
						this.battleCore = UnityEngine.Object.FindObjectOfType<BattleCore>();
					}
					bool value = this.CheckIfReady();
					taskCompletionSource.SetResult(new BoolValue
					{
						Value = value
					});
				});
				task = taskCompletionSource.Task;
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("IsReady exception: {0}", arg));
				throw new RpcException(new Status(StatusCode.Unknown, "Exception was thrown by handler."));
			}
			return task;
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x00003C39 File Offset: 0x00001E39
		private bool CheckIfReady()
		{
			return Singleton<GameManager>.Instance != null && this.battleCore != null;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00009C30 File Offset: 0x00007E30
		public override Task<GameState> StepNFrames(StepInput request, ServerCallContext context)
		{
			Task<GameState> task;
			try
			{
				TaskCompletionSource<GameState> taskCompletionSource = new TaskCompletionSource<GameState>();
				this.EnqueueToMainThread(delegate
				{
					if (this.battleCore == null)
					{
						this.battleCore = UnityEngine.Object.FindObjectOfType<BattleCore>();
						if (this.battleCore == null)
						{
							Debug.LogError("BattleCore not found during StepNFrames.");
							taskCompletionSource.SetResult(new GameState());
							return;
						}
					}
					if (this.battleGUI == null)
					{
						this.battleGUI = UnityEngine.Object.FindObjectOfType<BattleGUI>();
						if (this.battleGUI == null)
						{
							Debug.LogError("BattleGUI not found during StepNFrames.");
							taskCompletionSource.SetResult(new GameState());
							return;
						}
					}
					this.battleCore.SetP1InputData((int)request.P1Action);
					this.battleCore.SetP2InputData((int)request.P2Action);
					for (int i = 0; i < (int)request.NFrames; i++)
					{
						this.battleCore.ManualFixedUpdate();
						this.battleGUI.ManualFixedUpdate();
					}
					this.battleCore.ClearP1InputData();
					this.battleCore.ClearP2InputData();
					GameState gameState = this.battleCore.GetGameState();
					taskCompletionSource.SetResult(gameState);
				});
				task = taskCompletionSource.Task;
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("StepNFrames exception: {0}", arg));
				throw new RpcException(new Status(StatusCode.Unknown, "Exception was thrown by handler."));
			}
			return task;
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x00009CB0 File Offset: 0x00007EB0
		public override Task<GameState> GetState(Empty request, ServerCallContext context)
		{
			Task<GameState> task;
			try
			{
				TaskCompletionSource<GameState> taskCompletionSource = new TaskCompletionSource<GameState>();
				this.EnqueueToMainThread(delegate
				{
					if (this.battleCore == null)
					{
						this.battleCore = UnityEngine.Object.FindObjectOfType<BattleCore>();
						if (this.battleCore == null)
						{
							Debug.LogError("BattleCore not found during GetState.");
							taskCompletionSource.SetResult(new GameState());
							return;
						}
					}
					GameState gameState = this.battleCore.GetGameState();
					taskCompletionSource.SetResult(gameState);
				});
				task = taskCompletionSource.Task;
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("GetState exception: {0}", arg));
				throw new RpcException(new Status(StatusCode.Unknown, "Exception was thrown by handler."));
			}
			return task;
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x00009D28 File Offset: 0x00007F28
		public override Task<EncodedGameState> GetEncodedState(Empty request, ServerCallContext context)
		{
			Task<EncodedGameState> task;
			try
			{
				TaskCompletionSource<EncodedGameState> taskCompletionSource = new TaskCompletionSource<EncodedGameState>();
				this.EnqueueToMainThread(delegate
				{
					if (this.battleCore == null)
					{
						this.battleCore = UnityEngine.Object.FindObjectOfType<BattleCore>();
						if (this.battleCore == null)
						{
							Debug.LogError("BattleCore not found during GetEncodedState.");
							taskCompletionSource.SetResult(new EncodedGameState());
							return;
						}
					}
					EncodedGameState encodedGameState = this.battleCore.GetEncodedGameState();
					taskCompletionSource.SetResult(encodedGameState);
				});
				task = taskCompletionSource.Task;
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("GetEncodedState exception: {0}", arg));
				throw new RpcException(new Status(StatusCode.Unknown, "Exception was thrown by handler."));
			}
			return task;
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x00003C56 File Offset: 0x00001E56
		private void EnqueueToMainThread(Action action)
		{
			UnityMainThreadDispatcher.Instance.Enqueue(action);
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x00009DA0 File Offset: 0x00007FA0
		private void LogGameState(GameState gameState)
		{
			Debug.Log(string.Format("GameState - FrameCount: {0}, RoundState: {1}", gameState.FrameCount, gameState.RoundState));
			this.LogPlayerState("Player 1", gameState.Player1);
			this.LogPlayerState("Player 2", gameState.Player2);
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x00009DF4 File Offset: 0x00007FF4
		private void LogPlayerState(string playerName, PlayerState playerState)
		{
			Debug.Log(string.Concat(new string[]
			{
				string.Format("{0} - Position: ({1}, ", playerName, playerState.PlayerPositionX),
				string.Format("IsDead: {0} ({1}), ", playerState.IsDead, playerState.IsDead.GetType()),
				string.Format("VitalHealth: {0} ({1}), ", playerState.VitalHealth, playerState.VitalHealth.GetType()),
				string.Format("GuardHealth: {0} ({1}), ", playerState.GuardHealth, playerState.GuardHealth.GetType()),
				string.Format("CurrentActionID: {0} ({1}), ", playerState.CurrentActionId, playerState.CurrentActionId.GetType()),
				string.Format("CurrentActionFrame: {0} ({1}), ", playerState.CurrentActionFrame, playerState.CurrentActionFrame.GetType()),
				string.Format("CurrentActionFrameCount: {0} ({1}), ", playerState.CurrentActionFrameCount, playerState.CurrentActionFrameCount.GetType()),
				string.Format("IsActionEnd: {0} ({1}), ", playerState.IsActionEnd, playerState.IsActionEnd.GetType()),
				string.Format("IsAlwaysCancelable: {0} ({1}), ", playerState.IsAlwaysCancelable, playerState.IsAlwaysCancelable.GetType()),
				string.Format("CurrentActionHitCount: {0} ({1}), ", playerState.CurrentActionHitCount, playerState.CurrentActionHitCount.GetType()),
				string.Format("CurrentHitStunFrame: {0} ({1}), ", playerState.CurrentHitStunFrame, playerState.CurrentHitStunFrame.GetType()),
				string.Format("IsInHitStun: {0} ({1}), ", playerState.IsInHitStun, playerState.IsInHitStun.GetType()),
				string.Format("SpriteShakePosition: {0} ({1}), ", playerState.SpriteShakePosition, playerState.SpriteShakePosition.GetType()),
				string.Format("MaxSpriteShakeFrame: {0} ({1}), ", playerState.MaxSpriteShakeFrame, playerState.MaxSpriteShakeFrame.GetType()),
				string.Format("VelocityX: {0} ({1}), ", playerState.VelocityX, playerState.VelocityX.GetType()),
				string.Format("IsFaceRight: {0} ({1}), ", playerState.IsFaceRight, playerState.IsFaceRight.GetType()),
				string.Format("InputBuffer: [{0}] ({1})", string.Join<long>(", ", playerState.InputBuffer), playerState.InputBuffer.GetType())
			}));
		}

		// Token: 0x04000156 RID: 342
		private BattleCore battleCore;

		// Token: 0x04000157 RID: 343
		private BattleGUI battleGUI;
	}
}
