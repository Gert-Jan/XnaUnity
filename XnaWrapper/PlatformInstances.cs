using XnaWrapper.PlatformInterfaces;

namespace XnaWrapper
{
	public static class PlatformInstances
	{
		public static bool IsEditor = true;

		public static bool InfoOverlay = false;
		public static string InfoOverlayText = "";

		public static bool LogOverlay = false;
		public static bool LogUp = false;
		public static bool LogDown = false;
		public static bool LogToBottom = false;

		public static IGamePad GamePad;
		public static IAssetLoadingInfo AssetLoadingInfo;
	}

}
