using Microsoft.Xna.Framework.Graphics;
using System;
namespace Microsoft.Xna.Framework.Content
{
	internal class Texture2DReader : ContentTypeReader<Texture2D>
	{
		internal Texture2DReader()
		{
			// Do nothing
		}

#if ANDROID
        static string[] supportedExtensions = new string[] { ".jpg", ".bmp", ".jpeg", ".png", ".gif" };
#else
		static string[] supportedExtensions = new string[] { ".jpg", ".bmp", ".jpeg", ".png", ".gif", ".pict", ".tga" };
#endif

		internal static string Normalize(string fileName)
		{
			return Normalize(fileName, supportedExtensions);
		}

		protected internal override Texture2D Read(ContentReader reader, Texture2D existingInstance)
		{
			Texture2D texture = null;

			SurfaceFormat surfaceFormat;
			if (reader.version < 5)
			{
				SurfaceFormat_Legacy legacyFormat = (SurfaceFormat_Legacy)reader.ReadInt32();
				switch (legacyFormat)
				{
					case SurfaceFormat_Legacy.Dxt1:
						surfaceFormat = SurfaceFormat.Dxt1;
						break;
					case SurfaceFormat_Legacy.Dxt3:
						surfaceFormat = SurfaceFormat.Dxt3;
						break;
					case SurfaceFormat_Legacy.Dxt5:
						surfaceFormat = SurfaceFormat.Dxt5;
						break;
					case SurfaceFormat_Legacy.Color:
						surfaceFormat = SurfaceFormat.Color;
						break;
					default:
						throw new NotSupportedException("Unsupported legacy surface format.");
				}
			}
			else
			{
				surfaceFormat = (SurfaceFormat)reader.ReadInt32();
			}

			int width = (reader.ReadInt32());
			int height = (reader.ReadInt32());
			int levelCount = (reader.ReadInt32());
			int levelCountOutput = levelCount;

			SurfaceFormat convertedFormat = surfaceFormat;
			switch (surfaceFormat)
			{
				case SurfaceFormat.Dxt1:
				case SurfaceFormat.Dxt1a:
					convertedFormat = SurfaceFormat.Color;
					break;
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
					convertedFormat = SurfaceFormat.Color;
					break;
				case SurfaceFormat.NormalizedByte4:
					convertedFormat = SurfaceFormat.Color;
					break;
			}

			UnityEngine.Texture2D unityTexture = new UnityEngine.Texture2D(width, height, XnaToUnity.TextureFormat(convertedFormat), levelCountOutput > 1);

			for (int level = 0; level < levelCount; level++)
			{
				int levelDataSizeInBytes = (reader.ReadInt32());
				byte[] levelData = reader.ReadBytes(levelDataSizeInBytes);
				int levelWidth = width >> level;
				int levelHeight = height >> level;

				if (level >= levelCountOutput)
				{
					continue;
				}

				//Convert the image data if required
				switch (surfaceFormat)
				{
					case SurfaceFormat.Dxt1:
					case SurfaceFormat.Dxt1a:
						levelData = DxtUtil.DecompressDxt1(levelData, levelWidth, levelHeight);
						break;
					case SurfaceFormat.Dxt3:
						levelData = DxtUtil.DecompressDxt3(levelData, levelWidth, levelHeight);
						break;
					case SurfaceFormat.Dxt5:
						levelData = DxtUtil.DecompressDxt5(levelData, levelWidth, levelHeight);
						break;
				}
				unityTexture.SetPixels(XnaToUnity.Color(levelData), level);
				unityTexture.Apply();
				texture = new Texture2D(unityTexture);
			}
			return texture;
		}
	}
}

