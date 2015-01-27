using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture2D
	{
		private UnityEngine.Texture2D texture;

		public UnityEngine.Texture2D Texture
		{
			get { return texture; }
			private set { texture = value; }
		}
		public Texture2D(UnityEngine.Texture2D texture)
		{
			if (texture == null)
				throw new ArgumentNullException("texture");
			this.Texture = texture;
			this.width = texture.width;
			this.height = texture.height;
			this._format = SurfaceFormat.Color;
			this._levelCount = texture.mipmapCount;
		}

		private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
		{
			Texture = new UnityEngine.Texture2D(width, height, XnaToUnity.TextureFormat(format), mipmap);
		}

		private void PlatformSetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		{
			UnityEngine.Color[] unityData = new UnityEngine.Color[data.Length];
			XnaToUnity.Color<T>(data, ref unityData);
			Texture.SetPixels(unityData);
			Texture.Apply();
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

		private void PlatformGetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		{
			UnityEngine.Color[] output;
			// unity reads from bottom to top, so compensate for that on the Y position
			if (rect.HasValue)
				output = Texture.GetPixels(rect.Value.X, Texture.height - rect.Value.Y - rect.Value.Height, rect.Value.Width, rect.Value.Height, level);
			else
				output = Texture.GetPixels(level);

			UnityToXna.Color<T>(output, ref data);
		}

		private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
		{
			throw new NotImplementedException();
		}

		private void PlatformSaveAsJpeg(Stream stream, int width, int height)
		{
			throw new NotImplementedException();
		}

		private void PlatformSaveAsPng(Stream stream, int width, int height)
		{
			throw new NotImplementedException();
		}

		private void PlatformReload(Stream textureStream)
		{
			throw new NotImplementedException();
		}
	}
}
