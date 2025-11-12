using System;
using UnityEngine;
using UnityEngine.UI;

namespace Footsies
{
	// Token: 0x0200002D RID: 45
	public class BattleGUI : MonoBehaviour
	{
		// Token: 0x0600016B RID: 363 RVA: 0x00007E08 File Offset: 0x00006008
		private void Awake()
		{
			this.rectTransform = base.gameObject.GetComponent<RectTransform>();
			if (this._battleCoreGameObject != null)
			{
				this.battleCore = this._battleCoreGameObject.GetComponent<BattleCore>();
				BattleCore battleCore = this.battleCore;
				battleCore.damageHandler = (Action<Fighter, Vector2, DamageResult>)Delegate.Combine(battleCore.damageHandler, new Action<Fighter, Vector2, DamageResult>(this.OnDamageHandler));
			}
			if (this.fighter1ImageObject != null)
			{
				this.fighter1Image = this.fighter1ImageObject.GetComponent<Image>();
			}
			if (this.fighter2ImageObject != null)
			{
				this.fighter2Image = this.fighter2ImageObject.GetComponent<Image>();
			}
			if (this.hitEffectObject1 != null)
			{
				this.hitEffectAnimator1 = this.hitEffectObject1.GetComponent<Animator>();
			}
			if (this.hitEffectObject2 != null)
			{
				this.hitEffectAnimator2 = this.hitEffectObject2.GetComponent<Animator>();
			}
		}

		// Token: 0x0600016C RID: 364 RVA: 0x0000379B File Offset: 0x0000199B
		private void OnDestroy()
		{
			BattleCore battleCore = this.battleCore;
			battleCore.damageHandler = (Action<Fighter, Vector2, DamageResult>)Delegate.Remove(battleCore.damageHandler, new Action<Fighter, Vector2, DamageResult>(this.OnDamageHandler));
		}

		// Token: 0x0600016D RID: 365 RVA: 0x000037C4 File Offset: 0x000019C4
		private void FixedUpdate()
		{
			if (Input.GetKeyDown(KeyCode.F12))
			{
				this.drawDebug = !this.drawDebug;
			}
			if (this.battleCore.IsUsingGrpcController)
			{
				return;
			}
			this.UpdateLogic();
		}

		// Token: 0x0600016E RID: 366 RVA: 0x000037F5 File Offset: 0x000019F5
		public void ManualFixedUpdate()
		{
			this.UpdateLogic();
		}

		// Token: 0x0600016F RID: 367 RVA: 0x000037FD File Offset: 0x000019FD
		private void UpdateLogic()
		{
			this.CalculateBattleArea();
			this.CalculateFightPointToScreenScale();
			this.UpdateSprite();
		}

