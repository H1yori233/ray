using System;

namespace Footsies
{
	// Token: 0x0200002E RID: 46
	public class InputData
	{
		// Token: 0x0600019B RID: 411 RVA: 0x00009185 File Offset: 0x00007385
		public InputData ShallowCopy()
		{
			return (InputData)base.MemberwiseClone();
		}

		// Token: 0x04000131 RID: 305
		public int input;

		// Token: 0x04000132 RID: 306
		public float time;
	}
}
