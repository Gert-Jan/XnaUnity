using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
	public class GraphicsDeviceManager
	{
		private GraphicsProfile graphicsProfile = Graphics.GraphicsProfile.HiDef;

		public GraphicsDeviceManager(Game game)
		{
			GraphicsDevice = game.GraphicsDevice;
		}

		public DisplayOrientation SupportedOrientations { get; set; }
		public bool IsFullScreen { get; set; }
		public int PreferredBackBufferHeight { get; set; }
		public int PreferredBackBufferWidth { get; set; }
		public GraphicsDevice GraphicsDevice { get; private set; }
		public GraphicsProfile GraphicsProfile 
		{ 
			get { return graphicsProfile; }
			set { graphicsProfile = value; }
		}

		public void ToggleFullScreen()
		{
			IsFullScreen = !IsFullScreen;
			ApplyChanges();
		}

		public void ApplyChanges()
		{
			UnityEngine.Screen.SetResolution(1920, 1080, true);
		}
	}
}
