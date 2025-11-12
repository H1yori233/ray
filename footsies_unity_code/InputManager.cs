using System;
using UnityEngine;

namespace Footsies
{
	// Token: 0x0200002F RID: 47
	public class InputManager : Singleton<InputManager>
	{
		// Token: 0x0600019D RID: 413 RVA: 0x0000919C File Offset: 0x0000739C
		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			for (int i = 0; i < this.gamePads.Length; i++)
			{
				this.gamePads[i] = new InputManager.GamePadHelper();
			}
		}

		// Token: 0x0600019E RID: 414 RVA: 0x000091D4 File Offset: 0x000073D4
		private void Update()
		{
		}

		// Token: 0x0600019F RID: 415 RVA: 0x000091D6 File Offset: 0x000073D6
		public bool GetButton(InputManager.Command command)
		{
			return Input.GetButton(this.GetInputName(command));
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x000091E4 File Offset: 0x000073E4
		public bool GetButtonDown(InputManager.Command command)
		{
			return Input.GetButtonDown(this.GetInputName(command));
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x000091F2 File Offset: 0x000073F2
		public bool GetButtonUp(InputManager.Command command)
		{
			return Input.GetButtonUp(this.GetInputName(command));
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x00009200 File Offset: 0x00007400
		private string GetInputName(InputManager.Command command)
		{
			switch (command)
			{
			case InputManager.Command.p1Left:
				return "P1_Left";
			case InputManager.Command.p1Right:
				return "P1_Right";
			case InputManager.Command.p1Attack:
				return "P1_Attack";
			case InputManager.Command.p2Left:
				return "P2_Left";
			case InputManager.Command.p2Right:
				return "P2_Right";
			case InputManager.Command.p2Attack:
				return "P2_Attack";
			case InputManager.Command.cancel:
				return "Cancel";
			default:
				return "";
			}
		}

		// Token: 0x04000133 RID: 307
		public InputManager.GamePadHelper[] gamePads = new InputManager.GamePadHelper[2];

		// Token: 0x04000134 RID: 308
		private int previousMenuInput;

		// Token: 0x04000135 RID: 309
		private int currentMenuInput;

		// Token: 0x04000136 RID: 310
		private float stickThreshold = 0.01f;

		// Token: 0x0200004E RID: 78
		public enum Command
		{
			// Token: 0x0400018E RID: 398
			p1Left,
			// Token: 0x0400018F RID: 399
			p1Right,
			// Token: 0x04000190 RID: 400
			p1Attack,
			// Token: 0x04000191 RID: 401
			p2Left,
			// Token: 0x04000192 RID: 402
			p2Right,
			// Token: 0x04000193 RID: 403
			p2Attack,
			// Token: 0x04000194 RID: 404
			cancel
		}

		// Token: 0x0200004F RID: 79
		public enum PadMenuInputState
		{
			// Token: 0x04000196 RID: 406
			Up,
			// Token: 0x04000197 RID: 407
			Down,
			// Token: 0x04000198 RID: 408
			Confirm
		}

		// Token: 0x02000050 RID: 80
		public class GamePadHelper
		{
			// Token: 0x04000199 RID: 409
			public bool isSet;
		}
	}
}
