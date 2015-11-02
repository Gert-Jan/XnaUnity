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
	}

}
