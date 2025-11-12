using System;

namespace Footsies
{
	// Token: 0x02000034 RID: 52
	public enum CommonActionID
	{
		// Token: 0x04000110 RID: 272
		STAND,
		// Token: 0x04000111 RID: 273
		FORWARD,
		// Token: 0x04000112 RID: 274
		BACKWARD,
		// Token: 0x04000113 RID: 275
		DASH_FORWARD = 10,
		// Token: 0x04000114 RID: 276
		DASH_BACKWARD,
		// Token: 0x04000115 RID: 277
		N_ATTACK = 100,
		// Token: 0x04000116 RID: 278
		B_ATTACK = 105,
		// Token: 0x04000117 RID: 279
		N_SPECIAL = 110,
		// Token: 0x04000118 RID: 280
		B_SPECIAL = 115,
		// Token: 0x04000119 RID: 281
		DAMAGE = 200,
		// Token: 0x0400011A RID: 282
		GUARD_M = 301,
		// Token: 0x0400011B RID: 283
		GUARD_STAND = 305,
		// Token: 0x0400011C RID: 284
		GUARD_CROUCH,
		// Token: 0x0400011D RID: 285
		GUARD_BREAK = 310,
		// Token: 0x0400011E RID: 286
		GUARD_PROXIMITY = 350,
		// Token: 0x0400011F RID: 287
		DEAD = 500,
		// Token: 0x04000120 RID: 288
		WIN = 510
	}
}
