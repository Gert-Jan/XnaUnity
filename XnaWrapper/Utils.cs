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
		public static int NextPowerOf2(int x)
		{
			if (x < 0)
				return 0;
			--x;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			return x + 1;
		}
	}
}