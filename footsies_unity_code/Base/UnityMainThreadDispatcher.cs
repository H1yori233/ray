using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Footsies
{
	// Token: 0x02000037 RID: 55
	public class UnityMainThreadDispatcher : MonoBehaviour
	{
		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060001C4 RID: 452 RVA: 0x00009832 File Offset: 0x00007A32
		public static UnityMainThreadDispatcher Instance
		{
			get
			{
				if (UnityMainThreadDispatcher.instance == null)
				{
					GameObject gameObject = new GameObject("UnityMainThreadDispatcher");
					UnityMainThreadDispatcher.instance = gameObject.AddComponent<UnityMainThreadDispatcher>();
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
				return UnityMainThreadDispatcher.instance;
			}
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x00009860 File Offset: 0x00007A60
		private void Awake()
		{
			if (UnityMainThreadDispatcher.instance == null)
			{
				UnityMainThreadDispatcher.instance = this;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				return;
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000988C File Offset: 0x00007A8C
		private void Update()
		{
			Action action;
			while (UnityMainThreadDispatcher.actions.TryDequeue(out action))
			{
				action();
			}
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x000098AF File Offset: 0x00007AAF
		public void Enqueue(Action action)
		{
			UnityMainThreadDispatcher.actions.Enqueue(action);
		}

		// Token: 0x04000150 RID: 336
		private static readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

		// Token: 0x04000151 RID: 337
		private static UnityMainThreadDispatcher instance;
	}
}
