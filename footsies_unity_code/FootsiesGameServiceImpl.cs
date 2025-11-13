using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

namespace Footsies
{
	// Token: 0x0200002B RID: 43
	public class FootsiesGameServiceImpl : FootsiesGameService.FootsiesGameServiceBase
	{
		// Token: 0x0600018A RID: 394
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

		// Token: 0x0600018B RID: 395
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

		// Token: 0x0600018C RID: 396
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

		// Token: 0x0600018D RID: 397
		private bool CheckIfReady()
		{
			return Singleton<GameManager>.Instance != null && this.battleCore != null;
		}

		// Token: 0x0600018E RID: 398
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
					int num = (int)request.P1Action;
					int num2 = (int)request.P2Action;
					FootsiesGameServiceImpl.EnvAction envAction = FootsiesGameServiceImpl.ConvertInputToEnvAction(num, true);
					FootsiesGameServiceImpl.EnvAction envAction2 = FootsiesGameServiceImpl.ConvertInputToEnvAction(num2, false);
					this.battleCore.SetP1InputData(num);
					this.battleCore.SetP2InputData(num2);
					for (int i = 0; i < (int)request.NFrames; i++)
					{
						this.battleCore.ManualFixedUpdate();
						this.battleGUI.ManualFixedUpdate();
					}
					this.battleCore.ClearP1InputData();
					this.battleCore.ClearP2InputData();
					GameState gameState = this.battleCore.GetGameState();
					UnityMainThreadDispatcher.Instance.StartCoroutine(this.CaptureScreenshotCoroutine((int)envAction, (int)envAction2, (int)gameState.FrameCount));
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

		// Token: 0x0600018F RID: 399
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

		// Token: 0x06000190 RID: 400
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

		// Token: 0x06000191 RID: 401
		private void EnqueueToMainThread(Action action)
		{
			UnityMainThreadDispatcher.Instance.Enqueue(action);
		}

		// Token: 0x060002E3 RID: 739
		private static FootsiesGameServiceImpl.EnvAction ConvertInputToEnvAction(int input, bool isPlayerOne)
		{
			bool flag = (input & (int)InputDefine.Attack) != 0;
			bool flag2 = (input & (int)InputDefine.Left) != 0;
			bool flag3 = (input & (int)InputDefine.Right) != 0;
			if (flag)
			{
				bool flag4 = flag2 && !flag3;
				bool flag5 = flag3 && !flag2;
				if (isPlayerOne)
				{
					if (flag4)
					{
						return FootsiesGameServiceImpl.EnvAction.BackAttack;
					}
					if (flag5)
					{
						return FootsiesGameServiceImpl.EnvAction.ForwardAttack;
					}
				}
				else
				{
					if (flag4)
					{
						return FootsiesGameServiceImpl.EnvAction.ForwardAttack;
					}
					if (flag5)
					{
						return FootsiesGameServiceImpl.EnvAction.BackAttack;
					}
				}
				return FootsiesGameServiceImpl.EnvAction.Attack;
			}
			if (flag2 && flag3)
			{
				return FootsiesGameServiceImpl.EnvAction.None;
			}
			if (isPlayerOne)
			{
				if (flag2)
				{
					return FootsiesGameServiceImpl.EnvAction.Back;
				}
				if (flag3)
				{
					return FootsiesGameServiceImpl.EnvAction.Forward;
				}
			}
			else
			{
				if (flag2)
				{
					return FootsiesGameServiceImpl.EnvAction.Forward;
				}
				if (flag3)
				{
					return FootsiesGameServiceImpl.EnvAction.Back;
				}
			}
			return FootsiesGameServiceImpl.EnvAction.None;
		}

		// Token: 0x06000192 RID: 402
		private void LogGameState(GameState gameState)
		{
			Debug.Log(string.Format("GameState - FrameCount: {0}, RoundState: {1}", gameState.FrameCount, gameState.RoundState));
			this.LogPlayerState("Player 1", gameState.Player1);
			this.LogPlayerState("Player 2", gameState.Player2);
		}

		// Token: 0x06000193 RID: 403
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

		// Token: 0x060002E3 RID: 739
		private IEnumerator CaptureScreenshotCoroutine(int p1EnvAction, int p2EnvAction, int frameCount)
		{
			yield return new WaitForEndOfFrame();
			try
			{
				string filename = string.Format("{0:D06}_{1}_{2}.png", frameCount, p1EnvAction, p2EnvAction);
				string directory = "/mnt/d/Code/ray/recordings";
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				string savePath = Path.Combine(directory, filename);
				int width = Screen.width;
				int height = Screen.height;
				Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
				texture2D.ReadPixels(new Rect(0f, 0f, (float)width, (float)height), 0, 0);
				texture2D.Apply();
				byte[] bytes = texture2D.EncodeToPNG();
				File.WriteAllBytes(savePath, bytes);
				UnityEngine.Object.Destroy(texture2D);
				yield break;
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("CaptureScreenshot failed: {0}", ex));
				yield break;
			}
			yield break;
		}

		// Token: 0x04000125 RID: 293
		private BattleCore battleCore;

		// Token: 0x04000126 RID: 294
		private BattleGUI battleGUI;

		// Token: 0x0200002C RID: 44
		private enum EnvAction
		{
			// Token: 0x04000127 RID: 295
			None,
			// Token: 0x04000128 RID: 296
			Back,
			// Token: 0x04000129 RID: 297
			Forward,
			// Token: 0x0400012A RID: 298
			Attack,
			// Token: 0x0400012B RID: 299
			BackAttack,
			// Token: 0x0400012C RID: 300
			ForwardAttack,
			// Token: 0x0400012D RID: 301
			SpecialCharge
		}
	}
}
