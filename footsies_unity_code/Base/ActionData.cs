using System;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
	// Token: 0x02000016 RID: 22
	[CreateAssetMenu]
	public class ActionData : ScriptableObject
	{
		// Token: 0x060000B7 RID: 183 RVA: 0x0000450C File Offset: 0x0000270C
		public MotionFrameData GetMotionData(int frame)
		{
			foreach (MotionFrameData motionFrameData in this.motions)
			{
				if (frame >= motionFrameData.startEndFrame.x && frame <= motionFrameData.startEndFrame.y)
				{
					return motionFrameData;
				}
			}
			return null;
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00004554 File Offset: 0x00002754
		public StatusData GetStatusData(int frame)
		{
			foreach (StatusData statusData in this.status)
			{
				if (frame >= statusData.startEndFrame.x && frame <= statusData.startEndFrame.y)
				{
					return statusData;
				}
			}
			return null;
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x0000459C File Offset: 0x0000279C
		public List<HitboxData> GetHitboxData(int frame)
		{
			List<HitboxData> list = new List<HitboxData>();
			foreach (HitboxData hitboxData in this.hitboxes)
			{
				if (frame >= hitboxData.startEndFrame.x && frame <= hitboxData.startEndFrame.y)
				{
					list.Add(hitboxData);
				}
			}
			return list;
		}

		// Token: 0x060000BA RID: 186 RVA: 0x000045EC File Offset: 0x000027EC
		public List<HurtboxData> GetHurtboxData(int frame)
		{
			List<HurtboxData> list = new List<HurtboxData>();
			foreach (HurtboxData hurtboxData in this.hurtboxes)
			{
				if (frame >= hurtboxData.startEndFrame.x && frame <= hurtboxData.startEndFrame.y)
				{
					list.Add(hurtboxData);
				}
			}
			return list;
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0000463C File Offset: 0x0000283C
		public PushboxData GetPushboxData(int frame)
		{
			foreach (PushboxData pushboxData in this.pushboxes)
			{
				if (frame >= pushboxData.startEndFrame.x && frame <= pushboxData.startEndFrame.y)
				{
					return pushboxData;
				}
			}
			return null;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00004684 File Offset: 0x00002884
		public MovementData GetMovementData(int frame)
		{
			foreach (MovementData movementData in this.movements)
			{
				if (frame >= movementData.startEndFrame.x && frame <= movementData.startEndFrame.y)
				{
					return movementData;
				}
			}
			return null;
		}

		// Token: 0x060000BD RID: 189 RVA: 0x000046CC File Offset: 0x000028CC
		public List<CancelData> GetCancelData(int frame)
		{
			List<CancelData> list = new List<CancelData>();
			foreach (CancelData cancelData in this.cancels)
			{
				if (frame >= cancelData.startEndFrame.x && frame <= cancelData.startEndFrame.y)
				{
					list.Add(cancelData);
				}
			}
			return list;
		}

		// Token: 0x0400006F RID: 111
		public int actionID;

		// Token: 0x04000070 RID: 112
		public string actionName;

		// Token: 0x04000071 RID: 113
		public ActionType Type;

		// Token: 0x04000072 RID: 114
		public int frameCount;

		// Token: 0x04000073 RID: 115
		public bool isLoop;

		// Token: 0x04000074 RID: 116
		public int loopFromFrame;

		// Token: 0x04000075 RID: 117
		public MotionFrameData[] motions;

		// Token: 0x04000076 RID: 118
		public StatusData[] status;

		// Token: 0x04000077 RID: 119
		public HitboxData[] hitboxes;

		// Token: 0x04000078 RID: 120
		public HurtboxData[] hurtboxes;

		// Token: 0x04000079 RID: 121
		public PushboxData[] pushboxes;

		// Token: 0x0400007A RID: 122
		public MovementData[] movements;

		// Token: 0x0400007B RID: 123
		public CancelData[] cancels;

		// Token: 0x0400007C RID: 124
		public bool alwaysCancelable;

		// Token: 0x0400007D RID: 125
		public AudioClip audioClip;
	}
}
