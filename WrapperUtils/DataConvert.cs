using System;
using Xna = Microsoft.Xna.Framework;
using XnaSurfaceFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat;

namespace XnaWrapper
{
	public static class UnityToXna
	{
		public static void Vector2(UnityEngine.Vector2 vector, ref Xna.Vector2 output)
		{
			output.X = vector.x;
			output.Y = vector.y;
		}

		public static Xna.Color Color(UnityEngine.Color color)
		{
			return new Xna.Color(color.r, color.g, color.b, color.a);
		}

		public static void Color(UnityEngine.Color[] input, ref Xna.Color[] output)
		{
			for (int i = 0; i < input.Length && i < output.Length; i++)
			{
				output[i] = UnityToXna.Color(input[i]);
			}
		}

		public static void Color<T>(UnityEngine.Color[] input, ref T[] output)
		{
			for (int i = 0; i < input.Length && i < output.Length; i++)
			{
				output[i] = (T)(object)UnityToXna.Color(input[i]);
			}
		}
	}

	public static class XnaToUnity
	{
		public static void Color(Xna.Color color, ref UnityEngine.Color32 output)
		{
			output.r = color.R;
			output.g = color.G;
			output.b = color.B;
			output.a = color.A;
		}

		public static void Vector2(Xna.Vector2 vector, ref UnityEngine.Vector2 output)
		{
			output.x = vector.X;
			output.y = vector.Y;
		}

		public static void Vector3(Xna.Vector3 vector, ref UnityEngine.Vector3 output)
		{
			output.x = vector.X;
			output.y = vector.Y;
			output.z = vector.Z;
		}

		public static UnityEngine.Color Color(Xna.Color color)
		{
			return new UnityEngine.Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}

		public static void Color(Xna.Color[] input, ref UnityEngine.Color[] output)
		{
			for (int i = 0; i < input.Length && i < output.Length; i++)
			{
				output[i] = XnaToUnity.Color(input[i]);
			}
		}

		public static void Color<T>(T[] input, ref UnityEngine.Color[] output)
		{
			for (int i = 0; i < input.Length && i < output.Length; i++)
			{
				output[i] = XnaToUnity.Color((Xna.Color)(object)input[i]);
			}
		}

		public static UnityEngine.Color[] Color(byte[] input)
		{
			UnityEngine.Color[] output = new UnityEngine.Color[input.Length / 4];
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = new UnityEngine.Color(
					input[i * 4 + 0] / 255f,
					input[i * 4 + 1] / 255f,
					input[i * 4 + 2] / 255f,
					input[i * 4 + 3] / 255f);
			}
			return output;
		}

		public static UnityEngine.Matrix4x4 Matrix(Xna.Matrix input)
		{
			UnityEngine.Matrix4x4 output = new UnityEngine.Matrix4x4();
			return Matrix(input, out output);
		}

		public static UnityEngine.Matrix4x4 Matrix(Xna.Matrix input, out UnityEngine.Matrix4x4 output)
		{
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

		public static UnityEngine.TextureFormat TextureFormat(XnaSurfaceFormat surfaceFormat)
		{

			switch (surfaceFormat)
			{
				case XnaSurfaceFormat.Alpha8:
					return UnityEngine.TextureFormat.Alpha8;
				case XnaSurfaceFormat.Vector4:
					return UnityEngine.TextureFormat.ARGB32;
				case XnaSurfaceFormat.Bgra32:
					return UnityEngine.TextureFormat.BGRA32;
				case XnaSurfaceFormat.Dxt1:
					return UnityEngine.TextureFormat.DXT1;
				case XnaSurfaceFormat.Dxt5:
					return UnityEngine.TextureFormat.DXT5;
				case XnaSurfaceFormat.RgbEtc1:
					return UnityEngine.TextureFormat.ETC_RGB4;
				case XnaSurfaceFormat.RgbPvrtc2Bpp:
					return UnityEngine.TextureFormat.PVRTC_RGB2;
				case XnaSurfaceFormat.RgbaPvrtc2Bpp:
					return UnityEngine.TextureFormat.PVRTC_RGBA2;
				case XnaSurfaceFormat.RgbPvrtc4Bpp:
					return UnityEngine.TextureFormat.PVRTC_RGB4;
				case XnaSurfaceFormat.RgbaPvrtc4Bpp:
					return UnityEngine.TextureFormat.PVRTC_RGBA4;
				default:
				case XnaSurfaceFormat.Single:
				case XnaSurfaceFormat.Color:
					return UnityEngine.TextureFormat.RGBA32;
				case XnaSurfaceFormat.HalfSingle:
					return UnityEngine.TextureFormat.RGBA4444;
				case XnaSurfaceFormat.Bgr32:
				case XnaSurfaceFormat.Bgr565:
				case XnaSurfaceFormat.Bgra4444:
				case XnaSurfaceFormat.Bgra5551:
				case XnaSurfaceFormat.Dxt1a:
				case XnaSurfaceFormat.Dxt3:
				case XnaSurfaceFormat.HalfVector2:
				case XnaSurfaceFormat.HalfVector4:
				case XnaSurfaceFormat.HdrBlendable:
				case XnaSurfaceFormat.NormalizedByte2:
				case XnaSurfaceFormat.NormalizedByte4:
				case XnaSurfaceFormat.Rg32:
				case XnaSurfaceFormat.Rgba1010102:
				case XnaSurfaceFormat.Rgba64:
				case XnaSurfaceFormat.Vector2:
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