		// Token: 0x06000170 RID: 368 RVA: 0x00007EE8 File Offset: 0x000060E8
		private void OnGUI()
		{
			if (this.drawDebug)
			{
				this.battleCore.fighters.ForEach(delegate(Fighter f)
				{
					this.DrawFighter(f);
				});
				Rect position = new Rect((float)Screen.width * 0.4f, (float)Screen.height * 0.95f, (float)Screen.width * 0.2f, (float)Screen.height * 0.05f);
				this.debugTextStyle.alignment = TextAnchor.UpperCenter;
				GUI.Label(position, "F1=Pause/Resume, F2=Frame Step, F12=Debug Draw", this.debugTextStyle);
			}
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00007F6C File Offset: 0x0000616C
		private void UpdateSprite()
		{
			if (this.fighter1Image != null)
			{
				Sprite currentMotionSprite = this.battleCore.fighter1.GetCurrentMotionSprite();
				if (currentMotionSprite != null)
				{
					this.fighter1Image.sprite = currentMotionSprite;
				}
				Vector3 position = this.fighter1Image.transform.position;
				position.x = this.TransformHorizontalFightPointToScreen(this.battleCore.fighter1.position.x) + (float)this.battleCore.fighter1.spriteShakePosition;
				this.fighter1Image.transform.position = position;
			}
			if (this.fighter2Image != null)
			{
				Sprite currentMotionSprite2 = this.battleCore.fighter2.GetCurrentMotionSprite();
				if (currentMotionSprite2 != null)
				{
					this.fighter2Image.sprite = currentMotionSprite2;
				}
				Vector3 position2 = this.fighter2Image.transform.position;
				position2.x = this.TransformHorizontalFightPointToScreen(this.battleCore.fighter2.position.x) + (float)this.battleCore.fighter2.spriteShakePosition;
				this.fighter2Image.transform.position = position2;
			}
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00008090 File Offset: 0x00006290
		private void DrawFighter(Fighter fighter)
		{
			Rect position = new Rect(0f, (float)Screen.height * 0.86f, (float)Screen.width * 0.22f, 50f);
			if (fighter.isFaceRight)
			{
				position.x = (float)Screen.width * 0.01f;
				this.debugTextStyle.alignment = TextAnchor.UpperLeft;
			}
			else
			{
				position.x = (float)Screen.width * 0.77f;
				this.debugTextStyle.alignment = TextAnchor.UpperRight;
			}
			GUI.Label(position, fighter.position.ToString(), this.debugTextStyle);
			position.y += (float)Screen.height * 0.03f;
			int frameAdvantage = this.battleCore.GetFrameAdvantage(fighter.isFaceRight);
			string text = (frameAdvantage > 0) ? ("+" + frameAdvantage.ToString()) : frameAdvantage.ToString();
			GUI.Label(position, string.Concat(new string[]
			{
				"Frame: ",
				fighter.currentActionFrame.ToString(),
				"/",
				fighter.currentActionFrameCount.ToString(),
				"(",
				text,
				")"
			}), this.debugTextStyle);
			position.y += (float)Screen.height * 0.03f;
			GUI.Label(position, "Stun: " + fighter.currentHitStunFrame.ToString(), this.debugTextStyle);
			position.y += (float)Screen.height * 0.03f;
			GUI.Label(position, "Action: " + fighter.currentActionID.ToString() + " " + ((CommonActionID)fighter.currentActionID).ToString(), this.debugTextStyle);
			foreach (Hurtbox hurtbox in fighter.hurtboxes)
			{
				this.DrawFightBox(hurtbox.rect, Color.yellow, true);
			}
			if (fighter.pushbox != null)
			{
				this.DrawFightBox(fighter.pushbox.rect, Color.blue, true);
			}
			foreach (Hitbox hitbox in fighter.hitboxes)
			{
				if (hitbox.proximity)
				{
					this.DrawFightBox(hitbox.rect, Color.gray, true);
				}
				else
				{
					this.DrawFightBox(hitbox.rect, Color.red, true);
				}
			}
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000834C File Offset: 0x0000654C
		private void DrawFightBox(Rect fightPointRect, Color color, bool isFilled)
		{
			Rect rect = default(Rect);
			rect.width = fightPointRect.width * this.fightPointToScreenScale.x;
			rect.height = fightPointRect.height * this.fightPointToScreenScale.y;
			rect.x = this.TransformHorizontalFightPointToScreen(fightPointRect.x) - rect.width / 2f;
			rect.y = this.battleAreaBottomRightPoint.y - fightPointRect.y * this.fightPointToScreenScale.y - rect.height;
			this.DrawBox(rect, color, isFilled);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x000083F0 File Offset: 0x000065F0
		private void DrawBox(Rect rect, Color color, bool isFilled)
		{
			float x = rect.x;
			float y = rect.y;
			float width = rect.width;
			float height = rect.height;
			float x2 = x + width;
			float y2 = y + height;
			Draw.DrawLine(new Vector2(x, y), new Vector2(x2, y), color, this._battleBoxLineWidth);
			Draw.DrawLine(new Vector2(x, y), new Vector2(x, y2), color, this._battleBoxLineWidth);
			Draw.DrawLine(new Vector2(x2, y2), new Vector2(x2, y), color, this._battleBoxLineWidth);
			Draw.DrawLine(new Vector2(x2, y2), new Vector2(x, y2), color, this._battleBoxLineWidth);
			if (isFilled)
			{
				Color color2 = color;
				color2.a = 0.25f;
				Draw.DrawRect(new Rect(x, y, width, height), color2);
			}
		}

		// Token: 0x06000175 RID: 373 RVA: 0x00003811 File Offset: 0x00001A11
		private float TransformHorizontalFightPointToScreen(float x)
		{
			return x * this.fightPointToScreenScale.x + this.centerPoint;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00003827 File Offset: 0x00001A27
		private float TransformVerticalFightPointToScreen(float y)
		{
			return (float)Screen.height - this.battleAreaBottomRightPoint.y + y * this.fightPointToScreenScale.y;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x000084BC File Offset: 0x000066BC
		private void CalculateBattleArea()
		{
			Vector3[] array = new Vector3[4];
			this.rectTransform.GetWorldCorners(array);
			this.battleAreaTopLeftPoint = new Vector2(array[1].x, (float)Screen.height - array[1].y);
			this.battleAreaBottomRightPoint = new Vector2(array[3].x, (float)Screen.height - array[3].y);
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00008530 File Offset: 0x00006730
		private void CalculateFightPointToScreenScale()
		{
			this.fightPointToScreenScale.x = (this.battleAreaBottomRightPoint.x - this.battleAreaTopLeftPoint.x) / this.battleCore.battleAreaWidth;
			this.fightPointToScreenScale.y = (this.battleAreaBottomRightPoint.y - this.battleAreaTopLeftPoint.y) / this.battleCore.battleAreaMaxHeight;
			this.centerPoint = (this.battleAreaBottomRightPoint.x + this.battleAreaTopLeftPoint.x) / 2f;
		}

		// Token: 0x06000179 RID: 377 RVA: 0x000085BC File Offset: 0x000067BC
		private void OnDamageHandler(Fighter damagedFighter, Vector2 damagedPos, DamageResult damageResult)
		{
			if (damagedFighter == this.battleCore.fighter1)
			{
				this.fighter2Image.transform.SetAsLastSibling();
				this.RequestHitEffect(this.hitEffectAnimator1, damagedPos, damageResult);
				return;
			}
			if (damagedFighter == this.battleCore.fighter2)
			{
				this.fighter1Image.transform.SetAsLastSibling();
				this.RequestHitEffect(this.hitEffectAnimator2, damagedPos, damageResult);
			}
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00008624 File Offset: 0x00006824
		private void RequestHitEffect(Animator hitEffectAnimator, Vector2 damagedPos, DamageResult damageResult)
		{
			hitEffectAnimator.SetTrigger("Hit");
			Vector3 position = this.hitEffectAnimator2.transform.position;
			position.x = this.TransformHorizontalFightPointToScreen(damagedPos.x);
			position.y = this.TransformVerticalFightPointToScreen(damagedPos.y);
			hitEffectAnimator.transform.position = position;
			if (damageResult == DamageResult.GuardBreak)
			{
				hitEffectAnimator.transform.localScale = new Vector3(5f, 5f, 1f);
			}
			else if (damageResult == DamageResult.Damage)
			{
				hitEffectAnimator.transform.localScale = new Vector3(2f, 2f, 1f);
			}
			else if (damageResult == DamageResult.Guard)
			{
				hitEffectAnimator.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			hitEffectAnimator.transform.SetAsLastSibling();
		}

		// Token: 0x040000F8 RID: 248
		[SerializeField]
		private GameObject _battleCoreGameObject;

		// Token: 0x040000F9 RID: 249
		[SerializeField]
		private GameObject fighter1ImageObject;

		// Token: 0x040000FA RID: 250
		[SerializeField]
		private GameObject fighter2ImageObject;

		// Token: 0x040000FB RID: 251
		[SerializeField]
		private GameObject hitEffectObject1;

		// Token: 0x040000FC RID: 252
		[SerializeField]
		private GameObject hitEffectObject2;

		// Token: 0x040000FD RID: 253
		[SerializeField]
		private float _battleBoxLineWidth = 2f;

		// Token: 0x040000FE RID: 254
		[SerializeField]
		private GUIStyle debugTextStyle;

		// Token: 0x040000FF RID: 255
		[SerializeField]
		private bool drawDebug;

		// Token: 0x04000100 RID: 256
		private BattleCore battleCore;

		// Token: 0x04000101 RID: 257
		private Vector2 battleAreaTopLeftPoint;

		// Token: 0x04000102 RID: 258
		private Vector2 battleAreaBottomRightPoint;

		// Token: 0x04000103 RID: 259
		private Vector2 fightPointToScreenScale;

		// Token: 0x04000104 RID: 260
		private float centerPoint;

		// Token: 0x04000105 RID: 261
		private RectTransform rectTransform;

		// Token: 0x04000106 RID: 262
		private Image fighter1Image;

		// Token: 0x04000107 RID: 263
		private Image fighter2Image;

		// Token: 0x04000108 RID: 264
		private Animator hitEffectAnimator1;

		// Token: 0x04000109 RID: 265
		private Animator hitEffectAnimator2;
	}
}
