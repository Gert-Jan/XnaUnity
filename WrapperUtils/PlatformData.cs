using XnaWrapper.PlatformInterfaces;

namespace XnaWrapper
{
	public enum PlatformID
	{
		XboxOne,
		Fuze, 
		Windows, 
		Unknown
	}

	public static class PlatformData
	{
		public static bool IsEditor;
		public static PlatformID ActivePlatform;

		public static XnaGamePad GamePad;
		public static XnaAssetProvider AssetProvider;

		private static PlatformID dllPlatform =
#if U_FUZE
			PlatformID.Fuze;
#elif U_XBOXONE
			PlatformID.XboxOne;
#elif U_WINDOWS
			PlatformID.Windows;
#else
			PlatformID.Unknown;
#endif
		public static PlatformID DllPlatform { get { return dllPlatform; } }

	}
}
