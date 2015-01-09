using System;
using UnityTexture = UnityEngine.Texture2D;

namespace Microsoft.Xna.Framework.Graphics
{
	public class Texture2D
	{
		internal readonly UnityEngine.Texture2D Texture;
		private bool isDisposed = false;

		public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
		{
			Texture = new UnityEngine.Texture2D(width, height, XnaToUnity.TextureFormat(format), mipmap);
		}

		public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
		{
			Texture = new UnityEngine.Texture2D(width, height);
		}

		public Texture2D(UnityEngine.Texture2D texture)
		{
			if (texture == null)
				throw new ArgumentNullException("texture");
			this.Texture = texture;
		}

		public void Dispose()
		{
			isDisposed = true;
		}

		public void GetData<T>(int level, Rectangle? rect, Color[] data, int startIndex, int elementCount) where T : struct
		{
			UnityEngine.Color[] output;
			// unity reads from bottom to top, so compensate for that on the Y position
			if (rect.HasValue)
				output = Texture.GetPixels(rect.Value.X, Texture.height - rect.Value.Y - rect.Value.Height, rect.Value.Width, rect.Value.Height, level);
			else
				output = Texture.GetPixels(level);

			UnityToXna.Color(output, ref data);
		}

		public void SetData<T>(Color[] data) where T : struct
		{
			UnityEngine.Color[] unityData = new UnityEngine.Color[data.Length];
			XnaToUnity.Color(data, ref unityData);
			Texture.SetPixels(unityData);
			Texture.Apply();
		}

		public void SetData<T>(Color[] data, int startIndex, int elementCount) where T : struct
		{
			
		}

		public void SetData(UnityEngine.Color[] data)
		{
			Texture.SetPixels(data);
			Texture.Apply();
		}

		public void SetData(UnityEngine.Color[] data, int mipLevel)
		{
			Texture.SetPixels(data, mipLevel);
			Texture.Apply();
		}

		public Rectangle Bounds { get { return new Rectangle(0, 0, Width, Height); } }
		public int Width { get { return Texture.width; } }
		public int Height { get { return Texture.height; } }
		public bool IsDisposed { get { return isDisposed; } }
	}
}
