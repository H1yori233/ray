using System;
using UnityEngine;
using UnityEngine.UI;

namespace Footsies
{
	// Token: 0x0200004B RID: 75
	public class RoundPanelController : MonoBehaviour
	{
		// Token: 0x06000208 RID: 520 RVA: 0x0000A480 File Offset: 0x00008680
		private void Awake()
		{
			if (this._battleCoreGameObject != null)
			{
				this.battleCore = this._battleCoreGameObject.GetComponent<BattleCore>();
			}
			this.roundWonImages = new Image[this.roundWonImageObjects.Length];
			for (int i = 0; i < this.roundWonImageObjects.Length; i++)
			{
				this.roundWonImages[i] = this.roundWonImageObjects[i].GetComponent<Image>();
			}
			this.currentRoundWon = 0U;
			this.UpdateRoundWonImages();
		}

		// Token: 0x06000209 RID: 521 RVA: 0x00003DCC File Offset: 0x00001FCC
		private void Update()
		{
			if (this.currentRoundWon != this.getRoundWon())
			{
				this.currentRoundWon = this.getRoundWon();
				this.UpdateRoundWonImages();
			}
		}

		// Token: 0x0600020A RID: 522 RVA: 0x00003DEE File Offset: 0x00001FEE
		private uint getRoundWon()
		{
			if (this.battleCore == null)
			{
				return 0U;
			}
			if (this.isPlayerOne)
			{
				return this.battleCore.fighter1RoundWon;
			}
			return this.battleCore.fighter2RoundWon;
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000A4F4 File Offset: 0x000086F4
		private void UpdateRoundWonImages()
		{
			for (int i = 0; i < this.roundWonImages.Length; i++)
			{
				if (i <= (int)(this.currentRoundWon - 1U))
				{
					this.roundWonImages[i].sprite = this.spriteWon;
				}
				else
				{
					this.roundWonImages[i].sprite = this.spriteEmpty;
				}
			}
		}

		// Token: 0x04000184 RID: 388
		[SerializeField]
		private GameObject _battleCoreGameObject;

		// Token: 0x04000185 RID: 389
		[SerializeField]
		private Sprite spriteEmpty;

		// Token: 0x04000186 RID: 390
		[SerializeField]
		private Sprite spriteWon;

		// Token: 0x04000187 RID: 391
		[SerializeField]
		private bool isPlayerOne;

		// Token: 0x04000188 RID: 392
		[SerializeField]
		private GameObject[] roundWonImageObjects;

		// Token: 0x04000189 RID: 393
		private BattleCore battleCore;

		// Token: 0x0400018A RID: 394
		private Image[] roundWonImages;

		// Token: 0x0400018B RID: 395
		private uint currentRoundWon;
	}
}
