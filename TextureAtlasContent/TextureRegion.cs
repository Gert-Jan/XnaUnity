using Microsoft.Xna.Framework;

namespace TextureAtlasContent
{
	public struct TextureRegion
	{
		private string key;
		private Rectangle bounds;
		private Vector2 origin;
		private Vector2 originalSize;

		public string Key { get { return key; } }
		public Rectangle Bounds { get { return bounds; } }
		public Vector2 Origin { get { return origin; } }
		public Vector2 OriginalSize { get { return originalSize; } }

		public TextureRegion(ref string key, Rectangle bounds, Vector2 origin, Vector2 originalSize)
		{
			this.key = key;
			this.bounds = bounds;
			this.origin = origin;
			this.originalSize = originalSize;
		}
	}
}
