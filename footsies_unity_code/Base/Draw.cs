using System;
using UnityEngine;

namespace Footsies
{
	// Token: 0x0200002F RID: 47
	public class Draw
	{
		// Token: 0x0600017F RID: 383 RVA: 0x000086F8 File Offset: 0x000068F8
		public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
		{
			if (!Draw.lineTex)
			{
				Draw.lineTex = new Texture2D(1, 1);
			}
			Matrix4x4 matrix = GUI.matrix;
			Color color2 = GUI.color;
			GUI.color = color;
			Vector2 vector = pointB - pointA;
			GUIUtility.ScaleAroundPivot(new Vector2(vector.magnitude, width), Vector2.zero);
			GUIUtility.RotateAroundPivot(Vector2.Angle(vector, Vector2.right) * Mathf.Sign(vector.y), Vector2.zero);
			GUI.matrix = Matrix4x4.TRS(pointA, Quaternion.identity, Vector3.one) * GUI.matrix;
			GUI.DrawTexture(new Rect(Vector2.zero, Vector2.one), Draw.lineTex);
			GUI.matrix = matrix;
			GUI.color = color2;
		}

		// Token: 0x06000180 RID: 384 RVA: 0x0000387A File Offset: 0x00001A7A
		public static void DrawRect(Rect rect, Color color)
		{
			if (!Draw.rectText)
			{
				Draw.rectText = new Texture2D(1, 1);
			}
			Color color2 = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(rect, Draw.rectText);
			GUI.color = color2;
		}

		// Token: 0x0400010A RID: 266
		public static Texture2D lineTex;

		// Token: 0x0400010B RID: 267
		public static Texture2D rectText;
	}
}
