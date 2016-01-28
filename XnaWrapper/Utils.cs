using System;

namespace System
{
    public static class S_Path
    {
        public static string Combine(string[] parts)
        {
            string res = parts[0];
            for (int i = 1; i < parts.Length; ++i)
                res = IO.Path.Combine(res, parts[i]);
            return res;
        }
    }
}

namespace XnaWrapper
{
	public static class MathUtils
	{
		/// <summary>
		/// Returns the first number equal or higher than x, that is a power of 2
		/// Invalid if 
		/// </summary>
		public static int NextPowerOf2(int x)
		{
#if U_WINDOWS
			if (x < 1)
				throw new System.ArgumentException("zero or smaller is invalid (" + x + ")");
#endif
			--x;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			return x + 1;
		}

		/// <summary>
		/// Returns the zero-based index of the highest assigned bit of x
		/// 
		/// </summary>
		public static int HighestBitIndex(int x)
		{
#if U_WINDOWS
			if (x < 1)
				throw new System.ArgumentException("zero or smaller is invalid (" + x + ")");
#endif
			int bits = 0;
			for (int b = 16; b >= 1; b /= 2)
			{
				int s = 1 << b;
				if (x >= s) { x >>= b; bits += b; }
			}
			return bits;
		}
	}
}