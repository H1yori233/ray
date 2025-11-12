using System;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
	// Token: 0x02000028 RID: 40
	[CreateAssetMenu]
	public class FighterData : ScriptableObject
	{
		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000172 RID: 370 RVA: 0x000086C2 File Offset: 0x000068C2
		public Dictionary<int, ActionData> actions
		{
			get
			{
				return this._actions;
			}
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000173 RID: 371 RVA: 0x000086CA File Offset: 0x000068CA
		public Dictionary<int, AttackData> attackData
		{
			get
			{
				return this._attackData;
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000174 RID: 372 RVA: 0x000086D2 File Offset: 0x000068D2
		public Dictionary<int, MotionData> motionData
		{
			get
			{
				return this._motionData;
			}
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000086DC File Offset: 0x000068DC
		public void setupDictionary()
		{
			if (this.actionDataContainer == null)
			{
				Debug.LogError("ActionDataContainer is not set");
				return;
			}
			if (this.attackDataContainer == null)
			{
				Debug.LogError("ActionDataContainer is not set");
				return;
			}
			this._actions = new Dictionary<int, ActionData>();
			foreach (ActionData actionData in this.actionDataContainer.actions)
			{
				this._actions.Add(actionData.actionID, actionData);
			}
			this._attackData = new Dictionary<int, AttackData>();
			foreach (AttackData attackData in this.attackDataContainer.attackDataList)
			{
				this._attackData.Add(attackData.attackID, attackData);
			}
			this._motionData = new Dictionary<int, MotionData>();
			foreach (MotionData motionData in this.motionDataContainer.motionDataList)
			{
				this._motionData.Add(motionData.motionID, motionData);
			}
		}

		// Token: 0x04000111 RID: 273
		public int startGuardHealth = 3;

		// Token: 0x04000112 RID: 274
		public float forwardMoveSpeed = 2.2f;

		// Token: 0x04000113 RID: 275
		public float backwardMoveSpeed = 1.8f;

		// Token: 0x04000114 RID: 276
		public int dashAllowFrame = 10;

		// Token: 0x04000115 RID: 277
		public int specialAttackHoldFrame = 60;

		// Token: 0x04000116 RID: 278
		public bool canCancelOnWhiff;

		// Token: 0x04000117 RID: 279
		[SerializeField]
		public Rect baseHurtBoxRect;

		// Token: 0x04000118 RID: 280
		[SerializeField]
		public Rect basePushBoxRect;

		// Token: 0x04000119 RID: 281
		[SerializeField]
		private ActionDataContainer actionDataContainer;

		// Token: 0x0400011A RID: 282
		[SerializeField]
		private AttackDataContainer attackDataContainer;

		// Token: 0x0400011B RID: 283
		[SerializeField]
		private MotionDataContainer motionDataContainer;

		// Token: 0x0400011C RID: 284
		private Dictionary<int, ActionData> _actions = new Dictionary<int, ActionData>();

		// Token: 0x0400011D RID: 285
		private Dictionary<int, AttackData> _attackData = new Dictionary<int, AttackData>();

		// Token: 0x0400011E RID: 286
		private Dictionary<int, MotionData> _motionData = new Dictionary<int, MotionData>();
	}
}
