using Microsoft.Xna.Framework.Graphics;
namespace Microsoft.Xna.Framework
{
	public static class XnaToUnity
	{
		public static UnityEngine.Color Color(Color color)
		{
			return new UnityEngine.Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}

		public static void Color(Color[] input, ref UnityEngine.Color[] output)
		{
			for (int i = 0; i < input.Length && i < output.Length; i++)
			{
				output[i] = XnaToUnity.Color(input[i]);
			}
		}

		public static UnityEngine.Matrix4x4 Matrix(Xna.Framework.Matrix input)
		{
			UnityEngine.Matrix4x4 output = new UnityEngine.Matrix4x4();
			output.m00 = input.M11;
			output.m01 = input.M21;
			output.m02 = input.M31;
			output.m03 = input.M41;
			output.m10 = input.M12;
			output.m11 = input.M22;
			output.m12 = input.M32;
			output.m13 = input.M42;
			output.m20 = input.M13;
			output.m21 = input.M23;
			output.m22 = input.M33;
			output.m23 = input.M43;
			output.m30 = input.M14;
			output.m31 = input.M24;
			output.m32 = input.M34;
			output.m33 = input.M44;
			return output;
		}

		public static UnityEngine.TextureFormat TextureFormat(SurfaceFormat surfaceFormat)
		{
			switch (surfaceFormat)
			{
				case SurfaceFormat.Alpha8:
					return UnityEngine.TextureFormat.Alpha8;
				case SurfaceFormat.Vector4:
					return UnityEngine.TextureFormat.ARGB32;
				case SurfaceFormat.Bgra32:
					return UnityEngine.TextureFormat.BGRA32;
				case SurfaceFormat.Dxt1:
					return UnityEngine.TextureFormat.DXT1;
				case SurfaceFormat.Dxt5:
					return UnityEngine.TextureFormat.DXT5;
				case SurfaceFormat.RgbEtc1:
					return UnityEngine.TextureFormat.ETC_RGB4;
				case SurfaceFormat.RgbPvrtc2Bpp:
					return UnityEngine.TextureFormat.PVRTC_RGB2;
				case SurfaceFormat.RgbaPvrtc2Bpp:
					return UnityEngine.TextureFormat.PVRTC_RGBA2;
				case SurfaceFormat.RgbPvrtc4Bpp:
					return UnityEngine.TextureFormat.PVRTC_RGB4;
				case SurfaceFormat.RgbaPvrtc4Bpp:
					return UnityEngine.TextureFormat.PVRTC_RGBA4;
				default:
				case SurfaceFormat.Single:
				case SurfaceFormat.Color:
					return UnityEngine.TextureFormat.RGBA32;
				case SurfaceFormat.HalfSingle:
					return UnityEngine.TextureFormat.RGBA4444;
				case SurfaceFormat.Bgr32:
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.Dxt1a:
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.HdrBlendable:
				case SurfaceFormat.NormalizedByte2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
					return UnityEngine.TextureFormat.RGBA32;
					/*
					return UnityEngine.TextureFormat.ARGB4444;
					return UnityEngine.TextureFormat.ATC_RGB4;
					return UnityEngine.TextureFormat.ATC_RGBA8;
					return UnityEngine.TextureFormat.ATF_RGB_DXT1;
					return UnityEngine.TextureFormat.ATF_RGB_JPG;
					return UnityEngine.TextureFormat.ATF_RGBA_JPG;
					return UnityEngine.TextureFormat.PVRTC_RGB2;
					return UnityEngine.TextureFormat.PVRTC_RGB4;
					return UnityEngine.TextureFormat.PVRTC_RGBA2;
					return UnityEngine.TextureFormat.PVRTC_RGBA4;
					return UnityEngine.TextureFormat.RGB24;
					return UnityEngine.TextureFormat.RGB565;*/
			}
		}
	}
}
