using System;
using System.Collections.Generic;

namespace Footsies
{
	// Token: 0x0200001F RID: 31
	[Serializable]
	public class CancelData : FrameDataBase
	{
		// Token: 0x04000074 RID: 116
		public bool buffer;

		// Token: 0x04000075 RID: 117
		public bool execute;

		// Token: 0x04000076 RID: 118
		public List<int> actionID = new List<int>();
	}
}
