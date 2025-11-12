using System;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace Footsies
{
	// Token: 0x02000028 RID: 40
	public class BattleAIBarracuda
	{
		// Token: 0x06000121 RID: 289 RVA: 0x000066CC File Offset: 0x000048CC
		public BattleAIBarracuda(BattleCore core)
		{
			this.battleCore = core;
			this.encoder = new AIEncoder();
			this.lastHiddenStates[true] = new Tensor(1, 128, "");
			this.lastHiddenStates[false] = new Tensor(1, 128, "");
			this.lastCellStates[true] = new Tensor(1, 128, "");
			this.lastCellStates[false] = new Tensor(1, 128, "");
			this.specialChargeQueue[true] = 0;
			this.specialChargeQueue[false] = 0;
			this.actionQueue[true] = new Queue<int>();
			this.actionQueue[false] = new Queue<int>();
		}

		// Token: 0x06000122 RID: 290 RVA: 0x000067CC File Offset: 0x000049CC
		public void Initialize(NNModel model)
		{
			if (model == null)
			{
				throw new ArgumentNullException("Model asset cannot be null");
			}
			this.modelAsset = model;
			this.m_RuntimeModel = ModelLoader.Load(this.modelAsset, false, false);
			this.worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, this.m_RuntimeModel, null, null, false);
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000123 RID: 291 RVA: 0x000035EC File Offset: 0x000017EC
		public bool IsInitialized
		{
			get
			{
				return this.m_RuntimeModel != null;
			}
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00006820 File Offset: 0x00004A20
		public Tensor encodeGameState(GameState gameState, bool isPlayer1)
		{
			float[] srcData = this.encoder.EncodeGameState(gameState, isPlayer1);
			return new Tensor(1, AIEncoder.ObservationSize, srcData, "");
		}

		// Token: 0x06000125 RID: 293 RVA: 0x0000684C File Offset: 0x00004A4C
		public int getNextAIInput(bool isPlayer1)
		{
			if (this.actionQueue[isPlayer1].Count > 0)
			{
				return this.actionQueue[isPlayer1].Dequeue();
			}
			GameState gameState = this.battleCore.GetGameState();
			Dictionary<string, Tensor> dictionary = new Dictionary<string, Tensor>();
			Tensor value = this.encodeGameState(gameState, isPlayer1);
			dictionary["obs"] = value;
			if (this.lastHiddenStates.ContainsKey(isPlayer1) && this.lastCellStates.ContainsKey(isPlayer1))
			{
				dictionary["state_in_0"] = this.lastCellStates[isPlayer1];
				dictionary["state_in_1"] = this.lastHiddenStates[isPlayer1];
			}
			else
			{
				dictionary["state_in_0"] = new Tensor(1, 128, "");
				dictionary["state_in_1"] = new Tensor(1, 128, "");
			}
			dictionary["seq_lens"] = new Tensor(new int[]
			{
				1
			}, new float[]
			{
				1f
			}, "", false);
			IWorker worker = this.worker.Execute(dictionary);
			foreach (Tensor tensor in dictionary.Values)
			{
				tensor.Dispose();
			}
			this.lastCellStates[isPlayer1].Dispose();
			this.lastHiddenStates[isPlayer1].Dispose();
			float[] array = worker.PeekOutput("output").AsFloats();
			this.lastCellStates[isPlayer1] = worker.CopyOutput("state_out_0");
			this.lastHiddenStates[isPlayer1] = worker.CopyOutput("state_out_1");
			float num = 0f;
			float[] array2 = new float[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Mathf.Exp(array[i]);
				num += array2[i];
			}
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] /= num;
			}
			int num2 = 0;
			float value2 = UnityEngine.Random.value;
			float num3 = 0f;
			for (int k = 0; k < array2.Length; k++)
			{
				num3 += array2[k];
				if (value2 <= num3)
				{
					num2 = k;
					break;
				}
			}
			bool flag = this.specialChargeQueue[isPlayer1] <= 0;
			if (num2 == 6 && flag)
			{
				this.specialChargeQueue[isPlayer1] = 15;
			}
			int num4;
			switch (num2)
			{
			case 0:
				num4 = 0;
				break;
			case 1:
				num4 = 1;
				break;
			case 2:
				num4 = 2;
				break;
			case 3:
				num4 = 4;
				break;
			case 4:
				num4 = 5;
				break;
			case 5:
				num4 = 6;
				break;
			case 6:
				num4 = 0;
				break;
			default:
				num4 = 0;
				break;
			}
			if (this.specialChargeQueue[isPlayer1] > 0)
			{
				Dictionary<bool, int> dictionary2 = this.specialChargeQueue;
				dictionary2[isPlayer1] -= 4;
				num4 |= 4;
			}
			for (int l = 0; l < 4; l++)
			{
				this.actionQueue[isPlayer1].Enqueue(num4);
			}
			return this.actionQueue[isPlayer1].Dequeue();
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00006B84 File Offset: 0x00004D84
		public void Dispose()
		{
			if (this.worker != null)
			{
				this.worker.Dispose();
				this.worker = null;
			}
			foreach (Tensor tensor in this.lastHiddenStates.Values)
			{
				tensor.Dispose();
			}
			this.lastHiddenStates.Clear();
			foreach (Tensor tensor2 in this.lastCellStates.Values)
			{
				tensor2.Dispose();
			}
			this.lastCellStates.Clear();
		}

		// Token: 0x040000A8 RID: 168
		private BattleCore battleCore;

		// Token: 0x040000A9 RID: 169
		private AIEncoder encoder;

		// Token: 0x040000AA RID: 170
		public NNModel modelAsset;

		// Token: 0x040000AB RID: 171
		private Model m_RuntimeModel;

		// Token: 0x040000AC RID: 172
		private IWorker worker;

		// Token: 0x040000AD RID: 173
		private Dictionary<bool, Tensor> lastHiddenStates = new Dictionary<bool, Tensor>();

		// Token: 0x040000AE RID: 174
		private Dictionary<bool, Tensor> lastCellStates = new Dictionary<bool, Tensor>();

		// Token: 0x040000AF RID: 175
		private const int STATE_SIZE = 128;

		// Token: 0x040000B0 RID: 176
		private const int FRAME_SKIP = 4;

		// Token: 0x040000B1 RID: 177
		public Dictionary<bool, int> specialChargeQueue = new Dictionary<bool, int>();

		// Token: 0x040000B2 RID: 178
		private const int SPECIAL_CHARGE_DURATION = 15;

		// Token: 0x040000B3 RID: 179
		private Dictionary<bool, Queue<int>> actionQueue = new Dictionary<bool, Queue<int>>();
	}
}
