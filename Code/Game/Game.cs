// Copyright 2001-2016 Crytek GmbH / Crytek Group. All rights reserved.

using System;
using FairyGUI;
using CryEngine.Rendering;
using CryEngine.Common;
using CryEngine.Core;

namespace CryEngine.Game
{
	/// <summary>
	/// Basic sample of running a Mono Application.
	/// </summary>
	public class Game : IGameUpdateReceiver, IDisposable
	{
		private static Game _instance;

		private Game()
		{
			// The server doesn't support rendering UI and receiving input, so initializing those system is not required.
			if (Engine.IsDedicatedServer)
			{
				return;
			}


			Mouse.ShowCursor();

			GameFramework.RegisterForUpdate(this);

			Input.OnKey += OnKey;

			/*if(!Engine.IsSandbox)
			{
				Engine.Console.ExecuteString("map example", false, true);
			*/
			Engine.Console.ExecuteString("r_width 1136", false, true);
			Engine.Console.ExecuteString("r_height 640", false, true);
			
			UIConfig.defaultFont = "Microsoft YaHei";
			UIContentScaler.SetContentScaleFactor(1136, 640);
			GRoot.inst.AddChild(new MenuScene());
		}

		[CryEngine.Attributes.ConsoleCommand("open_chat_ui", 0, "")]
		void OpenChatUI()
		{
			GRoot.inst.RemoveChildren(0, -1, true);
			GRoot.inst.AddChild(new ChatScene());
		}

		public static void Initialize()
		{
			if (_instance == null)
			{
				_instance = new Game();
			}
		}

		public static void Shutdown()
		{
			_instance?.Dispose();
			_instance = null;
		}

		public void OnUpdate()
		{

		}

		public void Dispose()
		{
			if (Engine.IsDedicatedServer)
			{
				return;
			}

			Input.OnKey -= OnKey;
			GameFramework.UnregisterFromUpdate(this);
		}


		private void OnKey(InputEvent e)
		{
			if (e.KeyPressed(KeyId.Escape))
			{
				Engine.Shutdown();
			}
		}
	}
}
