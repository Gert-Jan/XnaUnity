using System;
//using System.Collections.Generic;
using XnaWrapper;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture2D
	{
		public UnityEngine.Texture2D UnityTexture2D
		{
			get { return (UnityEngine.Texture2D)texture; }
			private set { texture = value; }
		}

		public Texture2D(UnityEngine.Texture texture)
			: base(texture)
		{ }

		public Texture2D(UnityEngine.Texture2D texture)
			: base(texture)
		{
			this.width = texture.width;
			this.height = texture.height;
			this._levelCount = texture.mipmapCount;
		}

		/**
		 * Extra function for Unity so that the original texture settings can be copied to the new texture
		 */
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height, UnityEngine.Texture2D original)
			: this(graphicsDevice, width, height, false, SurfaceFormat.Color, SurfaceType.Texture, false, original)
		{
		}

		protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, UnityEngine.Texture2D original)
			: this(graphicsDevice, width, height, mipmap, format, type, false)
		{
			if (original != null)
			{
				// Copy the wrap mode (clamp, repeat) and filter mode (point, bilinair, trilinair) from the original texture
				UnityTexture2D.wrapMode = original.wrapMode;
				UnityTexture2D.filterMode = original.filterMode;
			}
		}

		private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
		{
			UnityTexture2D = new UnityEngine.Texture2D(width, height, XnaToUnity.TextureFormat(format), mipmap);
		}

		private void PlatformSetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		{
			UnityEngine.Color[] unityData = new UnityEngine.Color[data.Length];
			Color[] xnaData = (Color[])(object)data;
			XnaToUnity.Color(xnaData, ref unityData);
			if (rect.HasValue)
			{
				UnityTexture2D.SetPixels(rect.Value.X, UnityTexture2D.height - rect.Value.Y - rect.Value.Height, rect.Value.Width, rect.Value.Height, unityData);
			}
			else
			{
				UnityTexture2D.SetPixels(unityData);
			}
			UnityTexture2D.Apply();
		}

		public void SetData(UnityEngine.Color[] data)
		{
			UnityTexture2D.SetPixels(data);
			UnityTexture2D.Apply();
		}

		public void SetData(UnityEngine.Color[] data, int mipLevel)
		{
			UnityTexture2D.SetPixels(data, mipLevel);
			UnityTexture2D.Apply();
		}

		private void PlatformGetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		{
			UnityEngine.Color[] output;
			// unity reads from bottom to top, so compensate for that on the Y position
			if (rect.HasValue)
				output = UnityTexture2D.GetPixels(rect.Value.X, UnityTexture2D.height - rect.Value.Y - rect.Value.Height, rect.Value.Width, rect.Value.Height, level);
			else
				output = UnityTexture2D.GetPixels(level);

			Color[] xnaData = (Color[])(object)data;
			UnityToXna.Color(output, ref xnaData);
		}

		private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
		{
			UnityEngine.Texture2D unityTexture = new UnityEngine.Texture2D(0, 0);
			BinaryReader br = new BinaryReader(stream);
			byte[] data = br.ReadBytes((int)stream.Length);
			unityTexture.LoadImage(data);
			return new Texture2D(unityTexture);
		}

		public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream, int width, int height, bool zoom)
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
