using System;
using Grpc.Core;
using UnityEngine;

namespace Footsies
{
	// Token: 0x0200003A RID: 58
	public class GrpcServerSingleton : Singleton<GrpcServerSingleton>
	{
		// Token: 0x060001D8 RID: 472 RVA: 0x00003BF5 File Offset: 0x00001DF5
		private void Awake()
		{
			if (GrpcServerSingleton.instance == null)
			{
				GrpcServerSingleton.instance = this;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				this.StartServer();
				return;
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x000099C4 File Offset: 0x00007BC4
		private void StartServer()
		{
			string text = "localhost";
			int num = 50051;
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i] == "--host" && i + 1 < commandLineArgs.Length)
				{
					text = commandLineArgs[i + 1];
				}
				int num2;
				if (commandLineArgs[i] == "--port" && i + 1 < commandLineArgs.Length && int.TryParse(commandLineArgs[i + 1], out num2))
				{
					num = num2;
				}
			}
			this.server = new Server
			{
				Services = 
				{
					FootsiesGameService.BindService(new FootsiesGameServiceImpl())
				},
				Ports = 
				{
					new ServerPort(text, num, ServerCredentials.Insecure)
				}
			};
			this.server.Start();
			Debug.Log(string.Format("gRPC server started on {0}:{1}", text, num));
		}

		// Token: 0x060001DA RID: 474 RVA: 0x00003C27 File Offset: 0x00001E27
		private void OnApplicationQuit()
		{
			this.OnDomainUnload(null, null);
		}

		// Token: 0x060001DB RID: 475 RVA: 0x00009A90 File Offset: 0x00007C90
		private void OnDomainUnload(object sender, EventArgs e)
		{
			if (this.server != null)
			{
				try
				{
					this.server.ShutdownAsync().Wait();
					Debug.Log("gRPC server shut down successfully.");
				}
				catch (Exception arg)
				{
					Debug.LogError(string.Format("Error shutting down gRPC server: {0}", arg));
				}
			}
		}

		// Token: 0x04000154 RID: 340
		private static GrpcServerSingleton instance;

		// Token: 0x04000155 RID: 341
		private Server server;
	}
}
