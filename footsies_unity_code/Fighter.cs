using System;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
	// Token: 0x02000027 RID: 39
	public class Fighter
	{
		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000138 RID: 312 RVA: 0x0000749A File Offset: 0x0000569A
		public bool isDead
		{
			get
			{
				return this.vitalHealth <= 0;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000139 RID: 313 RVA: 0x000074A8 File Offset: 0x000056A8
		// (set) Token: 0x0600013A RID: 314 RVA: 0x000074B0 File Offset: 0x000056B0
		public int vitalHealth { get; private set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600013B RID: 315 RVA: 0x000074B9 File Offset: 0x000056B9
		// (set) Token: 0x0600013C RID: 316 RVA: 0x000074C1 File Offset: 0x000056C1
		public int guardHealth { get; private set; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x0600013D RID: 317 RVA: 0x000074CA File Offset: 0x000056CA
		// (set) Token: 0x0600013E RID: 318 RVA: 0x000074D2 File Offset: 0x000056D2
		public int currentActionID { get; private set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x0600013F RID: 319 RVA: 0x000074DB File Offset: 0x000056DB
		// (set) Token: 0x06000140 RID: 320 RVA: 0x000074E3 File Offset: 0x000056E3
		public int currentActionFrame { get; private set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000141 RID: 321 RVA: 0x000074EC File Offset: 0x000056EC
		public int currentActionFrameCount
		{
			get
			{
				return this.fighterData.actions[this.currentActionID].frameCount;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000142 RID: 322 RVA: 0x00007509 File Offset: 0x00005709
		private bool isActionEnd
		{
			get
			{
				return this.currentActionFrame >= this.fighterData.actions[this.currentActionID].frameCount;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000143 RID: 323 RVA: 0x00007531 File Offset: 0x00005731
		public bool isAlwaysCancelable
		{
			get
			{
				return this.fighterData.actions[this.currentActionID].alwaysCancelable;
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000144 RID: 324 RVA: 0x0000754E File Offset: 0x0000574E
		// (set) Token: 0x06000145 RID: 325 RVA: 0x00007556 File Offset: 0x00005756
		public int currentActionHitCount { get; private set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000146 RID: 326 RVA: 0x0000755F File Offset: 0x0000575F
		// (set) Token: 0x06000147 RID: 327 RVA: 0x00007567 File Offset: 0x00005767
		public int currentFrameAdvantage { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000148 RID: 328 RVA: 0x00007570 File Offset: 0x00005770
		// (set) Token: 0x06000149 RID: 329 RVA: 0x00007578 File Offset: 0x00005778
		public int currentHitStunFrame { get; private set; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x0600014A RID: 330 RVA: 0x00007581 File Offset: 0x00005781
		public bool isInHitStun
		{
			get
			{
				return this.currentHitStunFrame > 0;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x0600014B RID: 331 RVA: 0x0000758C File Offset: 0x0000578C
		// (set) Token: 0x0600014C RID: 332 RVA: 0x00007594 File Offset: 0x00005794
		public int spriteShakePosition { get; private set; }

		// Token: 0x0600014D RID: 333 RVA: 0x000075A0 File Offset: 0x000057A0
		public void SetupBattleStart(FighterData fighterData, Vector2 startPosition, bool isPlayerOne)
		{
			this.fighterData = fighterData;
			this.position = startPosition;
			this.isFaceRight = isPlayerOne;
			this.vitalHealth = 1;
			this.guardHealth = fighterData.startGuardHealth;
			this.hasWon = false;
			this.velocity_x = 0f;
			this.ClearInput();
			this.SetCurrentAction(0, 0);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x000075F8 File Offset: 0x000057F8
		public void IncrementActionFrame()
		{
			if (Mathf.Abs(this.spriteShakePosition) > 0)
			{
				this.spriteShakePosition *= -1;
				this.spriteShakePosition += ((this.spriteShakePosition > 0) ? -1 : 1);
			}
			int num;
			if (this.currentHitStunFrame > 0)
			{
				num = this.currentHitStunFrame;
				this.currentHitStunFrame = num - 1;
				return;
			}
			num = this.currentActionFrame;
			this.currentActionFrame = num + 1;
			if (this.isActionEnd && this.fighterData.actions[this.currentActionID].isLoop)
			{
				this.currentActionFrame = this.fighterData.actions[this.currentActionID].loopFromFrame;
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x000076AC File Offset: 0x000058AC
		public void UpdateInput(InputData inputData)
		{
			for (int i = this.input.Length - 1; i >= 1; i--)
			{
				this.input[i] = this.input[i - 1];
				this.inputDown[i] = this.inputDown[i - 1];
				this.inputUp[i] = this.inputUp[i - 1];
			}
			this.input[0] = inputData.input;
			this.inputDown[0] = ((this.input[0] ^ this.input[1]) & this.input[0]);
			this.inputUp[0] = ((this.input[0] ^ this.input[1]) & ~this.input[0]);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00007757 File Offset: 0x00005957
		public void UpdateIntroAction()
		{
			this.RequestAction(0, 0);
		}

		// Token: 0x06000151 RID: 337 RVA: 0x00007764 File Offset: 0x00005964
		public void UpdateActionRequest()
		{
			if (this.hasWon)
			{
				this.RequestAction(510, 0);
				return;
			}
			if (this.reserveDamageActionID != -1 && this.currentHitStunFrame <= 0)
			{
				this.SetCurrentAction(this.reserveDamageActionID, 0);
				this.reserveDamageActionID = -1;
				return;
			}
			if (this.bufferActionID != -1 && this.canCancelAttack() && this.currentHitStunFrame <= 0)
			{
				this.SetCurrentAction(this.bufferActionID, 0);
				this.bufferActionID = -1;
				return;
			}
			bool flag = this.IsForwardInput(this.input[0]);
			bool flag2 = this.IsBackwardInput(this.input[0]);
			bool flag3 = this.IsAttackInput(this.inputDown[0]);
			if (this.CheckSpecialAttackInput())
			{
				if (flag2 || flag)
				{
					this.RequestAction(115, 0);
				}
				else
				{
					this.RequestAction(110, 0);
				}
			}
			else if (flag3)
			{
				if ((this.currentActionID == 100 || this.currentActionID == 105) && !this.isActionEnd)
				{
					this.RequestAction(110, 0);
				}
				else if (flag2 || flag)
				{
					this.RequestAction(105, 0);
				}
				else
				{
					this.RequestAction(100, 0);
				}
			}
			if (this.CheckForwardDashInput())
			{
				this.RequestAction(10, 0);
			}
			else if (this.CheckBackwardDashInput())
			{
				this.RequestAction(11, 0);
			}
			this.isInputBackward = flag2;
			if (flag && flag2)
			{
				this.RequestAction(0, 0);
			}
			else if (flag)
			{
				this.RequestAction(1, 0);
			}
			else if (flag2)
			{
				if (this.isReserveProximityGuard)
				{
					this.RequestAction(350, 0);
				}
				else
				{
					this.RequestAction(2, 0);
				}
			}
			else
			{
				this.RequestAction(0, 0);
			}
			this.isReserveProximityGuard = false;
		}

		// Token: 0x06000152 RID: 338 RVA: 0x000078F8 File Offset: 0x00005AF8
		public void UpdateMovement()
		{
			if (this.isInHitStun)
			{
				return;
			}
			int num = this.isFaceRight ? 1 : -1;
			if (this.currentActionID == 1)
			{
				this.position.x = this.position.x + this.fighterData.forwardMoveSpeed * (float)num * Time.deltaTime;
				return;
			}
			if (this.currentActionID == 2)
			{
				this.position.x = this.position.x - this.fighterData.backwardMoveSpeed * (float)num * Time.deltaTime;
				return;
			}
			MovementData movementData = this.fighterData.actions[this.currentActionID].GetMovementData(this.currentActionFrame);
			if (movementData != null)
			{
				this.velocity_x = movementData.velocity_x;
				if (this.velocity_x != 0f)
				{
					this.position.x = this.position.x + this.velocity_x * (float)num * Time.deltaTime;
				}
			}
		}

		// Token: 0x06000153 RID: 339 RVA: 0x000079D1 File Offset: 0x00005BD1
		public void UpdateBoxes()
		{
			this.ApplyCurrentActionData();
		}

		// Token: 0x06000154 RID: 340 RVA: 0x000079DC File Offset: 0x00005BDC
		public void ApplyPositionChange(float x, float y)
		{
			this.position.x = this.position.x + x;
			this.position.y = this.position.y + y;
			foreach (Hitbox hitbox in this.hitboxes)
			{
				hitbox.rect.x = hitbox.rect.x + x;
				hitbox.rect.y = hitbox.rect.y + y;
			}
			foreach (Hurtbox hurtbox in this.hurtboxes)
			{
				hurtbox.rect.x = hurtbox.rect.x + x;
				hurtbox.rect.y = hurtbox.rect.y + y;
			}
			Pushbox pushbox = this.pushbox;
			pushbox.rect.x = pushbox.rect.x + x;
			Pushbox pushbox2 = this.pushbox;
			pushbox2.rect.y = pushbox2.rect.y + y;
		}

		// Token: 0x06000155 RID: 341 RVA: 0x00007AFC File Offset: 0x00005CFC
		public void NotifyAttackHit(Fighter damagedFighter, Vector2 damagePos)
		{
			int currentActionHitCount = this.currentActionHitCount;
			this.currentActionHitCount = currentActionHitCount + 1;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x00007B1C File Offset: 0x00005D1C
		public DamageResult NotifyDamaged(AttackData attackData, Vector2 damagePos)
		{
			bool flag = false;
			if (attackData.guardHealthDamage > 0)
			{
				this.guardHealth -= attackData.guardHealthDamage;
				if (this.guardHealth < 0)
				{
					flag = true;
					this.guardHealth = 0;
				}
			}
			if (this.currentActionID != 2 && this.fighterData.actions[this.currentActionID].Type != ActionType.Guard)
			{
				if (attackData.vitalHealthDamage > 0)
				{
					this.vitalHealth -= attackData.vitalHealthDamage;
					if (this.vitalHealth <= 0)
					{
						this.vitalHealth = 0;
					}
				}
				this.SetCurrentAction(attackData.damageActionID, 0);
				return DamageResult.Damage;
			}
			if (flag)
			{
				this.SetCurrentAction(attackData.guardActionID, 0);
				this.reserveDamageActionID = 310;
				Singleton<SoundManager>.Instance.playFighterSE(this.fighterData.actions[this.reserveDamageActionID].audioClip, this.isFaceRight, this.position.x);
				return DamageResult.GuardBreak;
			}
			this.SetCurrentAction(attackData.guardActionID, 0);
			return DamageResult.Guard;
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00007C1C File Offset: 0x00005E1C
		public void NotifyInProximityGuardRange()
		{
			if (this.isInputBackward)
			{
				this.isReserveProximityGuard = true;
			}
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00007C30 File Offset: 0x00005E30
		public bool CanAttackHit(int attackID)
		{
			if (!this.fighterData.attackData.ContainsKey(attackID))
			{
				Debug.LogWarning("Attack hit but AttackID=" + attackID.ToString() + " is not registered");
				return true;
			}
			return this.currentActionHitCount < this.fighterData.attackData[attackID].numberOfHit;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x00007C90 File Offset: 0x00005E90
		public AttackData getAttackData(int attackID)
		{
			if (!this.fighterData.attackData.ContainsKey(attackID))
			{
				Debug.LogWarning("Attack hit but AttackID=" + attackID.ToString() + " is not registered");
				return null;
			}
			return this.fighterData.attackData[attackID];
		}

		// Token: 0x0600015A RID: 346 RVA: 0x00007CDE File Offset: 0x00005EDE
		public void SetHitStun(int hitStunFrame)
		{
			this.currentHitStunFrame = hitStunFrame;
		}

		// Token: 0x0600015B RID: 347 RVA: 0x00007CE7 File Offset: 0x00005EE7
		public void SetSpriteShakeFrame(int spriteShakeFrame)
		{
			if (spriteShakeFrame > this.maxSpriteShakeFrame)
			{
				spriteShakeFrame = this.maxSpriteShakeFrame;
			}
			this.spriteShakePosition = spriteShakeFrame * (this.isFaceRight ? -1 : 1);
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00007D10 File Offset: 0x00005F10
		public int GetHitStunFrame(DamageResult damageResult, int attackID)
		{
			if (damageResult == DamageResult.Guard)
			{
				return this.fighterData.attackData[attackID].guardStunFrame;
			}
			if (damageResult == DamageResult.GuardBreak)
			{
				return this.fighterData.attackData[attackID].guardBreakStunFrame;
			}
			return this.fighterData.attackData[attackID].hitStunFrame;
		}

		// Token: 0x0600015D RID: 349 RVA: 0x00007D69 File Offset: 0x00005F69
		public int getGuardStunFrame(int attackID)
		{
			return this.fighterData.attackData[attackID].guardStunFrame;
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00007D81 File Offset: 0x00005F81
		public void RequestWinAction()
		{
			this.hasWon = true;
		}

		// Token: 0x0600015F RID: 351 RVA: 0x00007D8C File Offset: 0x00005F8C
		public bool RequestAction(int actionID, int startFrame = 0)
		{
			if (this.isActionEnd)
			{
				this.SetCurrentAction(actionID, startFrame);
				return true;
			}
			if (this.currentActionID == actionID)
			{
				return false;
			}
			if (this.fighterData.actions[this.currentActionID].alwaysCancelable)
			{
				this.SetCurrentAction(actionID, startFrame);
				return true;
			}
			foreach (CancelData cancelData in this.fighterData.actions[this.currentActionID].GetCancelData(this.currentActionFrame))
			{
				if (cancelData.actionID.Contains(actionID))
				{
					if (cancelData.execute)
					{
						this.bufferActionID = actionID;
						return true;
					}
					if (cancelData.buffer)
					{
						this.bufferActionID = actionID;
					}
				}
			}
			return false;
		}

		// Token: 0x06000160 RID: 352 RVA: 0x00007E6C File Offset: 0x0000606C
		public Sprite GetCurrentMotionSprite()
		{
			MotionFrameData motionData = this.fighterData.actions[this.currentActionID].GetMotionData(this.currentActionFrame);
			if (motionData == null)
			{
				return null;
			}
			return this.fighterData.motionData[motionData.motionID].sprite;
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00007EBC File Offset: 0x000060BC
		public void ClearInput()
		{
			for (int i = 0; i < this.input.Length; i++)
			{
				this.input[i] = 0;
				this.inputDown[i] = 0;
				this.inputUp[i] = 0;
			}
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00007EF7 File Offset: 0x000060F7
		private bool canCancelAttack()
		{
			return this.fighterData.canCancelOnWhiff || this.currentActionHitCount > 0;
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00007F14 File Offset: 0x00006114
		private void SetCurrentAction(int actionID, int startFrame = 0)
		{
			this.currentActionID = actionID;
			this.currentActionFrame = startFrame;
			this.currentActionHitCount = 0;
			this.bufferActionID = -1;
			this.reserveDamageActionID = -1;
			this.spriteShakePosition = 0;
			if (this.fighterData.actions[this.currentActionID].audioClip != null)
			{
				if (this.currentActionID == 310)
				{
					return;
				}
				Singleton<SoundManager>.Instance.playFighterSE(this.fighterData.actions[this.currentActionID].audioClip, this.isFaceRight, this.position.x);
			}
		}

		// Token: 0x06000164 RID: 356 RVA: 0x00007FB4 File Offset: 0x000061B4
		private bool CheckSpecialAttackInput()
		{
			if (!this.IsAttackInput(this.inputUp[0]))
			{
				return false;
			}
			for (int i = 1; i < this.fighterData.specialAttackHoldFrame; i++)
			{
				if (!this.IsAttackInput(this.input[i]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00007FFC File Offset: 0x000061FC
		private float GetSpecialAttackProgress()
		{
			int num = 0;
			int num2 = 0;
			while (num2 < this.fighterData.specialAttackHoldFrame && this.IsAttackInput(this.input[num2]))
			{
				num++;
				num2++;
			}
			return (float)num / (float)this.fighterData.specialAttackHoldFrame;
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00008044 File Offset: 0x00006244
		private bool CheckForwardDashInput()
		{
			if (!this.IsForwardInput(this.inputDown[0]))
			{
				return false;
			}
			for (int i = 1; i < this.fighterData.dashAllowFrame; i++)
			{
				if (this.IsBackwardInput(this.input[i]))
				{
					return false;
				}
				if (this.IsForwardInput(this.input[i]))
				{
					for (int j = i + 1; j < i + this.fighterData.dashAllowFrame; j++)
					{
						if (!this.IsForwardInput(this.input[j]) && !this.IsBackwardInput(this.input[j]))
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		// Token: 0x06000167 RID: 359 RVA: 0x000080DC File Offset: 0x000062DC
		private bool CheckBackwardDashInput()
		{
			if (!this.IsBackwardInput(this.inputDown[0]))
			{
				return false;
			}
			for (int i = 1; i < this.fighterData.dashAllowFrame; i++)
			{
				if (this.IsForwardInput(this.input[i]))
				{
					return false;
				}
				if (this.IsBackwardInput(this.input[i]))
				{
					for (int j = i + 1; j < i + this.fighterData.dashAllowFrame; j++)
					{
						if (!this.IsForwardInput(this.input[j]) && !this.IsBackwardInput(this.input[j]))
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00008174 File Offset: 0x00006374
		private bool WouldNextForwardInputDash()
		{
			for (int i = 0; i < this.fighterData.dashAllowFrame - 1; i++)
			{
				if (this.IsBackwardInput(this.input[i]))
				{
					return false;
				}
				if (this.IsForwardInput(this.input[i]))
				{
					for (int j = i + 1; j < i + this.fighterData.dashAllowFrame; j++)
					{
						if (!this.IsForwardInput(this.input[j]) && !this.IsBackwardInput(this.input[j]))
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		// Token: 0x06000169 RID: 361 RVA: 0x000081FC File Offset: 0x000063FC
		private bool WouldNextBackwardInputDash()
		{
			for (int i = 0; i < this.fighterData.dashAllowFrame - 1; i++)
			{
				if (this.IsForwardInput(this.input[i]))
				{
					return false;
				}
				if (this.IsBackwardInput(this.input[i]))
				{
					for (int j = i + 1; j < i + this.fighterData.dashAllowFrame; j++)
					{
						if (!this.IsForwardInput(this.input[j]) && !this.IsBackwardInput(this.input[j]))
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00008282 File Offset: 0x00006482
		private bool IsAttackInput(int input)
		{
			return (input & 4) > 0;
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0000828A File Offset: 0x0000648A
		private bool IsForwardInput(int input)
		{
			if (this.isFaceRight)
			{
				return (input & 2) > 0;
			}
			return (input & 1) > 0;
		}

		// Token: 0x0600016C RID: 364 RVA: 0x000082A1 File Offset: 0x000064A1
		private bool IsBackwardInput(int input)
		{
			if (this.isFaceRight)
			{
				return (input & 1) > 0;
			}
			return (input & 2) > 0;
		}

		// Token: 0x0600016D RID: 365 RVA: 0x000082B8 File Offset: 0x000064B8
		private void ApplyCurrentActionData()
		{
			this.hitboxes.Clear();
			this.hurtboxes.Clear();
			foreach (HitboxData hitboxData in this.fighterData.actions[this.currentActionID].GetHitboxData(this.currentActionFrame))
			{
				Hitbox hitbox = new Hitbox();
				hitbox.rect = this.TransformToFightRect(hitboxData.rect, this.position, this.isFaceRight);
				hitbox.proximity = hitboxData.proximity;
				hitbox.attackID = hitboxData.attackID;
				this.hitboxes.Add(hitbox);
			}
			foreach (HurtboxData hurtboxData in this.fighterData.actions[this.currentActionID].GetHurtboxData(this.currentActionFrame))
			{
				Hurtbox hurtbox = new Hurtbox();
				Rect dataRect = hurtboxData.useBaseRect ? this.fighterData.baseHurtBoxRect : hurtboxData.rect;
				hurtbox.rect = this.TransformToFightRect(dataRect, this.position, this.isFaceRight);
				this.hurtboxes.Add(hurtbox);
			}
			PushboxData pushboxData = this.fighterData.actions[this.currentActionID].GetPushboxData(this.currentActionFrame);
			this.pushbox = new Pushbox();
			Rect dataRect2 = pushboxData.useBaseRect ? this.fighterData.basePushBoxRect : pushboxData.rect;
			this.pushbox.rect = this.TransformToFightRect(dataRect2, this.position, this.isFaceRight);
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00008490 File Offset: 0x00006690
		private Rect TransformToFightRect(Rect dataRect, Vector2 basePosition, bool isFaceRight)
		{
			int num = isFaceRight ? 1 : -1;
			return new Rect
			{
				x = basePosition.x + dataRect.x * (float)num,
				y = basePosition.y + dataRect.y,
				width = dataRect.width,
				height = dataRect.height
			};
		}

		// Token: 0x0600016F RID: 367 RVA: 0x000084F8 File Offset: 0x000066F8
		public PlayerState getPlayerState()
		{
			PlayerState playerState = new PlayerState
			{
				PlayerPositionX = this.position.x,
				IsDead = this.isDead,
				VitalHealth = (long)this.vitalHealth,
				GuardHealth = (long)this.guardHealth,
				CurrentActionId = (long)this.currentActionID,
				CurrentActionFrame = (long)this.currentActionFrame,
				CurrentActionFrameCount = (long)this.currentActionFrameCount,
				IsActionEnd = this.isActionEnd,
				IsAlwaysCancelable = this.fighterData.actions[this.currentActionID].alwaysCancelable,
				CurrentActionHitCount = (long)this.currentActionHitCount,
				CurrentHitStunFrame = (long)this.currentHitStunFrame,
				IsInHitStun = this.isInHitStun,
				SpriteShakePosition = (long)this.spriteShakePosition,
				MaxSpriteShakeFrame = (long)this.maxSpriteShakeFrame,
				VelocityX = this.velocity_x,
				IsFaceRight = this.isFaceRight,
				CurrentFrameAdvantage = (long)this.currentFrameAdvantage,
				WouldNextForwardInputDash = this.WouldNextForwardInputDash(),
				WouldNextBackwardInputDash = this.WouldNextBackwardInputDash(),
				SpecialAttackProgress = this.GetSpecialAttackProgress()
			};
			for (int i = 0; i < this.input.Length; i++)
			{
				playerState.InputBuffer.Add((long)this.input[i]);
			}
			return playerState;
		}

		// Token: 0x040000F8 RID: 248
		public Vector2 position;

		// Token: 0x040000F9 RID: 249
		public float velocity_x;

		// Token: 0x040000FA RID: 250
		public bool isFaceRight;

		// Token: 0x040000FB RID: 251
		public List<Hitbox> hitboxes = new List<Hitbox>();

		// Token: 0x040000FC RID: 252
		public List<Hurtbox> hurtboxes = new List<Hurtbox>();

		// Token: 0x040000FD RID: 253
		public Pushbox pushbox;

		// Token: 0x040000FE RID: 254
		private FighterData fighterData;

		// Token: 0x04000106 RID: 262
		private static int inputRecordFrame = 180;

		// Token: 0x04000107 RID: 263
		private int[] input = new int[Fighter.inputRecordFrame];

		// Token: 0x04000108 RID: 264
		private int[] inputDown = new int[Fighter.inputRecordFrame];

		// Token: 0x04000109 RID: 265
		private int[] inputUp = new int[Fighter.inputRecordFrame];

		// Token: 0x0400010A RID: 266
		private bool isInputBackward;

		// Token: 0x0400010B RID: 267
		private bool isReserveProximityGuard;

		// Token: 0x0400010C RID: 268
		private int bufferActionID = -1;

		// Token: 0x0400010D RID: 269
		private int reserveDamageActionID = -1;

		// Token: 0x0400010F RID: 271
		private int maxSpriteShakeFrame = 6;

		// Token: 0x04000110 RID: 272
		private bool hasWon;
	}
}
