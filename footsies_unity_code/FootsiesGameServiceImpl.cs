using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

namespace Footsies
{
	// Token: 0x0200003B RID: 59
	public class FootsiesGameServiceImpl : FootsiesGameService.FootsiesGameServiceBase
	{
		// Token: 0x060001DC RID: 476
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
					this.episodeNumber++;
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

		// Token: 0x060001DD RID: 477
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

		// Token: 0x060001DE RID: 478
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

		// Token: 0x060001DF RID: 479
		private bool CheckIfReady()
		{
			return Singleton<GameManager>.Instance != null && this.battleCore != null;
		}

		// Token: 0x02000060 RID: custom
		private enum ActionCategory
		{
			Idle,
			Move,
			Attack
		}

		// Token: 0x060001DE-Helper
		private ActionCategory GetExpectedCategoryFromBits(int bits)
		{
			bool hasAttack = (bits & FootsiesGameServiceImpl.AttackBit) != 0;
			bool hasMove = (bits & (FootsiesGameServiceImpl.LeftBit | FootsiesGameServiceImpl.RightBit)) != 0;
			if (hasAttack)
			{
				return ActionCategory.Attack;
			}
			if (hasMove)
			{
				return ActionCategory.Move;
			}
			return ActionCategory.Idle;
		}

		// Token: 0x060001DE-Helper
		private ActionCategory GetActualCategoryFromActionId(int actionId)
		{
			if (actionId == (int)CommonActionID.FORWARD || actionId == (int)CommonActionID.BACKWARD || actionId == (int)CommonActionID.DASH_FORWARD || actionId == (int)CommonActionID.DASH_BACKWARD)
			{
				return ActionCategory.Move;
			}
			if (actionId == (int)CommonActionID.N_ATTACK || actionId == (int)CommonActionID.B_ATTACK || actionId == (int)CommonActionID.N_SPECIAL || actionId == (int)CommonActionID.B_SPECIAL || actionId == (int)CommonActionID.DAMAGE)
			{
				return ActionCategory.Attack;
			}
			return ActionCategory.Idle;
		}

		// Token: 0x060001E0 RID: 480
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
					GameState preState = this.battleCore.GetGameState();
					bool p1Valid = this.IsPlayerInputValid(preState.Player1);
					bool p2Valid = this.IsPlayerInputValid(preState.Player2);
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
					ActionCategory expectedP1 = this.GetExpectedCategoryFromBits(num);
					ActionCategory expectedP2 = this.GetExpectedCategoryFromBits(num2);
					ActionCategory actualP1 = this.GetActualCategoryFromActionId((int)gameState.Player1.CurrentActionId);
					ActionCategory actualP2 = this.GetActualCategoryFromActionId((int)gameState.Player2.CurrentActionId);
					bool shouldCapture = (p1Valid && expectedP1 == actualP1) || (p2Valid && expectedP2 == actualP2);
					if (shouldCapture)
					{
						UnityMainThreadDispatcher.Instance.StartCoroutine(this.CaptureScreenshotCoroutine(num, num2, (int)gameState.FrameCount, p1Valid, p2Valid, (int)expectedP1, (int)actualP1, (int)expectedP2, (int)actualP2));
					}
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

		// Token: 0x060001E1 RID: 481
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

		// Token: 0x060001E2 RID: 482
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

		// Token: 0x060001E3 RID: 483
		private void EnqueueToMainThread(Action action)
		{
			UnityMainThreadDispatcher.Instance.Enqueue(action);
		}

		// Token: 0x060001E4 RID: 484
		private void LogGameState(GameState gameState)
		{
			Debug.Log(string.Format("GameState - FrameCount: {0}, RoundState: {1}", gameState.FrameCount, gameState.RoundState));
			this.LogPlayerState("Player 1", gameState.Player1);
			this.LogPlayerState("Player 2", gameState.Player2);
		}

		// Token: 0x060001E5 RID: 485
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

		// Token: 0x060001E9 RID: 489
		private bool IsPlayerInputValid(PlayerState player)
		{
			return !player.IsInHitStun && (player.IsActionEnd || player.IsAlwaysCancelable);
		}

		// Token: 0x0600025E RID: 606
		private IEnumerator CaptureScreenshotCoroutine(int p1InputBits, int p2InputBits, int frameCount, bool p1Valid, bool p2Valid, int p1ExpectedCategory, int p1ActualCategory, int p2ExpectedCategory, int p2ActualCategory)
		{
			yield return new WaitForEndOfFrame();
			try
			{
				string filename = string.Format("episode{0}_{1:D06}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}.png", new object[]
				{
					this.episodeNumber,
					frameCount,
					p1InputBits,
					p2InputBits,
					p1Valid ? 1 : 0,
					p2Valid ? 1 : 0,
					p1ExpectedCategory,
					p1ActualCategory,
					p2ExpectedCategory,
					p2ActualCategory
				});
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

		// Token: 0x04000153 RID: 339
		private BattleCore battleCore;

		// Token: 0x04000154 RID: 340
		private BattleGUI battleGUI;

		// Token: 0x04000155 RID: 341
		private int episodeNumber;

		// Token: 0x04000156 RID: custom
		private const int LeftBit = 1;

		// Token: 0x04000157 RID: custom
		private const int RightBit = 2;

		// Token: 0x04000158 RID: custom
		private const int AttackBit = 4;
	}
}
