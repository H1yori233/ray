using System;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
	// Token: 0x0200001D RID: 29
	public class BattleCore : MonoBehaviour
	{
		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000E8 RID: 232 RVA: 0x0000578C File Offset: 0x0000398C
		public float battleAreaWidth
		{
			get
			{
				return this._battleAreaWidth;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000E9 RID: 233 RVA: 0x00005794 File Offset: 0x00003994
		public float battleAreaMaxHeight
		{
			get
			{
				return this._battleAreaMaxHeight;
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000EA RID: 234 RVA: 0x0000579C File Offset: 0x0000399C
		// (set) Token: 0x060000EB RID: 235 RVA: 0x000057A4 File Offset: 0x000039A4
		public Fighter fighter1 { get; private set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000EC RID: 236 RVA: 0x000057AD File Offset: 0x000039AD
		// (set) Token: 0x060000ED RID: 237 RVA: 0x000057B5 File Offset: 0x000039B5
		public Fighter fighter2 { get; private set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000EE RID: 238 RVA: 0x000057BE File Offset: 0x000039BE
		// (set) Token: 0x060000EF RID: 239 RVA: 0x000057C6 File Offset: 0x000039C6
		public InputData ServerP1Input { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x000057CF File Offset: 0x000039CF
		// (set) Token: 0x060000F1 RID: 241 RVA: 0x000057D7 File Offset: 0x000039D7
		public InputData ServerP2Input { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000F2 RID: 242 RVA: 0x000057E0 File Offset: 0x000039E0
		// (set) Token: 0x060000F3 RID: 243 RVA: 0x000057E8 File Offset: 0x000039E8
		public uint fighter1RoundWon { get; private set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000F4 RID: 244 RVA: 0x000057F1 File Offset: 0x000039F1
		// (set) Token: 0x060000F5 RID: 245 RVA: 0x000057F9 File Offset: 0x000039F9
		public uint fighter2RoundWon { get; private set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000F6 RID: 246 RVA: 0x00005802 File Offset: 0x00003A02
		public List<Fighter> fighters
		{
			get
			{
				return this._fighters;
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000F7 RID: 247 RVA: 0x0000580A File Offset: 0x00003A0A
		public BattleCore.RoundStateType roundState
		{
			get
			{
				return this._roundState;
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000F8 RID: 248 RVA: 0x00005812 File Offset: 0x00003A12
		// (set) Token: 0x060000F9 RID: 249 RVA: 0x0000581A File Offset: 0x00003A1A
		public bool isDebugPause { get; private set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000FA RID: 250 RVA: 0x00005823 File Offset: 0x00003A23
		public bool IsUsingGrpcController
		{
			get
			{
				return this.useGrpcController;
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0000582C File Offset: 0x00003A2C
		private void Awake()
		{
			this.ParseCommandLineArgs();
			this.fighterDataList.ForEach(delegate(FighterData data)
			{
				data.setupDictionary();
			});
			this.fighter1 = new Fighter();
			this.fighter2 = new Fighter();
			this._fighters.Add(this.fighter1);
			this._fighters.Add(this.fighter2);
			if (this.roundUI != null)
			{
				this.roundUIAnimator = this.roundUI.GetComponent<Animator>();
			}
			if (this.useGrpcController)
			{
				this.ChangeRoundState(BattleCore.RoundStateType.Intro);
				this.ServerP1Input = new InputData
				{
					input = 0
				};
				this.ServerP2Input = new InputData
				{
					input = 0
				};
				this.UpdateIntroState();
				this.ChangeRoundState(BattleCore.RoundStateType.Fight);
			}
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00005900 File Offset: 0x00003B00
		private void ParseCommandLineArgs()
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i] == "--grpc" || commandLineArgs[i] == "-g")
				{
					this.useGrpcController = true;
					Debug.Log("gRPC controller enabled via command line");
					return;
				}
			}
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00005954 File Offset: 0x00003B54
		private void UpdateLogic()
		{
			switch (this._roundState)
			{
			case BattleCore.RoundStateType.Stop:
				this.ChangeRoundState(BattleCore.RoundStateType.Intro);
				return;
			case BattleCore.RoundStateType.Intro:
				this.UpdateIntroState();
				this.timer -= Time.deltaTime;
				if (this.timer <= 0f)
				{
					this.ChangeRoundState(BattleCore.RoundStateType.Fight);
				}
				if (this.debugPlayLastRoundInput && !this.isReplayingLastRoundInput)
				{
					this.StartPlayLastRoundInput();
					return;
				}
				break;
			case BattleCore.RoundStateType.Fight:
				if (!this.CheckUpdateDebugPause())
				{
					this.frameCount++;
					this.UpdateFightState();
					if (this._fighters.Find((Fighter f) => f.isDead) != null)
					{
						this.ChangeRoundState(BattleCore.RoundStateType.KO);
						return;
					}
				}
				break;
			case BattleCore.RoundStateType.KO:
				this.UpdateKOState();
				this.timer -= Time.deltaTime;
				if (this.timer <= 0f)
				{
					this.ChangeRoundState(BattleCore.RoundStateType.End);
					return;
				}
				break;
			case BattleCore.RoundStateType.End:
				this.UpdateEndState();
				this.timer -= Time.deltaTime;
				if (this.timer <= 0f || (this.timer <= this.endStateSkippableTime && this.IsKOSkipButtonPressed()))
				{
					this.ChangeRoundState(BattleCore.RoundStateType.Stop);
				}
				break;
			default:
				return;
			}
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00005A96 File Offset: 0x00003C96
		private void FixedUpdate()
		{
			if (this.useGrpcController)
			{
				return;
			}
			this.UpdateLogic();
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00005AA7 File Offset: 0x00003CA7
		public void ManualFixedUpdate()
		{
			this.UpdateLogic();
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00005AB0 File Offset: 0x00003CB0
		private void ChangeRoundState(BattleCore.RoundStateType state)
		{
			this._roundState = state;
			switch (this._roundState)
			{
			case BattleCore.RoundStateType.Stop:
				if (this.fighter1RoundWon >= this.maxRoundWon || this.fighter2RoundWon >= this.maxRoundWon)
				{
					Singleton<GameManager>.Instance.LoadTitleScene();
					return;
				}
				break;
			case BattleCore.RoundStateType.Intro:
				this.fighter1.SetupBattleStart(this.fighterDataList[0], new Vector2(-2f, 0f), true);
				this.fighter2.SetupBattleStart(this.fighterDataList[0], new Vector2(2f, 0f), false);
				this.timer = this.introStateTime;
				this.roundUIAnimator.SetTrigger("RoundStart");
				if (Singleton<GameManager>.Instance.isVsCPU)
				{
					this.barracudaAI = new BattleAIBarracuda(this);
				}
				if (Singleton<GameManager>.Instance.barracudaModel == null)
				{
					Debug.LogError("Barracuda model not assigned! Please assign it in the Unity Inspector.");
					return;
				}
				break;
			case BattleCore.RoundStateType.Fight:
				this.roundStartTime = Time.fixedTime;
				this.frameCount = -1;
				this.currentRecordingInputIndex = 0U;
				return;
			case BattleCore.RoundStateType.KO:
				this.timer = this.koStateTime;
				this.CopyLastRoundInput();
				this.fighter1.ClearInput();
				this.fighter2.ClearInput();
				this.battleAI = null;
				if (this.barracudaAI != null)
				{
					this.barracudaAI.Dispose();
					this.barracudaAI = null;
				}
				this.roundUIAnimator.SetTrigger("RoundEnd");
				return;
			case BattleCore.RoundStateType.End:
			{
				this.timer = this.endStateTime;
				List<Fighter> list = this._fighters.FindAll((Fighter f) => f.isDead);
				if (list.Count == 1)
				{
					if (list[0] == this.fighter1)
					{
						uint num = this.fighter2RoundWon;
						this.fighter2RoundWon = num + 1U;
						this.fighter2.RequestWinAction();
						return;
					}
					if (list[0] == this.fighter2)
					{
						uint num = this.fighter1RoundWon;
						this.fighter1RoundWon = num + 1U;
						this.fighter1.RequestWinAction();
					}
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00005CBC File Offset: 0x00003EBC
		private void UpdateIntroState()
		{
			InputData p1InputData = this.GetP1InputData();
			InputData p2InputData = this.GetP2InputData();
			this.RecordInput(p1InputData, p2InputData);
			this.fighter1.UpdateInput(p1InputData);
			this.fighter2.UpdateInput(p2InputData);
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.IncrementActionFrame();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateIntroAction();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateMovement();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateBoxes();
			});
			this.UpdatePushCharacterVsCharacter();
			this.UpdatePushCharacterVsBackground();
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00005DAC File Offset: 0x00003FAC
		private void UpdateFightState()
		{
			InputData p1InputData = this.GetP1InputData();
			InputData p2InputData = this.GetP2InputData();
			this.RecordInput(p1InputData, p2InputData);
			this.fighter1.UpdateInput(p1InputData);
			this.fighter2.UpdateInput(p2InputData);
			this.fighter1.currentFrameAdvantage = this.GetFrameAdvantage(true);
			this.fighter2.currentFrameAdvantage = this.GetFrameAdvantage(false);
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.IncrementActionFrame();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateActionRequest();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateMovement();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateBoxes();
			});
			this.UpdatePushCharacterVsCharacter();
			this.UpdatePushCharacterVsBackground();
			this.UpdateHitboxHurtboxCollision();
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00005EC5 File Offset: 0x000040C5
		private void UpdateKOState()
		{
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00005EC8 File Offset: 0x000040C8
		private void UpdateEndState()
		{
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.IncrementActionFrame();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateActionRequest();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateMovement();
			});
			this._fighters.ForEach(delegate(Fighter f)
			{
				f.UpdateBoxes();
			});
			this.UpdatePushCharacterVsCharacter();
			this.UpdatePushCharacterVsBackground();
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00005F8C File Offset: 0x0000418C
		private InputData GetP1InputData()
		{
			if (this.isReplayingLastRoundInput)
			{
				return this.lastRoundP1Input[(int)this.currentReplayingInputIndex];
			}
			float time = Time.fixedTime - this.roundStartTime;
			InputData inputData = new InputData();
			if (this.useGrpcController)
			{
				inputData.input = this.ServerP1Input.input;
			}
			else
			{
				inputData.input |= (Singleton<InputManager>.Instance.GetButton(InputManager.Command.p1Left) ? 1 : 0);
				inputData.input |= (Singleton<InputManager>.Instance.GetButton(InputManager.Command.p1Right) ? 2 : 0);
				inputData.input |= (Singleton<InputManager>.Instance.GetButton(InputManager.Command.p1Attack) ? 4 : 0);
			}
			inputData.time = time;
			if (this.debugP1Attack)
			{
				inputData.input |= 4;
			}
			if (this.debugP1Guard)
			{
				inputData.input |= 1;
			}
			return inputData;
		}

		// Token: 0x06000106 RID: 262 RVA: 0x0000606B File Offset: 0x0000426B
		public void SetP1InputData(int input)
		{
			this.ServerP1Input = new InputData
			{
				input = input
			};
		}

		// Token: 0x06000107 RID: 263 RVA: 0x0000607F File Offset: 0x0000427F
		public void SetP2InputData(int input)
		{
			this.ServerP2Input = new InputData
			{
				input = input
			};
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00006093 File Offset: 0x00004293
		public void ClearP1InputData()
		{
			this.ServerP1Input = null;
		}

		// Token: 0x06000109 RID: 265 RVA: 0x0000609C File Offset: 0x0000429C
		public void ClearP2InputData()
		{
			this.ServerP2Input = null;
		}

		// Token: 0x0600010A RID: 266 RVA: 0x000060A8 File Offset: 0x000042A8
		private InputData GetP2InputData()
		{
			if (this.isReplayingLastRoundInput)
			{
				return this.lastRoundP2Input[(int)this.currentReplayingInputIndex];
			}
			float time = Time.fixedTime - this.roundStartTime;
			InputData inputData = new InputData();
			if (this.useGrpcController)
			{
				inputData.input = this.ServerP2Input.input;
			}
			else if (this.barracudaAI != null)
			{
				inputData.input |= this.barracudaAI.getNextAIInput(false);
			}
			else
			{
				inputData.input |= (Singleton<InputManager>.Instance.GetButton(InputManager.Command.p2Left) ? 1 : 0);
				inputData.input |= (Singleton<InputManager>.Instance.GetButton(InputManager.Command.p2Right) ? 2 : 0);
				inputData.input |= (Singleton<InputManager>.Instance.GetButton(InputManager.Command.p2Attack) ? 4 : 0);
			}
			inputData.time = time;
			if (this.debugP2Attack)
			{
				inputData.input |= 4;
			}
			if (this.debugP2Guard)
			{
				inputData.input |= 2;
			}
			return inputData;
		}

		// Token: 0x0600010B RID: 267 RVA: 0x000061AA File Offset: 0x000043AA
		private bool IsKOSkipButtonPressed()
		{
			return Singleton<InputManager>.Instance.GetButton(InputManager.Command.p1Attack) || Singleton<InputManager>.Instance.GetButton(InputManager.Command.p2Attack);
		}

		// Token: 0x0600010C RID: 268 RVA: 0x000061CC File Offset: 0x000043CC
		private void UpdatePushCharacterVsCharacter()
		{
			Rect rect = this.fighter1.pushbox.rect;
			Rect rect2 = this.fighter2.pushbox.rect;
			if (rect.Overlaps(rect2))
			{
				if (this.fighter1.position.x < this.fighter2.position.x)
				{
					this.fighter1.ApplyPositionChange((rect.xMax - rect2.xMin) * -1f / 2f, this.fighter1.position.y);
					this.fighter2.ApplyPositionChange((rect.xMax - rect2.xMin) * 1f / 2f, this.fighter2.position.y);
					return;
				}
				if (this.fighter1.position.x > this.fighter2.position.x)
				{
					this.fighter1.ApplyPositionChange((rect2.xMax - rect.xMin) * 1f / 2f, this.fighter1.position.y);
					this.fighter2.ApplyPositionChange((rect2.xMax - rect.xMin) * -1f / 2f, this.fighter1.position.y);
				}
			}
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00006328 File Offset: 0x00004528
		private void UpdatePushCharacterVsBackground()
		{
			float stageMinX = this.battleAreaWidth * -1f / 2f;
			float stageMaxX = this.battleAreaWidth / 2f;
			this._fighters.ForEach(delegate(Fighter f)
			{
				if (f.pushbox.xMin < stageMinX)
				{
					f.ApplyPositionChange(stageMinX - f.pushbox.xMin, f.position.y);
					return;
				}
				if (f.pushbox.xMax > stageMaxX)
				{
					f.ApplyPositionChange(stageMaxX - f.pushbox.xMax, f.position.y);
				}
			});
		}

		// Token: 0x0600010E RID: 270 RVA: 0x0000637C File Offset: 0x0000457C
		private void UpdateHitboxHurtboxCollision()
		{
			foreach (Fighter fighter in this._fighters)
			{
				Vector2 zero = Vector2.zero;
				bool flag = false;
				bool flag2 = false;
				int attackID = 0;
				foreach (Fighter fighter2 in this._fighters)
				{
					if (fighter != fighter2)
					{
						foreach (Hitbox hitbox in fighter.hitboxes)
						{
							if (fighter.CanAttackHit(hitbox.attackID))
							{
								foreach (Hurtbox hurtbox in fighter2.hurtboxes)
								{
									if (hitbox.Overlaps(hurtbox))
									{
										if (!hitbox.proximity)
										{
											flag = true;
											attackID = hitbox.attackID;
											float num = Mathf.Min(hitbox.xMax, hurtbox.xMax);
											float num2 = Mathf.Max(hitbox.xMin, hurtbox.xMin);
											float num3 = Mathf.Min(hitbox.yMax, hurtbox.yMax);
											float num4 = Mathf.Max(hitbox.yMin, hurtbox.yMin);
											zero.x = (num + num2) / 2f;
											zero.y = (num3 + num4) / 2f;
											break;
										}
										flag2 = true;
									}
								}
								if (flag)
								{
									break;
								}
							}
						}
						if (flag)
						{
							fighter.NotifyAttackHit(fighter2, zero);
							DamageResult damageResult = fighter2.NotifyDamaged(fighter.getAttackData(attackID), zero);
							int hitStunFrame = fighter.GetHitStunFrame(damageResult, attackID);
							fighter.SetHitStun(hitStunFrame);
							fighter2.SetHitStun(hitStunFrame);
							fighter2.SetSpriteShakeFrame(hitStunFrame / 3);
							this.damageHandler(fighter2, zero, damageResult);
						}
						else if (flag2)
						{
							fighter2.NotifyInProximityGuardRange();
						}
					}
				}
			}
		}

		// Token: 0x0600010F RID: 271 RVA: 0x000065FC File Offset: 0x000047FC
		private void RecordInput(InputData p1Input, InputData p2Input)
		{
			if (this.currentRecordingInputIndex >= BattleCore.maxRecordingInputFrame)
			{
				return;
			}
			this.recordingP1Input[(int)this.currentRecordingInputIndex] = p1Input.ShallowCopy();
			this.recordingP2Input[(int)this.currentRecordingInputIndex] = p2Input.ShallowCopy();
			this.currentRecordingInputIndex += 1U;
			if (this.isReplayingLastRoundInput && this.currentReplayingInputIndex < this.lastRoundMaxRecordingInput)
			{
				this.currentReplayingInputIndex += 1U;
			}
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00006670 File Offset: 0x00004870
		private void CopyLastRoundInput()
		{
			int num = 0;
			while ((long)num < (long)((ulong)this.currentRecordingInputIndex))
			{
				this.lastRoundP1Input[num] = this.recordingP1Input[num].ShallowCopy();
				this.lastRoundP2Input[num] = this.recordingP2Input[num].ShallowCopy();
				num++;
			}
			this.lastRoundMaxRecordingInput = this.currentRecordingInputIndex;
			this.isReplayingLastRoundInput = false;
			this.currentReplayingInputIndex = 0U;
		}

		// Token: 0x06000111 RID: 273 RVA: 0x000066D4 File Offset: 0x000048D4
		private void StartPlayLastRoundInput()
		{
			this.isReplayingLastRoundInput = true;
			this.currentReplayingInputIndex = 0U;
		}

		// Token: 0x06000112 RID: 274 RVA: 0x000066E4 File Offset: 0x000048E4
		private bool CheckUpdateDebugPause()
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				this.isDebugPause = !this.isDebugPause;
			}
			return this.isDebugPause && !Input.GetKeyDown(KeyCode.F2);
		}

		// Token: 0x06000113 RID: 275 RVA: 0x0000671C File Offset: 0x0000491C
		public int GetFrameAdvantage(bool getP1)
		{
			int num = this.fighter1.currentActionFrameCount - this.fighter1.currentActionFrame;
			if (this.fighter1.isAlwaysCancelable)
			{
				num = 0;
			}
			int num2 = this.fighter2.currentActionFrameCount - this.fighter2.currentActionFrame;
			if (this.fighter2.isAlwaysCancelable)
			{
				num2 = 0;
			}
			if (getP1)
			{
				return num2 - num;
			}
			return num - num2;
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00006784 File Offset: 0x00004984
		public GameState GetGameState()
		{
			return new GameState
			{
				Player1 = this.fighter1.getPlayerState(),
				Player2 = this.fighter2.getPlayerState(),
				RoundState = (long)this._roundState,
				FrameCount = (long)this.frameCount
			};
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000067D4 File Offset: 0x000049D4
		public EncodedGameState GetEncodedGameState()
		{
			EncodedGameState encodedGameState = new EncodedGameState();
			encodedGameState.Player1Encoding.AddRange(this.encoder.EncodeGameState(this.GetGameState(), true));
			encodedGameState.Player2Encoding.AddRange(this.encoder.EncodeGameState(this.GetGameState(), false));
			return encodedGameState;
		}

		// Token: 0x040000A1 RID: 161
		[SerializeField]
		private float _battleAreaWidth = 10f;

		// Token: 0x040000A2 RID: 162
		[SerializeField]
		private float _battleAreaMaxHeight = 2f;

		// Token: 0x040000A3 RID: 163
		[SerializeField]
		private GameObject roundUI;

		// Token: 0x040000A4 RID: 164
		[SerializeField]
		private List<FighterData> fighterDataList = new List<FighterData>();

		// Token: 0x040000A5 RID: 165
		public bool debugP1Attack;

		// Token: 0x040000A6 RID: 166
		public bool debugP2Attack;

		// Token: 0x040000A7 RID: 167
		public bool debugP1Guard;

		// Token: 0x040000A8 RID: 168
		public bool debugP2Guard;

		// Token: 0x040000A9 RID: 169
		public bool debugPlayLastRoundInput;

		// Token: 0x040000AA RID: 170
		private float timer;

		// Token: 0x040000AB RID: 171
		private uint maxRoundWon = 3U;

		// Token: 0x040000B2 RID: 178
		private List<Fighter> _fighters = new List<Fighter>();

		// Token: 0x040000B3 RID: 179
		private float roundStartTime;

		// Token: 0x040000B4 RID: 180
		private int frameCount;

		// Token: 0x040000B5 RID: 181
		private BattleCore.RoundStateType _roundState;

		// Token: 0x040000B6 RID: 182
		public Action<Fighter, Vector2, DamageResult> damageHandler;

		// Token: 0x040000B7 RID: 183
		private Animator roundUIAnimator;

		// Token: 0x040000B8 RID: 184
		private BattleAI battleAI;

		// Token: 0x040000B9 RID: 185
		private BattleAIBarracuda barracudaAI;

		// Token: 0x040000BA RID: 186
		[SerializeField]
		private AIEncoder encoder = new AIEncoder();

		// Token: 0x040000BB RID: 187
		private static uint maxRecordingInputFrame = 18000U;

		// Token: 0x040000BC RID: 188
		private InputData[] recordingP1Input = new InputData[BattleCore.maxRecordingInputFrame];

		// Token: 0x040000BD RID: 189
		private InputData[] recordingP2Input = new InputData[BattleCore.maxRecordingInputFrame];

		// Token: 0x040000BE RID: 190
		private uint currentRecordingInputIndex;

		// Token: 0x040000BF RID: 191
		private InputData[] lastRoundP1Input = new InputData[BattleCore.maxRecordingInputFrame];

		// Token: 0x040000C0 RID: 192
		private InputData[] lastRoundP2Input = new InputData[BattleCore.maxRecordingInputFrame];

		// Token: 0x040000C1 RID: 193
		private uint currentReplayingInputIndex;

		// Token: 0x040000C2 RID: 194
		private uint lastRoundMaxRecordingInput;

		// Token: 0x040000C3 RID: 195
		private bool isReplayingLastRoundInput;

		// Token: 0x040000C5 RID: 197
		private bool useGrpcController = true;

		// Token: 0x040000C6 RID: 198
		private float introStateTime = 3f;

		// Token: 0x040000C7 RID: 199
		private float koStateTime = 2f;

		// Token: 0x040000C8 RID: 200
		private float endStateTime = 3f;

		// Token: 0x040000C9 RID: 201
		private float endStateSkippableTime = 1.5f;

		// Token: 0x02000045 RID: 69
		public enum RoundStateType
		{
			// Token: 0x04000168 RID: 360
			Stop,
			// Token: 0x04000169 RID: 361
			Intro,
			// Token: 0x0400016A RID: 362
			Fight,
			// Token: 0x0400016B RID: 363
			KO,
			// Token: 0x0400016C RID: 364
			End
		}
	}
}
