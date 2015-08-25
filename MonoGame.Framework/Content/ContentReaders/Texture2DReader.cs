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
                SurfaceFormatLegacy legacyFormat = (SurfaceFormatLegacy)reader.ReadInt32();
				switch (legacyFormat)
				{
                    case SurfaceFormatLegacy.Dxt1:
						surfaceFormat = SurfaceFormat.Dxt1;
						break;
                    case SurfaceFormatLegacy.Dxt3:
						surfaceFormat = SurfaceFormat.Dxt3;
						break;
                    case SurfaceFormatLegacy.Dxt5:
						surfaceFormat = SurfaceFormat.Dxt5;
						break;
                    case SurfaceFormatLegacy.Color:
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

				// un-premultiply alpha (instead do it in the shader)
				for (int i = 0; i < levelData.Length; i += 4)
				{
					float r = levelData[i + 0] / 255.0f;
					float g = levelData[i + 1] / 255.0f;
					float b = levelData[i + 2] / 255.0f;
					float a = levelData[i + 3] / 255.0f;

					levelData[i + 0] = (byte)(r / a * 255.0f);
					levelData[i + 1] = (byte)(g / a * 255.0f);
					levelData[i + 2] = (byte)(b / a * 255.0f);

					//levelData[i + 0] = 0;
					//levelData[i + 1] = 255;
					//levelData[i + 2] = 0;
					//levelData[i + 3] = 255;
				}

				// swap rows because unity textures are laid out bottom-top instead of top-bottom
				int rowSize = width * 4;
				byte[] temp = new byte[rowSize];
				for (int i = 0; i < levelData.Length / 2; i += rowSize)
				{
					for (int j = 0; j < rowSize; j++)
						temp[j] = levelData[i + j];
					int p = levelData.Length - (i + rowSize);
					for (int j = 0; j < rowSize; j++)
						levelData[i + j] = levelData[p + j];
					for (int j = 0; j < rowSize; j++)
 						levelData[p + j] = temp[j];
				}

				unityTexture.SetPixels(XnaToUnity.Color(levelData), level);
				unityTexture.Apply();
				texture = new Texture2D(unityTexture);
			}
			return texture;
		}
	}
}

