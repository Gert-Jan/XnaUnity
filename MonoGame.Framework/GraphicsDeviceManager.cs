﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
	public class GraphicsDeviceManager
	{
		public GraphicsDeviceManager(Game game)
		{
			GraphicsDevice = game.GraphicsDevice;
		}

		public DisplayOrientation SupportedOrientations { get; set; }
		public bool IsFullScreen { get; set; }
		public int PreferredBackBufferHeight { get; set; }
		public int PreferredBackBufferWidth { get; set; }
		public GraphicsDevice GraphicsDevice { get; private set; }
		public GraphicsProfile GraphicsProfile { get { return Graphics.GraphicsProfile.HiDef; } }

		public void ToggleFullScreen()
		{
			IsFullScreen = !IsFullScreen;
			ApplyChanges();
		}

		public void ApplyChanges()
		{
			UnityEngine.Screen.SetResolution(PreferredBackBufferWidth, PreferredBackBufferHeight, IsFullScreen);
		}
	}
}
