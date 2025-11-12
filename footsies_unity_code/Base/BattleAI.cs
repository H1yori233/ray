using System;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
	// Token: 0x02000026 RID: 38
	public class BattleAI
	{
		// Token: 0x06000105 RID: 261 RVA: 0x00003465 File Offset: 0x00001665
		public BattleAI(BattleCore core)
		{
			this.battleCore = core;
		}

		// Token: 0x06000106 RID: 262 RVA: 0x000060F4 File Offset: 0x000042F4
		public int getNextAIInput()
		{
			int num = 0;
			this.UpdateFightState();
			BattleAI.FightState currentFightState = this.GetCurrentFightState();
			if (currentFightState != null)
			{
				if (this.moveQueue.Count > 0)
				{
					num |= this.moveQueue.Dequeue();
				}
				else if (this.moveQueue.Count == 0)
				{
					this.SelectMovement(currentFightState);
				}
				if (this.attackQueue.Count > 0)
				{
					num |= this.attackQueue.Dequeue();
				}
				else if (this.attackQueue.Count == 0)
				{
					this.SelectAttack(currentFightState);
				}
			}
			return num;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00006178 File Offset: 0x00004378
		private void SelectMovement(BattleAI.FightState fightState)
		{
			if (fightState.distanceX > 4f)
			{
				if (UnityEngine.Random.Range(0, 2) == 0)
				{
					this.AddFarApproach1();
					return;
				}
				this.AddFarApproach2();
				return;
			}
			else if (fightState.distanceX > 3f)
			{
				int num = UnityEngine.Random.Range(0, 7);
				if (num <= 1)
				{
					this.AddMidApproach1();
					return;
				}
				if (num <= 3)
				{
					this.AddMidApproach2();
					return;
				}
				if (num == 4)
				{
					this.AddFarApproach1();
					return;
				}
				if (num == 5)
				{
					this.AddFarApproach2();
					return;
				}
				this.AddNeutralMovement();
				return;
			}
			else if (fightState.distanceX > 2.5f)
			{
				int num2 = UnityEngine.Random.Range(0, 5);
				if (num2 == 0)
				{
					this.AddMidApproach1();
					return;
				}
				if (num2 == 1)
				{
					this.AddMidApproach2();
					return;
				}
				if (num2 == 2)
				{
					this.AddFallBack1();
					return;
				}
				if (num2 == 3)
				{
					this.AddFallBack2();
					return;
				}
				this.AddNeutralMovement();
				return;
			}
			else if (fightState.distanceX > 2f)
			{
				int num3 = UnityEngine.Random.Range(0, 4);
				if (num3 == 0)
				{
					this.AddFallBack1();
					return;
				}
				if (num3 == 1)
				{
					this.AddFallBack2();
					return;
				}
				this.AddNeutralMovement();
				return;
			}
			else
			{
				int num4 = UnityEngine.Random.Range(0, 3);
				if (num4 == 0)
				{
					this.AddFallBack1();
					return;
				}
				if (num4 == 1)
				{
					this.AddFallBack2();
					return;
				}
				this.AddNeutralMovement();
				return;
			}
		}

		// Token: 0x06000108 RID: 264 RVA: 0x0000628C File Offset: 0x0000448C
		private void SelectAttack(BattleAI.FightState fightState)
		{
			if (fightState.isOpponentDamage || fightState.isOpponentGuardBreak || fightState.isOpponentSpecialAttack)
			{
				this.AddTwoHitImmediateAttack();
				return;
			}
			if (fightState.distanceX > 4f)
			{
				if (UnityEngine.Random.Range(0, 4) <= 3)
				{
					this.AddNoAttack();
					return;
				}
				this.AddDelaySpecialAttack();
				return;
			}
			else if (fightState.distanceX > 3f)
			{
				if (fightState.isOpponentNormalAttack)
				{
					this.AddTwoHitImmediateAttack();
					return;
				}
				int num = UnityEngine.Random.Range(0, 5);
				if (num <= 1)
				{
					this.AddNoAttack();
					return;
				}
				if (num <= 3)
				{
					this.AddOneHitImmediateAttack();
					return;
				}
				this.AddDelaySpecialAttack();
				return;
			}
			else if (fightState.distanceX > 2.5f)
			{
				int num2 = UnityEngine.Random.Range(0, 3);
				if (num2 == 0)
				{
					this.AddNoAttack();
					return;
				}
				if (num2 == 1)
				{
					this.AddOneHitImmediateAttack();
					return;
				}
				this.AddTwoHitImmediateAttack();
				return;
			}
			else if (fightState.distanceX > 2f)
			{
				int num3 = UnityEngine.Random.Range(0, 6);
				if (num3 <= 1)
				{
					this.AddOneHitImmediateAttack();
					return;
				}
				if (num3 <= 3)
				{
					this.AddTwoHitImmediateAttack();
					return;
				}
				if (num3 == 4)
				{
					this.AddImmediateSpecialAttack();
					return;
				}
				this.AddDelaySpecialAttack();
				return;
			}
			else
			{
				if (UnityEngine.Random.Range(0, 3) == 0)
				{
					this.AddOneHitImmediateAttack();
					return;
				}
				this.AddTwoHitImmediateAttack();
				return;
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x000063A4 File Offset: 0x000045A4
		private void AddNeutralMovement()
		{
			for (int i = 0; i < 30; i++)
			{
				this.moveQueue.Enqueue(0);
			}
			Debug.Log("AddNeutral");
		}

		// Token: 0x0600010A RID: 266 RVA: 0x000034A1 File Offset: 0x000016A1
		private void AddFarApproach1()
		{
			this.AddForwardInputQueue(40);
			this.AddBackwardInputQueue(10);
			this.AddForwardInputQueue(30);
			this.AddBackwardInputQueue(10);
			Debug.Log("AddFarApproach1");
		}

		// Token: 0x0600010B RID: 267 RVA: 0x000034CD File Offset: 0x000016CD
		private void AddFarApproach2()
		{
			this.AddForwardDashInputQueue();
			this.AddBackwardInputQueue(25);
			this.AddForwardDashInputQueue();
			this.AddBackwardInputQueue(25);
			Debug.Log("AddFarApproach2");
		}

		// Token: 0x0600010C RID: 268 RVA: 0x000034F5 File Offset: 0x000016F5
		private void AddMidApproach1()
		{
			this.AddForwardInputQueue(30);
			this.AddBackwardInputQueue(10);
			this.AddForwardInputQueue(20);
			this.AddBackwardInputQueue(10);
			Debug.Log("AddMidApproach1");
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00003521 File Offset: 0x00001721
		private void AddMidApproach2()
		{
			this.AddForwardDashInputQueue();
			this.AddBackwardInputQueue(30);
			Debug.Log("AddMidApproach2");
		}

		// Token: 0x0600010E RID: 270 RVA: 0x0000353B File Offset: 0x0000173B
		private void AddFallBack1()
		{
			this.AddBackwardInputQueue(60);
			Debug.Log("AddFallBack1");
		}

		// Token: 0x0600010F RID: 271 RVA: 0x0000354F File Offset: 0x0000174F
		private void AddFallBack2()
		{
			this.AddBackwardDashInputQueue();
			this.AddBackwardInputQueue(60);
			Debug.Log("AddFallBack2");
		}

		// Token: 0x06000110 RID: 272 RVA: 0x000063D4 File Offset: 0x000045D4
		private void AddNoAttack()
		{
			for (int i = 0; i < 30; i++)
			{
				this.attackQueue.Enqueue(0);
			}
			Debug.Log("AddNoAttack");
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00006404 File Offset: 0x00004604
		private void AddOneHitImmediateAttack()
		{
			this.attackQueue.Enqueue(this.GetAttackInput());
			for (int i = 0; i < 18; i++)
			{
				this.attackQueue.Enqueue(0);
			}
			Debug.Log("AddOneHitImmediateAttack");
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00006448 File Offset: 0x00004648
		private void AddTwoHitImmediateAttack()
		{
			this.attackQueue.Enqueue(this.GetAttackInput());
			for (int i = 0; i < 3; i++)
			{
				this.attackQueue.Enqueue(0);
			}
			this.attackQueue.Enqueue(this.GetAttackInput());
			for (int j = 0; j < 18; j++)
			{
				this.attackQueue.Enqueue(0);
			}
			Debug.Log("AddTwoHitImmediateAttack");
		}

		// Token: 0x06000113 RID: 275 RVA: 0x000064B4 File Offset: 0x000046B4
		private void AddImmediateSpecialAttack()
		{
			for (int i = 0; i < 60; i++)
			{
				this.attackQueue.Enqueue(this.GetAttackInput());
			}
			this.attackQueue.Enqueue(0);
			Debug.Log("AddImmediateSpecialAttack");
		}

		// Token: 0x06000114 RID: 276 RVA: 0x000064F8 File Offset: 0x000046F8
		private void AddDelaySpecialAttack()
		{
			for (int i = 0; i < 120; i++)
			{
				this.attackQueue.Enqueue(this.GetAttackInput());
			}
			this.attackQueue.Enqueue(0);
			Debug.Log("AddDelaySpecialAttack");
		}

		// Token: 0x06000115 RID: 277 RVA: 0x0000653C File Offset: 0x0000473C
		private void AddForwardInputQueue(int frame)
		{
			for (int i = 0; i < frame; i++)
			{
				this.moveQueue.Enqueue(this.GetForwardInput());
			}
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00006568 File Offset: 0x00004768
		private void AddBackwardInputQueue(int frame)
		{
			for (int i = 0; i < frame; i++)
			{
				this.moveQueue.Enqueue(this.GetBackwardInput());
			}
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00003569 File Offset: 0x00001769
		private void AddForwardDashInputQueue()
		{
			this.moveQueue.Enqueue(this.GetForwardInput());
			this.moveQueue.Enqueue(0);
			this.moveQueue.Enqueue(this.GetForwardInput());
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00003569 File Offset: 0x00001769
		private void AddBackwardDashInputQueue()
		{
			this.moveQueue.Enqueue(this.GetForwardInput());
			this.moveQueue.Enqueue(0);
			this.moveQueue.Enqueue(this.GetForwardInput());
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00006594 File Offset: 0x00004794
		private void UpdateFightState()
		{
			BattleAI.FightState fightState = new BattleAI.FightState();
			fightState.distanceX = this.GetDistanceX();
			fightState.isOpponentDamage = (this.battleCore.fighter1.currentActionID == 200);
			fightState.isOpponentGuardBreak = (this.battleCore.fighter1.currentActionID == 310);
			fightState.isOpponentBlocking = (this.battleCore.fighter1.currentActionID == 306 || this.battleCore.fighter1.currentActionID == 305 || this.battleCore.fighter1.currentActionID == 301);
			fightState.isOpponentNormalAttack = (this.battleCore.fighter1.currentActionID == 100 || this.battleCore.fighter1.currentActionID == 105);
			fightState.isOpponentSpecialAttack = (this.battleCore.fighter1.currentActionID == 110 || this.battleCore.fighter1.currentActionID == 115);
			for (int i = 1; i < this.fightStates.Length; i++)
			{
				this.fightStates[i] = this.fightStates[i - 1];
			}
			this.fightStates[0] = fightState;
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00003599 File Offset: 0x00001799
		private BattleAI.FightState GetCurrentFightState()
		{
			return this.fightStates[this.fightStateReadIndex];
		}

		// Token: 0x0600011B RID: 283 RVA: 0x000035A8 File Offset: 0x000017A8
		private float GetDistanceX()
		{
			return Mathf.Abs(this.battleCore.fighter2.position.x - this.battleCore.fighter1.position.x);
		}

		// Token: 0x0600011C RID: 284 RVA: 0x000035DA File Offset: 0x000017DA
		private int GetAttackInput()
		{
			return 4;
		}

		// Token: 0x0600011D RID: 285 RVA: 0x000035DD File Offset: 0x000017DD
		private int GetForwardInput()
		{
			return 1;
		}

		// Token: 0x0600011E RID: 286 RVA: 0x000035E0 File Offset: 0x000017E0
		private int GetBackwardInput()
		{
			return 2;
		}

		// Token: 0x0400009C RID: 156
		private BattleCore battleCore;

		// Token: 0x0400009D RID: 157
		private Queue<int> moveQueue = new Queue<int>();

		// Token: 0x0400009E RID: 158
		private Queue<int> attackQueue = new Queue<int>();

		// Token: 0x0400009F RID: 159
		private BattleAI.FightState[] fightStates = new BattleAI.FightState[BattleAI.maxFightStateRecord];

		// Token: 0x040000A0 RID: 160
		public static readonly uint maxFightStateRecord = 10U;

		// Token: 0x040000A1 RID: 161
		private int fightStateReadIndex = 5;

		// Token: 0x02000027 RID: 39
		public class FightState
		{
			// Token: 0x040000A2 RID: 162
			public float distanceX;

			// Token: 0x040000A3 RID: 163
			public bool isOpponentDamage;

			// Token: 0x040000A4 RID: 164
			public bool isOpponentGuardBreak;

			// Token: 0x040000A5 RID: 165
			public bool isOpponentBlocking;

			// Token: 0x040000A6 RID: 166
			public bool isOpponentNormalAttack;

			// Token: 0x040000A7 RID: 167
			public bool isOpponentSpecialAttack;
		}
	}
}
