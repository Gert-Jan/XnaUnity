namespace XnaUnity
{
	public static class DllPlatformCheck
	{
#if U_FUZE
		public const object U_FUZE = null;
#elif U_XBOXONE
		public const object U_XBOXONE = null;
#elif U_WINDOWS
		public const object U_WINDOWS = null;
#endif
	}
}
