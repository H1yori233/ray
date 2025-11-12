using System;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Footsies
{
	// Token: 0x02000038 RID: 56
	public class GameManager : Singleton<GameManager>
	{
		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060001CA RID: 458 RVA: 0x00003B0A File Offset: 0x00001D0A
		// (set) Token: 0x060001CB RID: 459 RVA: 0x00003B12 File Offset: 0x00001D12
		public GameManager.SceneIndex currentScene { get; private set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060001CC RID: 460 RVA: 0x00003B1B File Offset: 0x00001D1B
		// (set) Token: 0x060001CD RID: 461 RVA: 0x00003B23 File Offset: 0x00001D23
		public bool isVsCPU { get; private set; }

		// Token: 0x060001CE RID: 462 RVA: 0x00003B2C File Offset: 0x00001D2C
		private void Awake()
		{
			Debug.Log("GameManager Awake() called");
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			Application.targetFrameRate = 60;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x00003B4A File Offset: 0x00001D4A
		private void Start()
		{
			Debug.Log("GameManager Start() called");
			this.LoadTitleScene();
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00003B5C File Offset: 0x00001D5C
		private void Update()
		{
			if (this.currentScene == GameManager.SceneIndex.Battle && Input.GetButtonDown("Cancel"))
			{
				this.LoadTitleScene();
			}
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x00003B79 File Offset: 0x00001D79
		public void LoadTitleScene()
		{
			SceneManager.LoadScene(1);
			this.currentScene = GameManager.SceneIndex.Title;
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x00003B88 File Offset: 0x00001D88
		public void LoadVsPlayerScene()
		{
			this.isVsCPU = false;
			this.LoadBattleScene();
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00003B97 File Offset: 0x00001D97
		public void LoadVsCPUScene()
		{
			this.isVsCPU = true;
			this.LoadBattleScene();
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x00003BA6 File Offset: 0x00001DA6
		private void LoadBattleScene()
		{
			Debug.Log("LoadBattleScene() called");
			SceneManager.LoadScene(2);
			this.currentScene = GameManager.SceneIndex.Battle;
			if (this.menuSelectAudioClip != null)
			{
				Singleton<SoundManager>.Instance.playSE(this.menuSelectAudioClip);
			}
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x00003BDD File Offset: 0x00001DDD
		public void StartGame()
		{
			this.LoadVsPlayerScene();
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x00003BE5 File Offset: 0x00001DE5
		public void ResetGame()
		{
			this.LoadTitleScene();
		}

		// Token: 0x0400014D RID: 333
		public AudioClip menuSelectAudioClip;

		// Token: 0x04000150 RID: 336
		[SerializeField]
		public NNModel barracudaModel;

		// Token: 0x02000039 RID: 57
		public enum SceneIndex
		{
			// Token: 0x04000152 RID: 338
			Title = 1,
			// Token: 0x04000153 RID: 339
			Battle
		}
	}
}
