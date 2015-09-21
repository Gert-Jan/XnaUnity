using System;
using System.Threading;
using System.Collections.Generic;
//using Xna = Microsoft.Xna.Framework;
using XnaSurfaceFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat;
using XVector2 = Microsoft.Xna.Framework.Vector2;
using UVector2 = UnityEngine.Vector2;
using XVector3 = Microsoft.Xna.Framework.Vector3;
using UVector3 = UnityEngine.Vector3;
using XMatrix = Microsoft.Xna.Framework.Matrix;
using UMatrix = UnityEngine.Matrix4x4;
using XColor = Microsoft.Xna.Framework.Color;
using UColor = UnityEngine.Color;
using UColor32 = UnityEngine.Color32;

namespace XnaWrapper
{
	public static class UnityToXna
	{
		public static XVector2 Vector2(UVector2 vector, out XVector2 output)
		{
			output.X = vector.x;
			output.Y = vector.y;
			return output;
		}

		public static XColor Color(UColor color, ref XColor output)
		{
			output.R = (byte)(color.r * 255);
			output.G = (byte)(color.g * 255);
			output.B = (byte)(color.b * 255);
			output.A = (byte)(color.a * 255);
			return output;
		}

		public static XColor[] Color(UColor[] input, ref XColor[] output)
		{
			for (int i = 0; i < input.Length && i < output.Length; i++)
				UnityToXna.Color(input[i], ref output[i]);
			return output;
		}

	}

	public static class XnaToUnity
	{
		public static UColor32 Color(XColor input, ref UColor32 output)
		{
			output.r = input.R;
			output.g = input.G;
			output.b = input.B;
			output.a = input.A;
			return output;
		}

		public static UVector2 Vector2(XVector2 input, ref UVector2 output)
		{
			output.x = input.X;
			output.y = input.Y;
			return output;
		}

		public static UVector3 Vector3(XVector3 input, ref UVector3 output)
		{
			output.x = input.X;
			output.y = input.Y;
			output.z = input.Z;
			return output;
		}

		public static UColor Color(XColor input, ref UColor output)
		{
			output.r = input.R / 255f;
			output.g = input.G / 255f;
			output.b = input.B / 255f;
			output.a = input.A / 255f;
			return output;
		}

		public static UColor[] Color(XColor[] input, ref UColor[] output)
		{
			for (int i = 0; i < input.Length && i < output.Length; i++)
			{
				XnaToUnity.Color(input[i], ref output[i]);
			}
			return output;
		}

		public static UColor[] Color(byte[] input, ref UColor[] output)
		{
			for (int i = 0; i < output.Length; i++)
			{
				output[i].r = input[i * 4 + 0] / 255f;
				output[i].g = input[i * 4 + 1] / 255f;
				output[i].b = input[i * 4 + 2] / 255f;
				output[i].a = input[i * 4 + 3] / 255f;
			}
			return output;
		}

		public static UnityEngine.Matrix4x4 Matrix(XMatrix input, out UMatrix output)
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
