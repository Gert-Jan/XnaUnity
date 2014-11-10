namespace Microsoft.Xna.Framework
{
	public static class XnaToUnity
	{
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
	}
}
