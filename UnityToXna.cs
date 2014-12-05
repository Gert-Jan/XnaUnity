using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
	public static class UnityToXna
	{
		public static Color Color(UnityEngine.Color color)
		{
			return new Color(color.r, color.g, color.b, color.a);
		}

		public static void Color(UnityEngine.Color[] input, ref Color[] output)
		{
			for (int i = 0; i < input.Length && i < output.Length; i++)
			{
				output[i] = UnityToXna.Color(input[i]);
			}
		}
	}
}
